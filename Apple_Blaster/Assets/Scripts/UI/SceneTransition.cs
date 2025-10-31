using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Provides a simple fade-to-black scene transition. Place this on a GameObject in your first scene (or let it create itself).
/// It is a singleton and marked DontDestroyOnLoad so it persists across scenes.
/// Public API: SceneTransition.Instance.TransitionTo(sceneName, duration)
/// </summary>
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Tooltip("Default fade duration in seconds")]
    public float defaultFadeDuration = 0.5f;

    // The CanvasGroup used to fade (alpha 0 = transparent, 1 = opaque)
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log($"SceneTransition.Awake: Instance set on GameObject '{gameObject.name}'");
        DontDestroyOnLoad(gameObject);

        // Try to find an existing CanvasGroup child
        canvasGroup = GetComponentInChildren<CanvasGroup>();
        if (canvasGroup == null)
        {
            CreateOverlay();
        }
    }

    private void CreateOverlay()
    {
        Debug.Log("SceneTransition: Creating overlay UI for fades");
        var canvasGO = new GameObject("SceneTransitionCanvas");
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // on top

        var raycaster = canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var imageGO = new GameObject("Overlay");
        imageGO.transform.SetParent(canvasGO.transform, false);
        var image = imageGO.AddComponent<Image>();
        image.color = Color.black;
    // Don't block raycasts when transparent by default
    image.raycastTarget = false;

        var rect = image.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        canvasGroup = imageGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        // Allow clicks to pass through when fully transparent
        canvasGroup.blocksRaycasts = false;
    }

    [RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnRuntimeInit()
    {
        Debug.Log("SceneTransition: Runtime initialize - assembly loaded.");
        Debug.Log($"SceneTransition: Instance currently {(Instance==null?"null":"present")}");
    }

    /// <summary>
    /// Fade out, load scene, then fade in. Uses defaultFadeDuration if duration <= 0.
    /// </summary>
    public void TransitionTo(string sceneName, float duration = -1f)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneTransition.TransitionTo called with empty sceneName");
            return;
        }

        Debug.Log($"SceneTransition: TransitionTo requested -> {sceneName} (duration={duration})");

        if (duration <= 0f) duration = defaultFadeDuration;
        StartCoroutine(DoTransition(sceneName, duration));
    }

    /// <summary>
    /// Static helper that ensures an Instance exists (creates the GameObject+component if necessary)
    /// and then starts the transition. Use this from callers who may not have placed a SceneTransition in the scene.
    /// </summary>
    public static void TransitionToScene(string sceneName, float duration = -1f)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneTransition.TransitionToScene called with empty sceneName");
            return;
        }

        if (Instance == null)
        {
            Debug.Log("SceneTransition: No instance found, creating singleton GameObject.");
            // Create a GameObject to host the SceneTransition singleton
            var go = new GameObject("SceneTransition");
            // Ensure it persists during startup sequence
            Instance = go.AddComponent<SceneTransition>();
            // Awake will run and create overlay if needed
        }

        Debug.Log($"SceneTransition: Starting transition to {sceneName} via static helper.");
        Instance.TransitionTo(sceneName, duration);
    }

    private IEnumerator DoTransition(string sceneName, float duration)
    {
        // Fade out
        Debug.Log($"SceneTransition: Fading out before loading {sceneName} (duration={duration})");
        yield return StartCoroutine(Fade(0f, 1f, duration));

        // Load async
        Debug.Log($"SceneTransition: Loading scene {sceneName} (async)");
        var async = SceneManager.LoadSceneAsync(sceneName);
        if (async != null)
        {
            // Wait until the load is done
            while (!async.isDone)
            {
                yield return null;
            }
            Debug.Log($"SceneTransition: Scene {sceneName} finished loading (async)");
        }
        else
        {
            // fallback
            Debug.LogWarning($"SceneTransition: LoadSceneAsync returned null for {sceneName}, falling back to LoadScene.");
            SceneManager.LoadScene(sceneName);
        }

        // Fade in
        Debug.Log($"SceneTransition: Fading in after loading {sceneName} (duration={duration})");
        yield return StartCoroutine(Fade(1f, 0f, duration));
        Debug.Log($"SceneTransition: Transition to {sceneName} complete.");
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float elapsed = 0f;
        canvasGroup.alpha = from;
        // If we're going to fade to visible, start blocking raycasts so UI underneath won't receive clicks
        if (to > 0f)
        {
            canvasGroup.blocksRaycasts = true;
        }
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        canvasGroup.alpha = to;
        // If we've become fully transparent, stop blocking raycasts
        if (Mathf.Approximately(to, 0f))
        {
            canvasGroup.blocksRaycasts = false;
        }
    }
}
