using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : MonoBehaviour
{
    private const int MaximumVisits = 6;
    private const int MaximumItemsPerStep = 5;
    private const string VisitCountPrefix = "SceneVisitCountByPath_v2_";
    private const string SceneNameSuffix = "_SceneName";
    private const string ScenePathSuffix = "_ScenePath";
    private const string VisitCountRegistryKey = "SceneVisitCountByPath_v2_Registry";
    private const string LegacyVisitCountPrefix = "SceneVisitCount_";

    [Header("Objects hidden by visit")]
    public List<GameObject> firstVisitHideObjects = new List<GameObject>();
    public List<GameObject> secondVisitHideObjects = new List<GameObject>();
    public List<GameObject> thirdVisitHideObjects = new List<GameObject>();
    public List<GameObject> fourthVisitHideObjects = new List<GameObject>();
    public List<GameObject> fifthVisitHideObjects = new List<GameObject>();
    public List<GameObject> sixthVisitHideObjects = new List<GameObject>();

    [Header("First Visit - Narrator audio")]
    public AudioClip narratorAudio1;
    public AudioClip narratorAudio2;
    public AudioClip narratorAudio3;

    [Header("First Visit - Item lists")]
    public List<GameObject> itemList1 = new List<GameObject>();
    public List<GameObject> itemList2 = new List<GameObject>();
    public List<GameObject> itemList3 = new List<GameObject>();

    [Header("First Visit - Scene transition")]
    [SerializeField] private string nextScene;

    [Header("Second Visit - Narrator audio")]
    public AudioClip secondVisitNarratorAudio1;
    public AudioClip secondVisitNarratorAudio2;
    public AudioClip secondVisitNarratorAudio3;

    [Header("Second Visit - Item lists")]
    public List<GameObject> secondVisitItemList1 = new List<GameObject>();
    public List<GameObject> secondVisitItemList2 = new List<GameObject>();
    public List<GameObject> secondVisitItemList3 = new List<GameObject>();

    [Header("Second Visit - Scene transition")]
    [SerializeField] private string secondVisitNextScene;

    [Header("Third Visit - Narrator audio")]
    public AudioClip thirdVisitNarratorAudio1;
    public AudioClip thirdVisitNarratorAudio2;
    public AudioClip thirdVisitNarratorAudio3;

    [Header("Third Visit - Item lists")]
    public List<GameObject> thirdVisitItemList1 = new List<GameObject>();
    public List<GameObject> thirdVisitItemList2 = new List<GameObject>();
    public List<GameObject> thirdVisitItemList3 = new List<GameObject>();

    [Header("Third Visit - Scene transition")]
    [SerializeField] private string thirdVisitNextScene;

    [Header("Fourth Visit - Narrator audio")]
    public AudioClip fourthVisitNarratorAudio1;
    public AudioClip fourthVisitNarratorAudio2;
    public AudioClip fourthVisitNarratorAudio3;

    [Header("Fourth Visit - Item lists")]
    public List<GameObject> fourthVisitItemList1 = new List<GameObject>();
    public List<GameObject> fourthVisitItemList2 = new List<GameObject>();
    public List<GameObject> fourthVisitItemList3 = new List<GameObject>();

    [Header("Fourth Visit - Scene transition")]
    [SerializeField] private string fourthVisitNextScene;

    [Header("Fifth Visit - Narrator audio")]
    public AudioClip fifthVisitNarratorAudio1;
    public AudioClip fifthVisitNarratorAudio2;
    public AudioClip fifthVisitNarratorAudio3;

    [Header("Fifth Visit - Item lists")]
    public List<GameObject> fifthVisitItemList1 = new List<GameObject>();
    public List<GameObject> fifthVisitItemList2 = new List<GameObject>();
    public List<GameObject> fifthVisitItemList3 = new List<GameObject>();

    [Header("Fifth Visit - Scene transition")]
    [SerializeField] private string fifthVisitNextScene;

    [Header("Sixth Visit - Narrator audio")]
    public AudioClip sixthVisitNarratorAudio1;
    public AudioClip sixthVisitNarratorAudio2;
    public AudioClip sixthVisitNarratorAudio3;

    [Header("Sixth Visit - Item lists")]
    public List<GameObject> sixthVisitItemList1 = new List<GameObject>();
    public List<GameObject> sixthVisitItemList2 = new List<GameObject>();
    public List<GameObject> sixthVisitItemList3 = new List<GameObject>();

    [Header("Sixth Visit - Scene transition")]
    [SerializeField] private string sixthVisitNextScene;

    private readonly HashSet<Interactive> trackedItems = new HashSet<Interactive>();
    private readonly HashSet<Interactive> currentStepItems = new HashSet<Interactive>();
    private readonly HashSet<Interactive> interactedItems = new HashSet<Interactive>();
    private AudioSource narrationSource;

    public int CurrentVisitNumber { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ClearVisitCountsAtApplicationStart()
    {
        ResetVisitCountsForNewGame();
    }

    public static void ResetVisitCountsForNewGame()
    {
        HashSet<string> visitCountKeys = new HashSet<string>();
        string registeredKeys = PlayerPrefs.GetString(VisitCountRegistryKey, string.Empty);

        foreach (string key in registeredKeys.Split('\n'))
        {
            if (!string.IsNullOrEmpty(key))
                visitCountKeys.Add(key);
        }

        for (int buildIndex = 0; buildIndex < UnitySceneManager.sceneCountInBuildSettings; buildIndex++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            if (string.IsNullOrEmpty(scenePath))
                continue;

            visitCountKeys.Add(VisitCountPrefix + scenePath);
            visitCountKeys.Add(LegacyVisitCountPrefix + Path.GetFileNameWithoutExtension(scenePath));
        }

        foreach (string key in visitCountKeys)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.DeleteKey(key + SceneNameSuffix);
            PlayerPrefs.DeleteKey(key + ScenePathSuffix);
        }

        PlayerPrefs.DeleteKey(VisitCountRegistryKey);
        PlayerPrefs.Save();
    }

    private void Awake()
    {
        narrationSource = GetComponent<AudioSource>();
        if (narrationSource == null)
            narrationSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        PrepareItemLists();

        Scene currentScene = UnitySceneManager.GetActiveScene();
        CurrentVisitNumber = RecordSceneVisit(currentScene);
        HideObjectsForVisit(CurrentVisitNumber);

        StartCoroutine(RunSequence(GetVisitConfiguration(CurrentVisitNumber)));
    }

    private void OnDestroy()
    {
        foreach (Interactive item in trackedItems)
        {
            if (item != null)
                item.Interacted -= OnItemInteracted;
        }
    }

    public static int GetVisitCount(Scene scene)
    {
        if (!scene.IsValid())
            return 0;

        return Mathf.Clamp(PlayerPrefs.GetInt(GetVisitCountKey(scene), 0), 0, MaximumVisits);
    }

    private static int RecordSceneVisit(Scene scene)
    {
        string visitCountKey = GetVisitCountKey(scene);
        int visitCount = Mathf.Min(GetVisitCount(scene) + 1, MaximumVisits);
        PlayerPrefs.SetInt(visitCountKey, visitCount);
        PlayerPrefs.SetString(visitCountKey + SceneNameSuffix, scene.name);
        PlayerPrefs.SetString(visitCountKey + ScenePathSuffix, scene.path);
        RegisterVisitCountKey(visitCountKey);
        PlayerPrefs.Save();
        return visitCount;
    }

    private static void RegisterVisitCountKey(string visitCountKey)
    {
        string registeredKeys = PlayerPrefs.GetString(VisitCountRegistryKey, string.Empty);
        foreach (string key in registeredKeys.Split('\n'))
        {
            if (key == visitCountKey)
                return;
        }

        string updatedKeys = string.IsNullOrEmpty(registeredKeys)
            ? visitCountKey
            : registeredKeys + "\n" + visitCountKey;
        PlayerPrefs.SetString(VisitCountRegistryKey, updatedKeys);
    }

    private static string GetVisitCountKey(Scene scene)
    {
        string sceneIdentity = string.IsNullOrEmpty(scene.path) ? scene.name : scene.path;
        return VisitCountPrefix + sceneIdentity;
    }

    private VisitConfiguration GetVisitConfiguration(int visitNumber)
    {
        switch (visitNumber)
        {
            case 1:
                return new VisitConfiguration(
                    new[] { narratorAudio1, narratorAudio2, narratorAudio3 },
                    new[] { itemList1, itemList2, itemList3 },
                    nextScene);
            case 2:
                return new VisitConfiguration(
                    new[] { secondVisitNarratorAudio1, secondVisitNarratorAudio2, secondVisitNarratorAudio3 },
                    new[] { secondVisitItemList1, secondVisitItemList2, secondVisitItemList3 },
                    secondVisitNextScene);
            case 3:
                return new VisitConfiguration(
                    new[] { thirdVisitNarratorAudio1, thirdVisitNarratorAudio2, thirdVisitNarratorAudio3 },
                    new[] { thirdVisitItemList1, thirdVisitItemList2, thirdVisitItemList3 },
                    thirdVisitNextScene);
            case 4:
                return new VisitConfiguration(
                    new[] { fourthVisitNarratorAudio1, fourthVisitNarratorAudio2, fourthVisitNarratorAudio3 },
                    new[] { fourthVisitItemList1, fourthVisitItemList2, fourthVisitItemList3 },
                    fourthVisitNextScene);
            case 5:
                return new VisitConfiguration(
                    new[] { fifthVisitNarratorAudio1, fifthVisitNarratorAudio2, fifthVisitNarratorAudio3 },
                    new[] { fifthVisitItemList1, fifthVisitItemList2, fifthVisitItemList3 },
                    fifthVisitNextScene);
            default:
                return new VisitConfiguration(
                    new[] { sixthVisitNarratorAudio1, sixthVisitNarratorAudio2, sixthVisitNarratorAudio3 },
                    new[] { sixthVisitItemList1, sixthVisitItemList2, sixthVisitItemList3 },
                    sixthVisitNextScene);
        }
    }

    private void PrepareItemLists()
    {
        foreach (List<GameObject> list in GetAllItemLists())
        {
            if (list == null)
                continue;

            foreach (GameObject gameObject in list)
            {
                if (gameObject == null)
                    continue;

                Interactive item = gameObject.GetComponent<Interactive>();
                if (item == null)
                {
                    Debug.LogError($"SceneManager: '{gameObject.name}' in an item list has no Interactive component.", gameObject);
                    continue;
                }

                if (trackedItems.Add(item))
                    item.Interacted += OnItemInteracted;

                item.SetInteractable(false);
            }
        }
    }

    private List<GameObject>[] GetAllItemLists()
    {
        return new[]
        {
            itemList1, itemList2, itemList3,
            secondVisitItemList1, secondVisitItemList2, secondVisitItemList3,
            thirdVisitItemList1, thirdVisitItemList2, thirdVisitItemList3,
            fourthVisitItemList1, fourthVisitItemList2, fourthVisitItemList3,
            fifthVisitItemList1, fifthVisitItemList2, fifthVisitItemList3,
            sixthVisitItemList1, sixthVisitItemList2, sixthVisitItemList3
        };
    }

    private void HideObjectsForVisit(int visitNumber)
    {
        List<GameObject> objectsToHide = null;

        switch (visitNumber)
        {
            case 1:
                objectsToHide = firstVisitHideObjects;
                break;
            case 2:
                objectsToHide = secondVisitHideObjects;
                break;
            case 3:
                objectsToHide = thirdVisitHideObjects;
                break;
            case 4:
                objectsToHide = fourthVisitHideObjects;
                break;
            case 5:
                objectsToHide = fifthVisitHideObjects;
                break;
            case 6:
                objectsToHide = sixthVisitHideObjects;
                break;
        }

        if (objectsToHide == null)
            return;

        foreach (GameObject gameObject in objectsToHide)
        {
            if (gameObject != null)
                gameObject.SetActive(false);
        }
    }

    private IEnumerator RunSequence(VisitConfiguration configuration)
    {
        for (int step = 0; step < configuration.NarratorClips.Length; step++)
        {
            List<Interactive> stepItems = GetStepItems(configuration.ItemLists[step]);
            AudioClip narratorClip = configuration.NarratorClips[step];

            if (narratorClip == null || stepItems.Count == 0)
                continue;

            yield return PlayNarratorAndWait(narratorClip);

            currentStepItems.Clear();
            interactedItems.Clear();

            foreach (Interactive item in stepItems)
            {
                currentStepItems.Add(item);
                item.SetInteractable(true);
            }

            int requiredInteractions = Mathf.Min(stepItems.Count, MaximumItemsPerStep);
            while (interactedItems.Count < requiredInteractions)
                yield return null;

            foreach (Interactive item in stepItems)
                item.SetInteractable(false);

            currentStepItems.Clear();
            interactedItems.Clear();
        }

        LoadNextScene(configuration.NextScene);
    }

    private IEnumerator PlayNarratorAndWait(AudioClip narratorClip)
    {
        narrationSource.Stop();
        narrationSource.clip = narratorClip;
        narrationSource.loop = false;

        const double schedulingLeadTime = 0.1d;
        double playbackStartTime = AudioSettings.dspTime + schedulingLeadTime;
        double playbackDuration = narratorClip.length / Mathf.Max(Mathf.Abs(narrationSource.pitch), 0.01f);
        double playbackEndTime = playbackStartTime + playbackDuration;

        narrationSource.PlayScheduled(playbackStartTime);

        while (AudioSettings.dspTime < playbackEndTime)
            yield return null;

        while (narrationSource.isPlaying)
            yield return null;
    }

    private List<Interactive> GetStepItems(List<GameObject> objects)
    {
        List<Interactive> stepItems = new List<Interactive>();
        HashSet<Interactive> uniqueItems = new HashSet<Interactive>();

        if (objects == null)
            return stepItems;

        foreach (GameObject gameObject in objects)
        {
            if (gameObject == null || !gameObject.activeInHierarchy)
                continue;

            Interactive item = gameObject.GetComponent<Interactive>();
            if (item != null && uniqueItems.Add(item))
                stepItems.Add(item);
        }

        return stepItems;
    }

    private void OnItemInteracted(Interactive item)
    {
        if (currentStepItems.Contains(item))
            interactedItems.Add(item);
    }

    private void LoadNextScene(string sceneToLoad)
    {
        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogWarning($"SceneManager: Set a next scene for visit {CurrentVisitNumber}.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError($"SceneManager: Scene '{sceneToLoad}' is not included in the build settings or could not be loaded.", this);
            return;
        }

        UnitySceneManager.LoadScene(sceneToLoad);
    }

    private sealed class VisitConfiguration
    {
        public readonly AudioClip[] NarratorClips;
        public readonly List<GameObject>[] ItemLists;
        public readonly string NextScene;

        public VisitConfiguration(AudioClip[] narratorClips, List<GameObject>[] itemLists, string nextScene)
        {
            NarratorClips = narratorClips;
            ItemLists = itemLists;
            NextScene = nextScene;
        }
    }
}
