using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles scene transitions with fade effects.
/// Persists across scenes to manage fade-in on new scene load.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private Color fadeColor = Color.black;

    [Header("References")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private Canvas fadeCanvas;

    private bool isFading = false;
    private bool shouldFadeInOnLoad = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create fade UI if not assigned
        if (fadeCanvas == null)
        {
            CreateFadeUI();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void CreateFadeUI()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999; // Always on top

        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create fade image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.raycastTarget = false;

        // Stretch to fill screen
        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (shouldFadeInOnLoad)
        {
            shouldFadeInOnLoad = false;
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// Load a scene with fade out, then fade in on the new scene
    /// </summary>
    public void LoadSceneWithFade(string sceneName)
    {
        if (isFading) return;
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    /// <summary>
    /// Load a scene by build index with fade
    /// </summary>
    public void LoadSceneWithFade(int buildIndex)
    {
        if (isFading) return;
        StartCoroutine(FadeAndLoadSceneByIndex(buildIndex));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        shouldFadeInOnLoad = true;
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeAndLoadSceneByIndex(int buildIndex)
    {
        yield return StartCoroutine(FadeOut());
        shouldFadeInOnLoad = true;
        SceneManager.LoadScene(buildIndex);
    }

    /// <summary>
    /// Fade from transparent to black
    /// </summary>
    public IEnumerator FadeOut()
    {
        isFading = true;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeOutDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        isFading = false;
    }

    /// <summary>
    /// Fade from black to transparent
    /// </summary>
    public IEnumerator FadeIn()
    {
        isFading = true;
        
        // Start fully black
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        isFading = false;
    }

    /// <summary>
    /// Instantly set to black (useful for scene start)
    /// </summary>
    public void SetBlack()
    {
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
    }

    /// <summary>
    /// Instantly set to transparent
    /// </summary>
    public void SetClear()
    {
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
    }

    public bool IsFading => isFading;
}
