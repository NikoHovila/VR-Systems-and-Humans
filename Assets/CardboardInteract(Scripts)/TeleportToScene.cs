using UnityEngine;
using UnityEngine.SceneManagement;

using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class TeleportToScene : Interactive
{
    [SerializeField] private string sceneToLoad;

    public new void Interact()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("TeleportToScene: sceneToLoad is empty.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError($"TeleportToScene: Scene '{sceneToLoad}' could not be loaded.", this);
            return;
        }

        UnitySceneManager.LoadScene(sceneToLoad);
    }
}