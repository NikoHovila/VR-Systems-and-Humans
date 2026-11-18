using UnityEngine;
using UnityEngine.SceneManagement;

using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public static class LanguageManager
{
    public static string CurrentLanguage { get; private set; }
    public static event System.Action OnLanguageChanged;

    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        CurrentLanguage = PlayerPrefs.GetString("language", "FI");
        UnitySceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnLanguageChanged?.Invoke();
    }

    public static void SetLanguage(string lang)
    {
        CurrentLanguage = lang;
        PlayerPrefs.SetString("language", lang);
        OnLanguageChanged?.Invoke();
    }
}