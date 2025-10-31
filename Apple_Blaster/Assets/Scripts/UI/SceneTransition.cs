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

        var rect = image.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        canvasGroup = imageGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
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

        if (duration <= 0f) duration = defaultFadeDuration;
        StartCoroutine(DoTransition(sceneName, duration));
    }

    private IEnumerator DoTransition(string sceneName, float duration)
    {
        // Fade out
        yield return StartCoroutine(Fade(0f, 1f, duration));

        // Load async
        var async = SceneManager.LoadSceneAsync(sceneName);
        if (async != null)
        {
            // Wait until the load is done
            while (!async.isDone)
            {
                yield return null;
            }
        }
        else
        {
            // fallback
            SceneManager.LoadScene(sceneName);
        }

        // Fade in
        yield return StartCoroutine(Fade(1f, 0f, duration));
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float elapsed = 0f;
        canvasGroup.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
