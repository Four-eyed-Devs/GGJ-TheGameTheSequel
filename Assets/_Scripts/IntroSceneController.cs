using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the intro scene with typewriter text effect.
/// Automatically transitions to the game scene when text is complete.
/// </summary>
public class IntroSceneController : MonoBehaviour
{
    [Header("Text Settings")]
    [TextArea(5, 20)]
    [SerializeField] private string introText = @"The city never sleeps, but tonight it holds its breath.

A mask was found at the scene. Just one.

But everyone in that room was wearing a face that wasn't their own.

Now it's your job to find the truth...

...before the mask falls.";

    [Header("Typewriter Settings")]
    [SerializeField] private float charactersPerSecond = 30f;
    [SerializeField] private float delayAfterComplete = 2f;
    [SerializeField] private float initialDelay = 1f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI introTextUI;
    [SerializeField] private CanvasGroup textCanvasGroup;

    [Header("Skip Settings")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private TextMeshProUGUI skipHintText;

    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "SampleScene";

    private bool isTyping = false;
    private bool isComplete = false;
    private Coroutine typewriterCoroutine;

    private void Start()
    {
        // Initialize
        if (introTextUI != null)
        {
            introTextUI.text = "";
        }

        if (skipHintText != null)
        {
            skipHintText.text = allowSkip ? "Press SPACE to skip" : "";
            skipHintText.alpha = 0.5f;
        }

        // Start the intro sequence
        StartCoroutine(IntroSequence());
    }

    private void Update()
    {
        // Handle skip input
        if (allowSkip && Input.GetKeyDown(skipKey))
        {
            if (isTyping)
            {
                // Skip to end of text
                SkipTypewriter();
            }
            else if (isComplete)
            {
                // Skip wait and go to next scene
                TransitionToNextScene();
            }
        }

        // Also allow mouse click to skip
        if (allowSkip && Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                SkipTypewriter();
            }
            else if (isComplete)
            {
                TransitionToNextScene();
            }
        }
    }

    private IEnumerator IntroSequence()
    {
        // Wait for fade in to complete
        if (SceneTransitionManager.Instance != null)
        {
            yield return SceneTransitionManager.Instance.FadeIn();
        }

        // Initial delay
        yield return new WaitForSeconds(initialDelay);

        // Start typewriter effect
        typewriterCoroutine = StartCoroutine(TypewriterEffect());
        yield return typewriterCoroutine;

        // Mark as complete
        isComplete = true;

        // Update skip hint
        if (skipHintText != null)
        {
            skipHintText.text = "Press SPACE to continue";
        }

        // Wait before auto-transitioning
        yield return new WaitForSeconds(delayAfterComplete);

        // Transition to next scene
        TransitionToNextScene();
    }

    private IEnumerator TypewriterEffect()
    {
        isTyping = true;
        introTextUI.text = "";

        float delay = 1f / charactersPerSecond;

        foreach (char c in introText)
        {
            introTextUI.text += c;

            // Longer pause for punctuation
            if (c == '.' || c == '?' || c == '!')
            {
                yield return new WaitForSeconds(delay * 5f);
            }
            else if (c == ',')
            {
                yield return new WaitForSeconds(delay * 2f);
            }
            else if (c == '\n')
            {
                yield return new WaitForSeconds(delay * 3f);
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }
        }

        isTyping = false;
    }

    private void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        introTextUI.text = introText;
        isTyping = false;
        isComplete = true;

        if (skipHintText != null)
        {
            skipHintText.text = "Press SPACE to continue";
        }
    }

    private void TransitionToNextScene()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(nextSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}
