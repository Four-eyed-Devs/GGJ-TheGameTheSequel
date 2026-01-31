using System.Collections;
using UnityEngine;
using TMPro;

namespace Interrogation.Dialogue
{
    /// <summary>
    /// Displays dialogue subtitles with optional typewriter effect.
    /// Shows both investigator and player lines.
    /// </summary>
    public class SubtitleUI : MonoBehaviour
    {
        public static SubtitleUI Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private GameObject subtitlePanel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Speaker Labels")]
        [SerializeField] private TextMeshProUGUI speakerLabel;
        [SerializeField] private string investigatorName = "Detective";
        [SerializeField] private string playerName = "You";

        [Header("Typewriter Settings")]
        [SerializeField] private bool useTypewriter = true;
        [SerializeField] private float charactersPerSecond = 40f;
        [SerializeField] private float punctuationDelay = 0.2f;

        [Header("Fade Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float autoHideDelay = 2f;

        [Header("Colors")]
        [SerializeField] private Color investigatorColor = new Color(0.8f, 0.2f, 0.2f); // Red
        [SerializeField] private Color playerColor = new Color(0.2f, 0.6f, 0.8f); // Blue

        private Coroutine currentTypewriterCoroutine;
        private Coroutine currentFadeCoroutine;
        private Coroutine autoHideCoroutine;
        private bool isTyping;
        private string fullText;

        public bool IsTyping => isTyping;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Ensure we have a canvas group for fading
            if (canvasGroup == null)
            {
                canvasGroup = subtitlePanel?.GetComponent<CanvasGroup>();
                if (canvasGroup == null && subtitlePanel != null)
                {
                    canvasGroup = subtitlePanel.AddComponent<CanvasGroup>();
                }
            }

            HideImmediate();
        }

        /// <summary>
        /// Show investigator dialogue line
        /// </summary>
        public void ShowInvestigatorLine(string text, float? displayDuration = null)
        {
            ShowLine(text, investigatorName, investigatorColor, displayDuration);
        }

        /// <summary>
        /// Show player dialogue line
        /// </summary>
        public void ShowPlayerLine(string text, float? displayDuration = null)
        {
            ShowLine(text, playerName, playerColor, displayDuration);
        }

        /// <summary>
        /// Show a dialogue line with custom speaker
        /// </summary>
        public void ShowLine(string text, string speakerName, Color speakerColor, float? displayDuration = null)
        {
            Debug.Log($"[SubtitleUI] ShowLine called: speaker={speakerName}, text={text}");
            
            if (subtitleText == null)
            {
                Debug.LogError("[SubtitleUI] subtitleText is not assigned! Please assign in Inspector.");
                return;
            }
            
            StopAllSubtitleCoroutines();

            fullText = text;

            if (speakerLabel != null)
            {
                speakerLabel.text = speakerName;
                speakerLabel.color = speakerColor;
            }

            subtitleText.color = Color.white;

            if (subtitlePanel != null)
            {
                subtitlePanel.SetActive(true);
            }

            currentFadeCoroutine = StartCoroutine(FadeIn());

            if (useTypewriter)
            {
                currentTypewriterCoroutine = StartCoroutine(TypewriterEffect(text));
            }
            else
            {
                subtitleText.text = text;
                isTyping = false;
            }

            // Auto-hide after duration if specified
            if (displayDuration.HasValue)
            {
                autoHideCoroutine = StartCoroutine(AutoHide(displayDuration.Value));
            }
        }

        /// <summary>
        /// Skip the typewriter effect and show full text
        /// </summary>
        public void SkipTypewriter()
        {
            if (isTyping && currentTypewriterCoroutine != null)
            {
                StopCoroutine(currentTypewriterCoroutine);
                subtitleText.text = fullText;
                isTyping = false;
            }
        }

        /// <summary>
        /// Hide subtitles with fade
        /// </summary>
        public void Hide()
        {
            StopAllSubtitleCoroutines();
            currentFadeCoroutine = StartCoroutine(FadeOut());
        }

        /// <summary>
        /// Hide subtitles immediately without fade
        /// </summary>
        public void HideImmediate()
        {
            StopAllSubtitleCoroutines();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (subtitlePanel != null)
            {
                subtitlePanel.SetActive(false);
            }

            subtitleText.text = "";
            isTyping = false;
        }

        /// <summary>
        /// Wait for typewriter to complete
        /// </summary>
        public IEnumerator WaitForTypewriter()
        {
            while (isTyping)
            {
                yield return null;
            }
        }

        private void StopAllSubtitleCoroutines()
        {
            if (currentTypewriterCoroutine != null)
            {
                StopCoroutine(currentTypewriterCoroutine);
                currentTypewriterCoroutine = null;
            }

            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }

            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }
        }

        private IEnumerator TypewriterEffect(string text)
        {
            isTyping = true;
            subtitleText.text = "";

            float delay = 1f / charactersPerSecond;

            foreach (char c in text)
            {
                subtitleText.text += c;

                // Add extra delay for punctuation
                if (c == '.' || c == '!' || c == '?' || c == ',')
                {
                    yield return new WaitForSeconds(punctuationDelay);
                }
                else
                {
                    yield return new WaitForSeconds(delay);
                }
            }

            isTyping = false;
        }

        private IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            if (canvasGroup == null)
            {
                if (subtitlePanel != null)
                {
                    subtitlePanel.SetActive(false);
                }
                yield break;
            }

            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;

            if (subtitlePanel != null)
            {
                subtitlePanel.SetActive(false);
            }
        }

        private IEnumerator AutoHide(float delay)
        {
            // Wait for typewriter to finish first
            yield return WaitForTypewriter();

            // Then wait the additional delay
            yield return new WaitForSeconds(delay);

            Hide();
        }
    }
}
