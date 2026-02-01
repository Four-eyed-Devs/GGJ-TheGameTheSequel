using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Interrogation.Dialogue;

namespace Interrogation.UI
{
    /// <summary>
    /// Cinematic tension meter UI.
    /// Positioned top-right with minimal film noir aesthetic.
    /// </summary>
    public class TensionMeterUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider tensionSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI tensionLabel;
        [SerializeField] private TextMeshProUGUI tensionValueText;

        [Header("Cinematic Style")]
        [SerializeField] private bool createUIAtRuntime = true;
        [SerializeField] private float barWidth = 280f;
        [SerializeField] private float barHeight = 12f;
        [SerializeField] private float topMargin = 60f;
        [SerializeField] private float rightMargin = 50f;

        [Header("Colors")]
        [SerializeField] private Color accentColor = new Color(0.9f, 0.75f, 0.4f); // Gold/amber - matches dialogue
        [SerializeField] private Color calmColor = new Color(0.4f, 0.7f, 0.5f); // Muted teal-green
        [SerializeField] private Color stressedColor = new Color(0.85f, 0.35f, 0.3f); // Muted red
        [SerializeField] private Color backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        [SerializeField] private Color barBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        [Header("Animation")]
        [SerializeField] private float animationSpeed = 5f;
        [SerializeField] private bool animateChanges = true;
        [SerializeField] private float pulseDuration = 0.3f;
        [SerializeField] private float pulseIntensity = 0.2f;

        [Header("Labels")]
        [SerializeField] private string meterLabel = "TENSION";

        private float targetValue;
        private float currentValue;
        private Vector3 originalScale;
        private Coroutine pulseCoroutine;
        private Image leftAccent;
        private GameObject panelContainer;

        private void Awake()
        {
            if (createUIAtRuntime)
            {
                CreateCinematicUI();
            }

            if (tensionSlider != null)
            {
                originalScale = tensionSlider.transform.localScale;
            }
        }

        private void CreateCinematicUI()
        {
            // Find canvas
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }
            if (canvas == null)
            {
                Debug.LogError("[TensionMeterUI] No Canvas found!");
                return;
            }

            // Create main panel - top right positioning
            panelContainer = new GameObject("CinematicTensionMeter");
            panelContainer.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelContainer.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1); // Top-right anchor
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            panelRect.anchoredPosition = new Vector2(-rightMargin, -topMargin);
            panelRect.sizeDelta = new Vector2(barWidth + 50f, 75f);

            // Subtle background
            backgroundImage = panelContainer.AddComponent<Image>();
            backgroundImage.color = backgroundColor;
            backgroundImage.raycastTarget = false;

            // Left accent bar (matching dialogue style)
            GameObject accentObj = new GameObject("LeftAccent");
            accentObj.transform.SetParent(panelContainer.transform, false);
            RectTransform accentRect = accentObj.AddComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0, 0);
            accentRect.anchorMax = new Vector2(0, 1);
            accentRect.pivot = new Vector2(0, 0.5f);
            accentRect.anchoredPosition = Vector2.zero;
            accentRect.sizeDelta = new Vector2(4f, 0);
            leftAccent = accentObj.AddComponent<Image>();
            leftAccent.color = accentColor;
            leftAccent.raycastTarget = false;

            // Label - "TENSION"
            GameObject labelObj = new GameObject("TensionLabel");
            labelObj.transform.SetParent(panelContainer.transform, false);
            tensionLabel = labelObj.AddComponent<TextMeshProUGUI>();
            tensionLabel.text = "TENSION";
            tensionLabel.fontSize = 16f;
            tensionLabel.fontStyle = FontStyles.UpperCase | FontStyles.Bold;
            tensionLabel.characterSpacing = 6f;
            tensionLabel.color = accentColor;
            tensionLabel.alignment = TextAlignmentOptions.Left;
            tensionLabel.raycastTarget = false;

            RectTransform labelRect = tensionLabel.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.pivot = new Vector2(0, 1);
            labelRect.anchoredPosition = new Vector2(15f, -8f);
            labelRect.sizeDelta = new Vector2(-20f, 20f);

            // Value text - percentage
            GameObject valueObj = new GameObject("TensionValue");
            valueObj.transform.SetParent(panelContainer.transform, false);
            tensionValueText = valueObj.AddComponent<TextMeshProUGUI>();
            tensionValueText.text = "50%";
            tensionValueText.fontSize = 16f;
            tensionValueText.fontStyle = FontStyles.Bold;
            tensionValueText.color = new Color(1f, 1f, 1f, 0.7f);
            tensionValueText.alignment = TextAlignmentOptions.Right;
            tensionValueText.raycastTarget = false;

            RectTransform valueRect = tensionValueText.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0, 1);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.pivot = new Vector2(1, 1);
            valueRect.anchoredPosition = new Vector2(-15f, -10f);
            valueRect.sizeDelta = new Vector2(-20f, 25f);

            // Slider container
            GameObject sliderObj = new GameObject("TensionSlider");
            sliderObj.transform.SetParent(panelContainer.transform, false);
            tensionSlider = sliderObj.AddComponent<Slider>();
            tensionSlider.minValue = 0f;
            tensionSlider.maxValue = 1f;
            tensionSlider.value = 0.5f;
            tensionSlider.interactable = false;

            RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 0);
            sliderRect.anchorMax = new Vector2(0.5f, 0);
            sliderRect.pivot = new Vector2(0.5f, 0);
            sliderRect.anchoredPosition = new Vector2(0, 12f);
            sliderRect.sizeDelta = new Vector2(barWidth, barHeight);

            // Slider background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = barBackgroundColor;
            bgImage.raycastTarget = false;

            RectTransform bgRect = bgImage.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Fill area
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            fillImage = fillObj.AddComponent<Image>();
            fillImage.color = calmColor;
            fillImage.raycastTarget = false;

            RectTransform fillRect = fillImage.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            tensionSlider.fillRect = fillRect;
            tensionSlider.targetGraphic = fillImage;

            originalScale = sliderRect.localScale;

            Debug.Log("[TensionMeterUI] Cinematic UI created - top-right positioning");
        }

        private void Start()
        {
            if (TensionMeter.Instance != null)
            {
                TensionMeter.Instance.OnTensionChanged += HandleTensionChanged;
                TensionMeter.Instance.OnTensionThresholdCrossed += HandleThresholdCrossed;

                // Initialize with current value
                float normalized = TensionMeter.Instance.TensionNormalized;
                currentValue = normalized;
                targetValue = normalized;
                UpdateUI(TensionMeter.Instance.CurrentTension, TensionMeter.Instance.IsHighTension);
            }
        }

        private void OnDestroy()
        {
            if (TensionMeter.Instance != null)
            {
                TensionMeter.Instance.OnTensionChanged -= HandleTensionChanged;
                TensionMeter.Instance.OnTensionThresholdCrossed -= HandleThresholdCrossed;
            }
        }

        private void Update()
        {
            if (animateChanges && Mathf.Abs(currentValue - targetValue) > 0.001f)
            {
                currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * animationSpeed);
                
                if (tensionSlider != null)
                {
                    tensionSlider.value = currentValue;
                }
            }
        }

        private void HandleTensionChanged(int newValue, int delta)
        {
            targetValue = newValue / 100f;

            if (!animateChanges)
            {
                currentValue = targetValue;
                if (tensionSlider != null)
                {
                    tensionSlider.value = currentValue;
                }
            }

            bool isHighTension = newValue >= 50;
            UpdateUI(newValue, isHighTension);

            // Pulse effect on change
            if (delta != 0 && pulseCoroutine == null)
            {
                pulseCoroutine = StartCoroutine(PulseEffect());
            }
        }

        private void HandleThresholdCrossed(bool isNowHighTension)
        {
            UpdateColors(TensionMeter.Instance?.CurrentTension ?? 50);
            Debug.Log($"[TensionMeterUI] Threshold crossed! Now: {(isNowHighTension ? "HIGH" : "LOW")} tension");
        }

        private void UpdateUI(int value, bool isHighTension)
        {
            if (tensionValueText != null)
            {
                tensionValueText.text = $"{value}%";
            }

            UpdateColors(value);
        }

        private void UpdateColors(int tension)
        {
            if (fillImage == null) return;

            // Smooth gradient: calm (green) at 100, stressed (red) at 0
            // High tension = good composure = green
            // Low tension = losing composure = red
            float t = tension / 100f;
            fillImage.color = Color.Lerp(stressedColor, calmColor, t);

            // Update accent color intensity based on stress
            if (leftAccent != null)
            {
                // Accent pulses more red when stressed
                Color currentAccent = Color.Lerp(stressedColor, accentColor, t);
                leftAccent.color = currentAccent;
            }
        }

        private System.Collections.IEnumerator PulseEffect()
        {
            if (fillImage == null)
            {
                pulseCoroutine = null;
                yield break;
            }

            Color originalColor = fillImage.color;
            float elapsed = 0f;
            float halfDuration = pulseDuration / 2f;

            // Brighten
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                fillImage.color = Color.Lerp(originalColor, originalColor + new Color(pulseIntensity, pulseIntensity, pulseIntensity, 0), t);
                yield return null;
            }

            elapsed = 0f;

            // Return to normal
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                fillImage.color = Color.Lerp(originalColor + new Color(pulseIntensity, pulseIntensity, pulseIntensity, 0), originalColor, t);
                yield return null;
            }

            // Refresh to proper color
            if (TensionMeter.Instance != null)
            {
                UpdateColors(TensionMeter.Instance.CurrentTension);
            }
            pulseCoroutine = null;
        }

        /// <summary>
        /// Force refresh the UI
        /// </summary>
        public void Refresh()
        {
            if (TensionMeter.Instance != null)
            {
                HandleTensionChanged(TensionMeter.Instance.CurrentTension, 0);
            }
        }
    }
}
