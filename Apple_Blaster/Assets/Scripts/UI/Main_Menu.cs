using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Main menu controller. Attach to a GameObject (for example an empty "MenuManager" or your Canvas).
/// Exposes methods for UI Buttons: PlayGame, OpenOptions, CloseOptions, QuitGame.
/// Options are implemented via a separate scene (additive or single).
/// </summary>
public class Main_Menu : MonoBehaviour
{
	[Header("Scenes")]
	[Tooltip("Name of the scene to load when Play is pressed. Leave empty to log a warning.")]
	[SerializeField]
	private string sceneToLoad = "";

	[Tooltip("Name of this project's Main Menu scene. Useful when the Options scene wants to go back to the menu.")]
	[SerializeField]
	private string mainMenuSceneName = "";

	[Header("Options (Scene)")]
	[Tooltip("Name of the Options scene to load when Options is pressed. Options are loaded in Single mode (replace current scene).")]
	[SerializeField]
	private string optionsSceneName = "";

	/// <summary>
	/// Called by the Play button. Loads the configured scene.
	/// If sceneToLoad is empty, logs a warning instead of throwing.
	/// </summary>
	public void PlayGame()
	{
		if (!string.IsNullOrEmpty(sceneToLoad))
		{
			if (SceneTransition.Instance != null)
				SceneTransition.Instance.TransitionTo(sceneToLoad);
			else
				SceneManager.LoadScene(sceneToLoad);
		}
		else
		{
			Debug.LogWarning("Main_Menu.PlayGame called but no sceneToLoad configured in the Inspector.");
		}
	}

	/// <summary>
	/// Opens the Options scene. Options are always loaded in Single mode (replace current scene).
	/// </summary>
	public void OpenOptions()
	{
		if (!string.IsNullOrEmpty(optionsSceneName))
		{
			if (SceneTransition.Instance != null)
				SceneTransition.Instance.TransitionTo(optionsSceneName);
			else
				SceneManager.LoadScene(optionsSceneName, LoadSceneMode.Single);
		}
		else
		{
			Debug.LogWarning("Main_Menu.OpenOptions called but no optionsSceneName configured in the Inspector.");
		}
	}

	/// <summary>
	/// CloseOptions will return to the configured main menu scene (if set).
	/// Since options are no longer loaded additively, CloseOptions will load the `mainMenuSceneName` if configured.
	/// </summary>
	public void CloseOptions()
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
			Debug.LogWarning("Main_Menu.CloseOptions called but no mainMenuSceneName configured in the Inspector. Use ReturnToMainMenu or set mainMenuSceneName.");
		}
	}

	/// <summary>
	/// Quit the application. In the editor this stops play mode.
	/// </summary>
	public void QuitGame()
	{
#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	/// <summary>
	/// Optional helper: called from the Options scene Back button to return to the Main Menu.
	/// If Main Menu scene name is configured it will be loaded, otherwise logs a warning.
	/// </summary>
	public void ReturnToMainMenu()
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
			Debug.LogWarning("Main_Menu.ReturnToMainMenu called but no mainMenuSceneName configured in the Inspector.");
		}
	}
}

