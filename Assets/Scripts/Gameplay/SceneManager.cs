using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : MonoBehaviour
{
    private const int MaximumVisits = 4;
    private const int MaximumItemsPerStep = 5;
    private const float QuestMarkerDelaySeconds = 10f;
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

    [Header("First Visit - Narrator audio")]
    public AudioClip narratorAudio1;
    public AudioClip narratorAudio2;
    public AudioClip narratorAudio3;

    [Header("First Visit - Speaker signs (leave empty for off-screen narrator)")]
    public VoiceReactiveSign narratorSpeaker1;
    public VoiceReactiveSign narratorSpeaker2;
    public VoiceReactiveSign narratorSpeaker3;

    [Header("First Visit - Item lists")]
    public List<GameObject> itemList1 = new List<GameObject>();
    public List<GameObject> itemList2 = new List<GameObject>();
    public List<GameObject> itemList3 = new List<GameObject>();

    [Header("First Visit - Optional quest markers")]
    [SerializeField] private QuestMarkerSetting firstVisitItemList1Quest = new QuestMarkerSetting();
    [SerializeField] private QuestMarkerSetting firstVisitItemList2Quest = new QuestMarkerSetting();
    [SerializeField] private QuestMarkerSetting firstVisitItemList3Quest = new QuestMarkerSetting();

    [Header("First Visit - Scene transition")]
    [SerializeField] private string nextScene;

    [Header("First Visit - Objects revealed after narration")]
    public List<GameObject> firstVisitNarration1RevealObjects = new List<GameObject>();
    public List<GameObject> firstVisitNarration2RevealObjects = new List<GameObject>();
    public List<GameObject> firstVisitNarration3RevealObjects = new List<GameObject>();

    [Header("First Visit - Objects hidden after narration")]
    public List<GameObject> firstVisitNarration1HideObjects = new List<GameObject>();
    public List<GameObject> firstVisitNarration2HideObjects = new List<GameObject>();
    public List<GameObject> firstVisitNarration3HideObjects = new List<GameObject>();

    [Header("First Visit - Objects revealed after item lists")]
    public List<GameObject> firstVisitItemList1RevealObjects = new List<GameObject>();
    public List<GameObject> firstVisitItemList2RevealObjects = new List<GameObject>();
    public List<GameObject> firstVisitItemList3RevealObjects = new List<GameObject>();

    [Header("First Visit - Objects hidden after item lists")]
    public List<GameObject> firstVisitItemList1HideObjects = new List<GameObject>();
    public List<GameObject> firstVisitItemList2HideObjects = new List<GameObject>();
    public List<GameObject> firstVisitItemList3HideObjects = new List<GameObject>();

    [Header("Second Visit - Narrator audio")]
    public AudioClip secondVisitNarratorAudio1;
    public AudioClip secondVisitNarratorAudio2;
    public AudioClip secondVisitNarratorAudio3;

    [Header("Second Visit - Speaker signs (leave empty for off-screen narrator)")]
    public VoiceReactiveSign secondVisitNarratorSpeaker1;
    public VoiceReactiveSign secondVisitNarratorSpeaker2;
    public VoiceReactiveSign secondVisitNarratorSpeaker3;

    [Header("Second Visit - Item lists")]
    public List<GameObject> secondVisitItemList1 = new List<GameObject>();
    public List<GameObject> secondVisitItemList2 = new List<GameObject>();
    public List<GameObject> secondVisitItemList3 = new List<GameObject>();

    [Header("Second Visit - Optional quest markers")]
    [SerializeField] private QuestMarkerSetting secondVisitItemList1Quest = new QuestMarkerSetting();
    [SerializeField] private QuestMarkerSetting secondVisitItemList2Quest = new QuestMarkerSetting();
    [SerializeField] private QuestMarkerSetting secondVisitItemList3Quest = new QuestMarkerSetting();

    [Header("Second Visit - Scene transition")]
    [SerializeField] private string secondVisitNextScene;

    [Header("Second Visit - Objects revealed after narration")]
    public List<GameObject> secondVisitNarration1RevealObjects = new List<GameObject>();
    public List<GameObject> secondVisitNarration2RevealObjects = new List<GameObject>();
    public List<GameObject> secondVisitNarration3RevealObjects = new List<GameObject>();

    [Header("Second Visit - Objects hidden after narration")]
    public List<GameObject> secondVisitNarration1HideObjects = new List<GameObject>();
    public List<GameObject> secondVisitNarration2HideObjects = new List<GameObject>();
    public List<GameObject> secondVisitNarration3HideObjects = new List<GameObject>();

    [Header("Second Visit - Objects revealed after item lists")]
    public List<GameObject> secondVisitItemList1RevealObjects = new List<GameObject>();
    public List<GameObject> secondVisitItemList2RevealObjects = new List<GameObject>();
    public List<GameObject> secondVisitItemList3RevealObjects = new List<GameObject>();

    [Header("Second Visit - Objects hidden after item lists")]
    public List<GameObject> secondVisitItemList1HideObjects = new List<GameObject>();
    public List<GameObject> secondVisitItemList2HideObjects = new List<GameObject>();
    public List<GameObject> secondVisitItemList3HideObjects = new List<GameObject>();

    [Header("Third Visit - Narrator audio")]
    public AudioClip thirdVisitNarratorAudio1;
    public AudioClip thirdVisitNarratorAudio2;
    public AudioClip thirdVisitNarratorAudio3;

    [Header("Third Visit - Speaker signs (leave empty for off-screen narrator)")]
    public VoiceReactiveSign thirdVisitNarratorSpeaker1;
    public VoiceReactiveSign thirdVisitNarratorSpeaker2;
    public VoiceReactiveSign thirdVisitNarratorSpeaker3;

    [Header("Third Visit - Item lists")]
    public List<GameObject> thirdVisitItemList1 = new List<GameObject>();
    public List<GameObject> thirdVisitItemList2 = new List<GameObject>();
    public List<GameObject> thirdVisitItemList3 = new List<GameObject>();

    [Header("Third Visit - Optional quest markers")]
    [SerializeField] private QuestMarkerSetting thirdVisitItemList1Quest = new QuestMarkerSetting();
    [SerializeField] private QuestMarkerSetting thirdVisitItemList2Quest = new QuestMarkerSetting();
    [SerializeField] private QuestMarkerSetting thirdVisitItemList3Quest = new QuestMarkerSetting();

    [Header("Third Visit - Scene transition")]
    [SerializeField] private string thirdVisitNextScene;

    [Header("Third Visit - Objects revealed after narration")]
    public List<GameObject> thirdVisitNarration1RevealObjects = new List<GameObject>();
    public List<GameObject> thirdVisitNarration2RevealObjects = new List<GameObject>();
    public List<GameObject> thirdVisitNarration3RevealObjects = new List<GameObject>();

    [Header("Third Visit - Objects hidden after narration")]
    public List<GameObject> thirdVisitNarration1HideObjects = new List<GameObject>();
    public List<GameObject> thirdVisitNarration2HideObjects = new List<GameObject>();
    public List<GameObject> thirdVisitNarration3HideObjects = new List<GameObject>();

    [Header("Third Visit - Objects revealed after item lists")]
    public List<GameObject> thirdVisitItemList1RevealObjects = new List<GameObject>();
    public List<GameObject> thirdVisitItemList2RevealObjects = new List<GameObject>();
    public List<GameObject> thirdVisitItemList3RevealObjects = new List<GameObject>();

    [Header("Third Visit - Objects hidden after item lists")]
    public List<GameObject> thirdVisitItemList1HideObjects = new List<GameObject>();
    public List<GameObject> thirdVisitItemList2HideObjects = new List<GameObject>();
    public List<GameObject> thirdVisitItemList3HideObjects = new List<GameObject>();

    [Header("Fourth Visit - Narrator audio")]
    public AudioClip fourthVisitNarratorAudio1;
    public AudioClip fourthVisitNarratorAudio2;
    public AudioClip fourthVisitNarratorAudio3;

    [Header("Fourth Visit - Speaker signs (leave empty for off-screen narrator)")]
    public VoiceReactiveSign fourthVisitNarratorSpeaker1;
    public VoiceReactiveSign fourthVisitNarratorSpeaker2;
    public VoiceReactiveSign fourthVisitNarratorSpeaker3;

    [Header("Fourth Visit - Item lists")]
    public List<GameObject> fourthVisitItemList1 = new List<GameObject>();
    public List<GameObject> fourthVisitItemList2 = new List<GameObject>();
    public List<GameObject> fourthVisitItemList3 = new List<GameObject>();

    [Header("Fourth Visit - Optional quest markers")]
    [SerializeField] private QuestMarkerSetting fourthVisitItemList1Quest = new QuestMarkerSetting();
    [SerializeField] private QuestMarkerSetting fourthVisitItemList2Quest = new QuestMarkerSetting();
    [SerializeField] private QuestMarkerSetting fourthVisitItemList3Quest = new QuestMarkerSetting();

    [Header("Fourth Visit - Scene transition")]
    [SerializeField] private string fourthVisitNextScene;

    [Header("Fourth Visit - Objects revealed after narration")]
    public List<GameObject> fourthVisitNarration1RevealObjects = new List<GameObject>();
    public List<GameObject> fourthVisitNarration2RevealObjects = new List<GameObject>();
    public List<GameObject> fourthVisitNarration3RevealObjects = new List<GameObject>();

    [Header("Fourth Visit - Objects hidden after narration")]
    public List<GameObject> fourthVisitNarration1HideObjects = new List<GameObject>();
    public List<GameObject> fourthVisitNarration2HideObjects = new List<GameObject>();
    public List<GameObject> fourthVisitNarration3HideObjects = new List<GameObject>();

    [Header("Fourth Visit - Objects revealed after item lists")]
    public List<GameObject> fourthVisitItemList1RevealObjects = new List<GameObject>();
    public List<GameObject> fourthVisitItemList2RevealObjects = new List<GameObject>();
    public List<GameObject> fourthVisitItemList3RevealObjects = new List<GameObject>();

    [Header("Fourth Visit - Objects hidden after item lists")]
    public List<GameObject> fourthVisitItemList1HideObjects = new List<GameObject>();
    public List<GameObject> fourthVisitItemList2HideObjects = new List<GameObject>();
    public List<GameObject> fourthVisitItemList3HideObjects = new List<GameObject>();

    private readonly HashSet<Interactive> trackedItems = new HashSet<Interactive>();
    private readonly HashSet<Interactive> currentStepItems = new HashSet<Interactive>();
    private readonly HashSet<Interactive> interactedItems = new HashSet<Interactive>();
    private AudioSource narrationSource;
    private VoiceReactiveSign activeSpeakerSign;

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
        HideAllQuestMarkers();
        PrepareItemLists();

        Scene currentScene = UnitySceneManager.GetActiveScene();
        CurrentVisitNumber = RecordSceneVisit(currentScene);
        HideObjectsForVisit(CurrentVisitNumber);

        StartCoroutine(RunSequence(GetVisitConfiguration(CurrentVisitNumber)));
    }

    private void OnDestroy()
    {
        if (activeSpeakerSign != null)
            activeSpeakerSign.EndSpeaking();

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
                    new[] { narratorSpeaker1, narratorSpeaker2, narratorSpeaker3 },
                    new[] { itemList1, itemList2, itemList3 },
                    nextScene,
                    new[] { firstVisitNarration1RevealObjects, firstVisitNarration2RevealObjects, firstVisitNarration3RevealObjects },
                    new[] { firstVisitNarration1HideObjects, firstVisitNarration2HideObjects, firstVisitNarration3HideObjects },
                    new[] { firstVisitItemList1RevealObjects, firstVisitItemList2RevealObjects, firstVisitItemList3RevealObjects },
                    new[] { firstVisitItemList1HideObjects, firstVisitItemList2HideObjects, firstVisitItemList3HideObjects },
                    new[] { firstVisitItemList1Quest, firstVisitItemList2Quest, firstVisitItemList3Quest });
            case 2:
                return new VisitConfiguration(
                    new[] { secondVisitNarratorAudio1, secondVisitNarratorAudio2, secondVisitNarratorAudio3 },
                    new[] { secondVisitNarratorSpeaker1, secondVisitNarratorSpeaker2, secondVisitNarratorSpeaker3 },
                    new[] { secondVisitItemList1, secondVisitItemList2, secondVisitItemList3 },
                    secondVisitNextScene,
                    new[] { secondVisitNarration1RevealObjects, secondVisitNarration2RevealObjects, secondVisitNarration3RevealObjects },
                    new[] { secondVisitNarration1HideObjects, secondVisitNarration2HideObjects, secondVisitNarration3HideObjects },
                    new[] { secondVisitItemList1RevealObjects, secondVisitItemList2RevealObjects, secondVisitItemList3RevealObjects },
                    new[] { secondVisitItemList1HideObjects, secondVisitItemList2HideObjects, secondVisitItemList3HideObjects },
                    new[] { secondVisitItemList1Quest, secondVisitItemList2Quest, secondVisitItemList3Quest });
            case 3:
                return new VisitConfiguration(
                    new[] { thirdVisitNarratorAudio1, thirdVisitNarratorAudio2, thirdVisitNarratorAudio3 },
                    new[] { thirdVisitNarratorSpeaker1, thirdVisitNarratorSpeaker2, thirdVisitNarratorSpeaker3 },
                    new[] { thirdVisitItemList1, thirdVisitItemList2, thirdVisitItemList3 },
                    thirdVisitNextScene,
                    new[] { thirdVisitNarration1RevealObjects, thirdVisitNarration2RevealObjects, thirdVisitNarration3RevealObjects },
                    new[] { thirdVisitNarration1HideObjects, thirdVisitNarration2HideObjects, thirdVisitNarration3HideObjects },
                    new[] { thirdVisitItemList1RevealObjects, thirdVisitItemList2RevealObjects, thirdVisitItemList3RevealObjects },
                    new[] { thirdVisitItemList1HideObjects, thirdVisitItemList2HideObjects, thirdVisitItemList3HideObjects },
                    new[] { thirdVisitItemList1Quest, thirdVisitItemList2Quest, thirdVisitItemList3Quest });
            case 4:
                return new VisitConfiguration(
                    new[] { fourthVisitNarratorAudio1, fourthVisitNarratorAudio2, fourthVisitNarratorAudio3 },
                    new[] { fourthVisitNarratorSpeaker1, fourthVisitNarratorSpeaker2, fourthVisitNarratorSpeaker3 },
                    new[] { fourthVisitItemList1, fourthVisitItemList2, fourthVisitItemList3 },
                    fourthVisitNextScene,
                    new[] { fourthVisitNarration1RevealObjects, fourthVisitNarration2RevealObjects, fourthVisitNarration3RevealObjects },
                    new[] { fourthVisitNarration1HideObjects, fourthVisitNarration2HideObjects, fourthVisitNarration3HideObjects },
                    new[] { fourthVisitItemList1RevealObjects, fourthVisitItemList2RevealObjects, fourthVisitItemList3RevealObjects },
                    new[] { fourthVisitItemList1HideObjects, fourthVisitItemList2HideObjects, fourthVisitItemList3HideObjects },
                    new[] { fourthVisitItemList1Quest, fourthVisitItemList2Quest, fourthVisitItemList3Quest });
            default:
                return new VisitConfiguration(
                    new[] { fourthVisitNarratorAudio1, fourthVisitNarratorAudio2, fourthVisitNarratorAudio3 },
                    new[] { fourthVisitNarratorSpeaker1, fourthVisitNarratorSpeaker2, fourthVisitNarratorSpeaker3 },
                    new[] { fourthVisitItemList1, fourthVisitItemList2, fourthVisitItemList3 },
                    fourthVisitNextScene,
                    new[] { fourthVisitNarration1RevealObjects, fourthVisitNarration2RevealObjects, fourthVisitNarration3RevealObjects },
                    new[] { fourthVisitNarration1HideObjects, fourthVisitNarration2HideObjects, fourthVisitNarration3HideObjects },
                    new[] { fourthVisitItemList1RevealObjects, fourthVisitItemList2RevealObjects, fourthVisitItemList3RevealObjects },
                    new[] { fourthVisitItemList1HideObjects, fourthVisitItemList2HideObjects, fourthVisitItemList3HideObjects },
                    new[] { fourthVisitItemList1Quest, fourthVisitItemList2Quest, fourthVisitItemList3Quest });
        }
    }

    private void HideAllQuestMarkers()
    {
        for (int visit = 1; visit <= MaximumVisits; visit++)
        {
            foreach (QuestMarkerSetting setting in GetVisitConfiguration(visit).QuestMarkers)
            {
                if (setting != null && setting.markerObject != null)
                    setting.markerObject.SetActive(false);
            }
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
        }

        if (objectsToHide != null)
        {
            foreach (GameObject gameObject in objectsToHide)
            {
                if (gameObject != null)
                    gameObject.SetActive(false);
            }
        }

        foreach (List<GameObject> stepObjects in GetVisitConfiguration(visitNumber).ObjectsToReveal)
        {
            if (stepObjects == null)
                continue;

            foreach (GameObject gameObject in stepObjects)
            {
                if (gameObject != null)
                    gameObject.SetActive(false);
            }
        }

        foreach (List<GameObject> stepObjects in GetVisitConfiguration(visitNumber).ObjectsToRevealAfterItemLists)
        {
            if (stepObjects == null)
                continue;

            foreach (GameObject gameObject in stepObjects)
            {
                if (gameObject != null)
                    gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator RunSequence(VisitConfiguration configuration)
    {
        for (int step = 0; step < configuration.NarratorClips.Length; step++)
        {
            AudioClip narratorClip = configuration.NarratorClips[step];
            QuestMarkerSetting questSetting = configuration.QuestMarkers[step];
            GameObject questMarker = questSetting != null ? questSetting.markerObject : null;
            if (questMarker != null)
                questMarker.SetActive(false);

            float narrationCompletedAt = Time.unscaledTime;
            if (narratorClip != null)
            {
                yield return PlayNarratorAndWait(narratorClip, configuration.SpeakerSigns[step]);
                narrationCompletedAt = Time.unscaledTime;
            }

            RevealObjects(configuration.ObjectsToReveal[step]);
            HideObjects(configuration.ObjectsToHide[step]);

            if (narratorClip != null)
            {
                List<Interactive> stepItems = GetStepItems(configuration.ItemLists[step]);
                if (stepItems.Count > 0)
                {
                    Interactive questTarget = null;
                    bool canShowQuestMarker = false;
                    if (questMarker != null)
                    {
                        if (questSetting.targetObject == null)
                        {
                            Debug.LogWarning($"SceneManager: Quest marker for item list {step + 1} has no target object.", this);
                        }
                        else
                        {
                            questTarget = questSetting.targetObject.GetComponent<Interactive>();
                            if (questTarget == null)
                            {
                                Debug.LogWarning($"SceneManager: Quest target '{questSetting.targetObject.name}' has no Interactive component.", questSetting.targetObject);
                            }
                            else if (!stepItems.Contains(questTarget))
                            {
                                Debug.LogWarning($"SceneManager: Quest target '{questSetting.targetObject.name}' is not in item list {step + 1}.", questSetting.targetObject);
                            }
                            else
                            {
                                canShowQuestMarker = true;
                            }
                        }
                    }

                    currentStepItems.Clear();
                    interactedItems.Clear();

                    foreach (Interactive item in stepItems)
                    {
                        currentStepItems.Add(item);
                        item.SetInteractable(true);
                    }

                    int requiredInteractions = Mathf.Min(stepItems.Count, MaximumItemsPerStep);
                    while (interactedItems.Count < requiredInteractions)
                    {
                        if (canShowQuestMarker)
                        {
                            if (interactedItems.Contains(questTarget))
                                questMarker.SetActive(false);
                            else if (Time.unscaledTime - narrationCompletedAt >= QuestMarkerDelaySeconds)
                                questMarker.SetActive(true);
                        }

                        yield return null;
                    }

                    foreach (Interactive item in stepItems)
                        item.SetInteractable(false);

                    currentStepItems.Clear();
                    interactedItems.Clear();
                }
            }

            if (questMarker != null)
                questMarker.SetActive(false);

            RevealObjects(configuration.ObjectsToRevealAfterItemLists[step]);
            HideObjects(configuration.ObjectsToHideAfterItemLists[step]);
        }

        LoadNextScene(configuration.NextScene);
    }

    private static void RevealObjects(List<GameObject> objects)
    {
        if (objects == null)
            return;

        foreach (GameObject gameObject in objects)
        {
            if (gameObject != null)
                gameObject.SetActive(true);
        }
    }

    private static void HideObjects(List<GameObject> objects)
    {
        if (objects == null)
            return;

        foreach (GameObject gameObject in objects)
        {
            if (gameObject != null)
                gameObject.SetActive(false);
        }
    }

    private IEnumerator PlayNarratorAndWait(AudioClip narratorClip, VoiceReactiveSign speakerSign)
    {
        narrationSource.Stop();
        narrationSource.clip = narratorClip;
        narrationSource.loop = false;

        const double schedulingLeadTime = 0.1d;
        double playbackStartTime = AudioSettings.dspTime + schedulingLeadTime;
        double playbackDuration = narratorClip.length / Mathf.Max(Mathf.Abs(narrationSource.pitch), 0.01f);
        double playbackEndTime = playbackStartTime + playbackDuration;

        narrationSource.PlayScheduled(playbackStartTime);

        while (AudioSettings.dspTime < playbackStartTime)
            yield return null;

        activeSpeakerSign = speakerSign;
        if (activeSpeakerSign != null)
            activeSpeakerSign.BeginSpeaking(narrationSource);

        while (AudioSettings.dspTime < playbackEndTime)
            yield return null;

        while (narrationSource.isPlaying)
            yield return null;

        if (activeSpeakerSign != null)
            activeSpeakerSign.EndSpeaking();
        activeSpeakerSign = null;
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
        public readonly VoiceReactiveSign[] SpeakerSigns;
        public readonly List<GameObject>[] ItemLists;
        public readonly string NextScene;
        public readonly List<GameObject>[] ObjectsToReveal;
        public readonly List<GameObject>[] ObjectsToHide;
        public readonly List<GameObject>[] ObjectsToRevealAfterItemLists;
        public readonly List<GameObject>[] ObjectsToHideAfterItemLists;
        public readonly QuestMarkerSetting[] QuestMarkers;

        public VisitConfiguration(
            AudioClip[] narratorClips,
            VoiceReactiveSign[] speakerSigns,
            List<GameObject>[] itemLists,
            string nextScene,
            List<GameObject>[] objectsToReveal,
            List<GameObject>[] objectsToHide,
            List<GameObject>[] objectsToRevealAfterItemLists,
            List<GameObject>[] objectsToHideAfterItemLists,
            QuestMarkerSetting[] questMarkers)
        {
            NarratorClips = narratorClips;
            SpeakerSigns = speakerSigns;
            ItemLists = itemLists;
            NextScene = nextScene;
            ObjectsToReveal = objectsToReveal;
            ObjectsToHide = objectsToHide;
            ObjectsToRevealAfterItemLists = objectsToRevealAfterItemLists;
            ObjectsToHideAfterItemLists = objectsToHideAfterItemLists;
            QuestMarkers = questMarkers;
        }
    }

    [System.Serializable]
    private sealed class QuestMarkerSetting
    {
        [Tooltip("Interactive object in this step's item list that the marker points to.")]
        public GameObject targetObject = null;

        [Tooltip("Marker shown after 10 seconds if the target has not been interacted with.")]
        public GameObject markerObject = null;
    }
}
