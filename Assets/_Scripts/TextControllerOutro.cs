using System.Collections;
using UnityEngine;

public class TextControllerOutro : MonoBehaviour
{
    [Tooltip("UI GameObjects containing the text (each should be a child with a Text or TMP component)")]
    public GameObject[] textObjects;

    [Tooltip("How long a fade-in or fade-out takes (seconds)")]
    public float textFadeDuration = 1f;

    [Tooltip("How long the text stays fully visible before fading out (seconds)")]
    public float displayDuration = 5f;

    // Optional: index mapping for outcomes (set in Inspector)
    [Tooltip("Index used when player wins.")]
    public int winIndex = 0;
    [Tooltip("Index used when player loses.")]
    public int loseIndex = 1;

    private CanvasGroup[] groups;
    private Coroutine cycleCoroutine;

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

            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();

            groups[i] = cg;

            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            go.SetActive(false);
        }
    }

    // Shows the text at index. If autoHide is true, it will fade out after displayDuration.
    public void ShowTextAtIndex(int index, bool autoHide = false)
    {
        if (groups == null || groups.Length == 0)
        {
            Debug.LogWarning("TextController: No groups available.");
            return;
        }

        if (index < 0 || index >= groups.Length)
        {
            Debug.LogWarning($"TextController: index {index} out of range.");
            return;
        }

        // stop any running cycle
        if (cycleCoroutine != null)
        {
            StopCoroutine(cycleCoroutine);
            cycleCoroutine = null;
        }

        // hide all other texts immediately
        for (int i = 0; i < groups.Length; i++)
        {
            if (i == index) continue;
            groups[i].gameObject.SetActive(false);
            groups[i].alpha = 0f;
        }

        cycleCoroutine = StartCoroutine(ShowSingleTextCoroutine(index, autoHide));
    }

    // Map a boolean outcome to a text index (win/lose)
    public void ShowTextByOutcome(bool playerWon, bool autoHide = false)
    {
        int idx = playerWon ? winIndex : loseIndex;
        ShowTextAtIndex(idx, autoHide);
    }

    private IEnumerator ShowSingleTextCoroutine(int index, bool autoHide)
    {
        CanvasGroup cg = groups[index];

        cg.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(cg, 0f, 1f, textFadeDuration));

        if (autoHide)
        {
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(Fade(cg, 1f, 0f, textFadeDuration));
            cg.gameObject.SetActive(false);
        }
        else
        {
            // keep visible; optionally ensure it's fully opaque
            cg.alpha = 1f;
        }

        cycleCoroutine = null;
    }

    private IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;

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

    // Optional: convenience for stopping any running show
    public void HideAllImmediate()
    {
        if (cycleCoroutine != null)
        {
            StopCoroutine(cycleCoroutine);
            cycleCoroutine = null;
        }
        if (groups == null) return;
        foreach (var g in groups)
        {
            if (g == null) continue;
            g.alpha = 0f;
            g.gameObject.SetActive(false);
        }
    }
}
