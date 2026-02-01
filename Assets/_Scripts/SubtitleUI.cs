using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Interrogation.Dialogue
{
    /// <summary>
    /// Displays dialogue subtitles with cinematic style.
    /// Positioned top-left for film noir aesthetic.
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
        [SerializeField] private Color investigatorColor = new Color(0.9f, 0.75f, 0.4f); // Gold/amber - cinematic
        [SerializeField] private Color playerColor = new Color(0.7f, 0.7f, 0.7f); // Light gray
        [SerializeField] private Color textColor = new Color(1f, 1f, 1f, 0.95f); // Slightly transparent white

        [Header("Cinematic Style")]
        [SerializeField] private bool createUIAtRuntime = true;
        [SerializeField] private float maxTextWidth = 600f;
        [SerializeField] private float padding = 30f;
        [SerializeField] private float topMargin = 60f;
        [SerializeField] private float leftMargin = 50f;
        [SerializeField] private bool showSpeakerLine = true; // Decorative line under speaker name

        private Coroutine currentTypewriterCoroutine;
        private Coroutine currentFadeCoroutine;
        private Coroutine autoHideCoroutine;
        private bool isTyping;
        private string fullText;
        private Image speakerUnderline;
        private Image leftAccent;

        public bool IsTyping => isTyping;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (createUIAtRuntime)
            {
                CreateCinematicUI();
            }

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

        private void CreateCinematicUI()
        {
            // Find or create canvas
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }
            if (canvas == null)
            {
                Debug.LogError("[SubtitleUI] No Canvas found for cinematic UI!");
                return;
            }

            // Create subtitle panel - top left positioning
            subtitlePanel = new GameObject("CinematicSubtitlePanel");
            subtitlePanel.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = subtitlePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1); // Top-left anchor
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.anchoredPosition = new Vector2(leftMargin, -topMargin);
            panelRect.sizeDelta = new Vector2(maxTextWidth + padding * 2, 150f);

            // Add subtle background gradient (optional - semi-transparent)
            Image panelBg = subtitlePanel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.4f); // Subtle dark overlay
            panelBg.raycastTarget = false;

            // Add canvas group for fading
            canvasGroup = subtitlePanel.AddComponent<CanvasGroup>();

            // Create vertical layout
            VerticalLayoutGroup layout = subtitlePanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset((int)padding, (int)padding, (int)(padding * 0.5f), (int)(padding * 0.5f));
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter panelFitter = subtitlePanel.AddComponent<ContentSizeFitter>();
            panelFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            panelFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Create left accent bar (cinematic touch)
            GameObject accentObj = new GameObject("LeftAccent");
            accentObj.transform.SetParent(subtitlePanel.transform, false);
            RectTransform accentRect = accentObj.AddComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0, 0);
            accentRect.anchorMax = new Vector2(0, 1);
            accentRect.pivot = new Vector2(0, 0.5f);
            accentRect.anchoredPosition = new Vector2(0, 0);
            accentRect.sizeDelta = new Vector2(4f, 0);
            leftAccent = accentObj.AddComponent<Image>();
            leftAccent.color = investigatorColor;
            leftAccent.raycastTarget = false;

            // Create speaker label container with underline
            GameObject speakerContainer = new GameObject("SpeakerContainer");
            speakerContainer.transform.SetParent(subtitlePanel.transform, false);
            RectTransform speakerContRect = speakerContainer.AddComponent<RectTransform>();
            LayoutElement speakerContLayout = speakerContainer.AddComponent<LayoutElement>();
            speakerContLayout.preferredHeight = 35f;

            // Speaker label - cinematic uppercase style
            GameObject speakerObj = new GameObject("SpeakerLabel");
            speakerObj.transform.SetParent(speakerContainer.transform, false);
            speakerLabel = speakerObj.AddComponent<TextMeshProUGUI>();
            speakerLabel.fontSize = 14f;
            speakerLabel.fontStyle = FontStyles.UpperCase | FontStyles.Bold;
            speakerLabel.characterSpacing = 8f; // Wide letter spacing
            speakerLabel.color = investigatorColor;
            speakerLabel.alignment = TextAlignmentOptions.TopLeft;
            speakerLabel.raycastTarget = false;

            RectTransform speakerRect = speakerLabel.GetComponent<RectTransform>();
            speakerRect.anchorMin = Vector2.zero;
            speakerRect.anchorMax = Vector2.one;
            speakerRect.offsetMin = Vector2.zero;
            speakerRect.offsetMax = Vector2.zero;

            // Underline beneath speaker name
            if (showSpeakerLine)
            {
                GameObject underlineObj = new GameObject("SpeakerUnderline");
                underlineObj.transform.SetParent(speakerContainer.transform, false);
                RectTransform underlineRect = underlineObj.AddComponent<RectTransform>();
                underlineRect.anchorMin = new Vector2(0, 0);
                underlineRect.anchorMax = new Vector2(0.3f, 0);
                underlineRect.pivot = new Vector2(0, 0);
                underlineRect.anchoredPosition = new Vector2(0, 2);
                underlineRect.sizeDelta = new Vector2(0, 2f);
                speakerUnderline = underlineObj.AddComponent<Image>();
                speakerUnderline.color = investigatorColor;
                speakerUnderline.raycastTarget = false;
            }

            // Create subtitle text - main dialogue
            GameObject textObj = new GameObject("SubtitleText");
            textObj.transform.SetParent(subtitlePanel.transform, false);
            subtitleText = textObj.AddComponent<TextMeshProUGUI>();
            subtitleText.fontSize = 22f;
            subtitleText.fontStyle = FontStyles.Italic;
            subtitleText.color = textColor;
            subtitleText.alignment = TextAlignmentOptions.TopLeft;
            subtitleText.enableWordWrapping = true;
            subtitleText.raycastTarget = false;
            subtitleText.lineSpacing = 8f;

            RectTransform textRect = subtitleText.GetComponent<RectTransform>();
            LayoutElement textLayout = textObj.AddComponent<LayoutElement>();
            textLayout.preferredWidth = maxTextWidth;
            textLayout.flexibleWidth = 1f;

            Debug.Log("[SubtitleUI] Cinematic UI created - top-left positioning");
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

            // Update accent elements to match speaker
            if (speakerUnderline != null)
            {
                speakerUnderline.color = speakerColor;
            }
            if (leftAccent != null)
            {
                leftAccent.color = speakerColor;
            }

            subtitleText.color = textColor;

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

            if (subtitleText != null)
            {
                subtitleText.text = "";
            }
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
