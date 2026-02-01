using System.Collections;
using UnityEngine;

public class TextController : MonoBehaviour
{
    [Tooltip("UI GameObjects containing the text (each should be a child with a Text or TMP component)")]
    public GameObject[] textObjects;

    [Tooltip("How long a fade-in or fade-out takes (seconds)")]
    public float textFadeDuration = 1f;

    [Tooltip("How long the text stays fully visible before fading out (seconds)")]
    public float displayDuration = 5f;

    [Header("Scene Transition")]
    [Tooltip("Name of the scene to load after all text is consumed")]
    public string nextSceneName = "SampleScene";
    
    [Tooltip("Reference to SceneController for transitions")]
    public SceneController sceneController;

    private CanvasGroup[] groups;
    private int index = 0;

    private void Awake()
    {
        if (textObjects == null || textObjects.Length == 0)
        {
            Debug.LogWarning("TextController: no textObjects assigned.");
            return;
        }

        groups = new CanvasGroup[textObjects.Length];
        for (int i = 0; i < textObjects.Length; i++)
        {
            var go = textObjects[i];
            if (go == null) continue;

            // Ensure CanvasGroup exists so we can control alpha for the whole GameObject
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();

            groups[i] = cg;

            // Start invisible and not interactable
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            go.SetActive(false);
        }
    }

    private void Start()
    {
        if (groups == null || groups.Length == 0) return;
        index = 0;
        StartCoroutine(TextChangeRoutine());
    }

    private IEnumerator TextChangeRoutine()
    {
        // Show the first text (fade in)
        CanvasGroup currentCg = groups[index];
        currentCg.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(currentCg, 0f, 1f, textFadeDuration));

        while (index < groups.Length - 1)
        {
            // Wait while visible
            yield return new WaitForSeconds(displayDuration);

            // Fade out current
            yield return StartCoroutine(Fade(currentCg, 1f, 0f, textFadeDuration));
            currentCg.gameObject.SetActive(false);

            // Advance index
            index++;
            currentCg = groups[index];

            // Activate and fade in next
            currentCg.gameObject.SetActive(true);
            yield return StartCoroutine(Fade(currentCg, 0f, 1f, textFadeDuration));
        }
        
        // Show last text for display duration
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out last text
        yield return StartCoroutine(Fade(currentCg, 1f, 0f, textFadeDuration));
        currentCg.gameObject.SetActive(false);
        
        // All text consumed - transition to next scene
        TransitionToNextScene();
    }
    
    private void TransitionToNextScene()
    {
        // Use SceneController if assigned
        if (sceneController != null)
        {
            sceneController.LoadScene(nextSceneName);
        }
        // Fallback to SceneTransitionManager
        else if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(nextSceneName);
        }
        // Direct load as last resort
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    private IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;

        // handle zero duration gracefully
        if (duration <= 0f)
        {
            cg.alpha = to;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        cg.alpha = to;
    }
}
