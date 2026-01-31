using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Interrogation.Dialogue;

namespace Interrogation.UI
{
    /// <summary>
    /// Visual UI for the tension meter.
    /// Shows a bar/slider that fills based on tension level.
    /// Changes color based on high/low tension threshold.
    /// </summary>
    public class TensionMeterUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider tensionSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI tensionText;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Colors")]
        [SerializeField] private Color lowTensionColor = new Color(0.2f, 0.6f, 0.2f); // Green - good
        [SerializeField] private Color highTensionColor = new Color(0.8f, 0.3f, 0.3f); // Red - bad
        [SerializeField] private Color neutralColor = new Color(0.8f, 0.7f, 0.3f); // Yellow - at threshold

        [Header("Animation")]
        [SerializeField] private float animationSpeed = 5f;
        [SerializeField] private bool animateChanges = true;
        [SerializeField] private float pulseOnChange = 1f;
        [SerializeField] private float pulseDuration = 0.3f;
        [SerializeField] private float pulseScale = 1.1f;

        [Header("Status Text")]
        [SerializeField] private string lowTensionStatus = "Suspicious";
        [SerializeField] private string highTensionStatus = "Composed";

        private float targetValue;
        private float currentValue;
        private Vector3 originalScale;
        private Coroutine pulseCoroutine;

        private void Awake()
        {
            if (tensionSlider != null)
            {
                originalScale = tensionSlider.transform.localScale;
            }
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
            UpdateColors(isNowHighTension);
            
            if (statusText != null)
            {
                statusText.text = isNowHighTension ? highTensionStatus : lowTensionStatus;
            }

            Debug.Log($"[TensionMeterUI] Threshold crossed! Now: {(isNowHighTension ? "HIGH" : "LOW")} tension");
        }

        private void UpdateUI(int value, bool isHighTension)
        {
            if (tensionText != null)
            {
                tensionText.text = $"{value}/100";
            }

            UpdateColors(isHighTension);
        }

        private void UpdateColors(bool isHighTension)
        {
            if (fillImage != null)
            {
                // Gradient based on value
                if (TensionMeter.Instance != null)
                {
                    int tension = TensionMeter.Instance.CurrentTension;
                    
                    if (tension == 50)
                    {
                        fillImage.color = neutralColor;
                    }
                    else if (tension > 50)
                    {
                        // Lerp from neutral to high (more red = worse)
                        float t = (tension - 50) / 50f;
                        fillImage.color = Color.Lerp(neutralColor, highTensionColor, t);
                    }
                    else
                    {
                        // Lerp from neutral to low (more green = worse actually, means we're losing composure)
                        float t = (50 - tension) / 50f;
                        fillImage.color = Color.Lerp(neutralColor, lowTensionColor, t);
                    }
                }
                else
                {
                    fillImage.color = isHighTension ? highTensionColor : lowTensionColor;
                }
            }

            if (statusText != null)
            {
                statusText.color = isHighTension ? highTensionColor : lowTensionColor;
            }
        }

        private System.Collections.IEnumerator PulseEffect()
        {
            if (tensionSlider == null)
            {
                pulseCoroutine = null;
                yield break;
            }

            Transform target = tensionSlider.transform;
            float elapsed = 0f;
            float halfDuration = pulseDuration / 2f;

            // Scale up
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                target.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, t);
                yield return null;
            }

            elapsed = 0f;

            // Scale down
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                target.localScale = Vector3.Lerp(originalScale * pulseScale, originalScale, t);
                yield return null;
            }

            target.localScale = originalScale;
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
