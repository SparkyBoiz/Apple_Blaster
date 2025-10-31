using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controller for the Options scene. Attach to a root object in the Options scene and wire the Back/Close button to OptionsSceneController.Back().
/// Behavior:
/// - If a `Main_Menu` instance exists in any loaded scene and `preferCloseIfMenuPresent` is enabled, Back() will call Main_Menu.CloseOptions() — useful when the Options scene was loaded additively.
/// - Otherwise, if `mainMenuSceneName` is set, Back() will load that scene (useful when Options is loaded as Single).
/// </summary>
public class OptionsSceneController : MonoBehaviour
{
    [Tooltip("If set, Back() will load this scene name. Use this when the Options scene is loaded in Single mode and you want a direct return to the Main Menu.")]
    [SerializeField]
    private string mainMenuSceneName = "";

    /// <summary>
    /// Called by the Back/Close button in the Options scene.
    /// Tries to close the options overlay via Main_Menu.CloseOptions() if available, otherwise loads the configured main menu scene.
    /// </summary>
    public void Back()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            if (SceneTransition.Instance != null)
                SceneTransition.Instance.TransitionTo(mainMenuSceneName);
            else
                SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("OptionsSceneController.Back: no mainMenuSceneName configured. Set mainMenuSceneName to enable returning to the main menu.");
        }
    }
}
