// DifficultyDoor.cs
// =================
// Lisää tämä scripti jokaisen lobby-oven Interactive-objektiin.
// Aseta Inspectorissa:
//   - Difficulty       → Easy / Medium / Hard
//   - Scene To Load    → Pelin scenen nimi (string)
//

using UnityEngine;
using UnityEngine.SceneManagement;

using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class DifficultyDoor : Interactive
{
    [SerializeField] private Difficulty difficulty;
    [SerializeField] private string sceneToLoad;

    public new void Interact()
    {
        // Tallenna vaikeustaso ennen scenen vaihtoa
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.SetDifficulty(difficulty);
        }
        else
        {
            Debug.LogWarning("DifficultyManager ei löydy scenestä! Lisää se lobby-sceneen.");
        }

        // Vaihda sceneen
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
            {
                Debug.LogError($"DifficultyDoor: Scene '{sceneToLoad}' could not be loaded.", this);
                return;
            }

            global::SceneManager.ResetVisitCountsForNewGame();
            UnitySceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("DifficultyDoor: sceneToLoad on tyhjä!");
        }
    }
}
