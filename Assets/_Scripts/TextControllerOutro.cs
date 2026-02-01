using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Interrogation.Dialogue;

public class TextControllerOutro : MonoBehaviour
{
    [Tooltip("UI GameObjects containing the text (each should be a child with a Text or TMP component)")]
    public GameObject[] textObjects;

    [Tooltip("How long a fade-in or fade-out takes (seconds)")]
    public float textFadeDuration = 1f;

    [Tooltip("How long the text stays fully visible before fading out (seconds)")]
    public float displayDuration = 5f;

    // Optional: index mapping for outcomes (set in Inspector)
    [Tooltip("Index used when player wins (tension < 50).")]
    public int winIndex = 0;
    [Tooltip("Index used when player loses (tension >= 50).")]
    public int loseIndex = 1;

    [Header("Auto-Start")]
    [Tooltip("Automatically show win/lose text on Start based on TensionMeter")]
    public bool autoShowOnStart = true;
    [Tooltip("Auto-hide after display duration")]
    public bool autoHide = false;

    [Header("Voice Settings")]
    [Tooltip("Delay before starting voice playback")]
    public float voiceStartDelay = 0.5f;
    [Tooltip("Delay between dialogue lines")]
    public float delayBetweenLines = 0.5f;

    [Header("Scene Transition")]
    [Tooltip("Name of the initial/main menu scene to return to")]
    public string initialSceneName = "IntroScene";
    [Tooltip("Delay after dialogue ends before transitioning")]
    public float transitionDelay = 2f;

    private AudioSource audioSource;
    private CanvasGroup[] groups;
    private Coroutine cycleCoroutine;
    private EndingsData endingsData;

    private void Awake()
    {
        // Get or create AudioSource for voice playback
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // Load endings data from JSON
        endingsData = DialogueLoader.LoadEndings();
        if (endingsData == null)
        {
            Debug.LogWarning("[TextControllerOutro] Failed to load endings.json");
        }

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

    private void Start()
    {
        if (autoShowOnStart)
        {
            ShowOutcomeBasedOnTension();
        }
    }

    /// <summary>
    /// Shows win text if tension < 50, lose text if tension >= 50
    /// Also plays the appropriate voice clips from endings.json
    /// </summary>
    public void ShowOutcomeBasedOnTension()
    {
        int tension = 50; // Default to 50 if TensionMeter not found

        if (TensionMeter.Instance != null)
        {
            tension = TensionMeter.Instance.CurrentTension;
            Debug.Log($"[TextControllerOutro] TensionMeter found. Tension = {tension}");
        }
        else
        {
            Debug.LogWarning("[TextControllerOutro] TensionMeter.Instance not found! Using default tension of 50.");
        }

        bool playerWon = tension < 50;
        Debug.Log($"[TextControllerOutro] Showing {(playerWon ? "WIN" : "LOSE")} outcome (tension: {tension})");
        
        ShowTextByOutcome(playerWon, autoHide);
        
        // Play the appropriate ending dialogue from endings.json
        StartCoroutine(PlayEndingDialogue(playerWon));
    }

    /// <summary>
    /// Plays the ending dialogue lines from endings.json based on win/lose
    /// </summary>
    private IEnumerator PlayEndingDialogue(bool playerWon)
    {
        yield return new WaitForSeconds(voiceStartDelay);

        if (endingsData == null)
        {
            Debug.LogWarning("[TextControllerOutro] No endings data loaded");
            yield break;
        }

        // Select the correct ending based on tension
        EndingData ending = playerWon ? endingsData.goodEnding : endingsData.badEnding;
        
        if (ending?.lines == null || ending.lines.Length == 0)
        {
            Debug.Log($"[TextControllerOutro] No lines for {(playerWon ? "good" : "bad")} ending");
            yield break;
        }

        Debug.Log($"[TextControllerOutro] Playing {(playerWon ? "GOOD" : "BAD")} ending with {ending.lines.Length} lines");

        foreach (var line in ending.lines)
        {
            if (line == null) continue;

            // Show subtitle if SubtitleUI exists
            if (SubtitleUI.Instance != null)
            {
                SubtitleUI.Instance.ShowInvestigatorLine(line.text);
            }

            // Load and play voice clip
            if (!string.IsNullOrEmpty(line.voiceClipPath))
            {
                AudioClip clip = DialogueLoader.LoadVoiceClip(line.voiceClipPath);
                if (clip != null)
                {
                    Debug.Log($"[TextControllerOutro] Playing: {line.text}");
                    audioSource.clip = clip;
                    audioSource.Play();
                    
                    // Wait for clip to finish
                    yield return new WaitForSeconds(clip.length + delayBetweenLines);
                }
                else
                {
                    Debug.LogWarning($"[TextControllerOutro] Failed to load voice clip: {line.voiceClipPath}");
                    // Wait a default time if no clip
                    yield return new WaitForSeconds(2f);
                }
            }
            else
            {
                // No voice clip, wait based on text length
                float waitTime = line.text.Length * 0.05f + 1f;
                yield return new WaitForSeconds(waitTime);
            }
        }
        
        // Hide subtitle after all lines
        if (SubtitleUI.Instance != null)
        {
            yield return new WaitForSeconds(0.5f);
            SubtitleUI.Instance.Hide();
        }
        
        // Wait before transitioning back to initial screen
        yield return new WaitForSeconds(transitionDelay);
        
        // Transition back to initial screen
        Debug.Log($"[TextControllerOutro] Transitioning to {initialSceneName}");
        SceneManager.LoadScene(initialSceneName);
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
