using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Interrogation.Dialogue
{
    /// <summary>
    /// Editor tool to quickly set up the interrogation UI in the scene.
    /// Creates all necessary UI elements for subtitles, tension meter, and managers.
    /// </summary>
    public class InterrogationSetup : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("Interrogation/Setup Scene")]
        public static void SetupScene()
        {
            // Create Managers GameObject
            CreateManagers();
            
            // Create UI Canvas
            CreateInterrogationUI();
            
            Debug.Log("[InterrogationSetup] Scene setup complete!");
        }

        private static void CreateManagers()
        {
            // Check if managers already exist
            if (FindFirstObjectByType<DialogueManager>() != null)
            {
                Debug.Log("[InterrogationSetup] DialogueManager already exists");
                return;
            }

            GameObject managersObj = new GameObject("InterrogationManagers");
            
            // Add DialogueManager
            managersObj.AddComponent<DialogueManager>();
            
            // Add TensionMeter
            managersObj.AddComponent<TensionMeter>();
            
            // Add VoiceManager
            managersObj.AddComponent<VoiceManager>();
            
            Debug.Log("[InterrogationSetup] Created InterrogationManagers with DialogueManager, TensionMeter, VoiceManager");
        }

        private static void CreateInterrogationUI()
        {
            // Check if SubtitleUI already exists
            if (FindFirstObjectByType<SubtitleUI>() != null)
            {
                Debug.Log("[InterrogationSetup] SubtitleUI already exists");
                return;
            }

            // Find or create canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("InterrogationCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Create Subtitle Panel
            CreateSubtitlePanel(canvas.transform);
            
            // Create Tension Meter
            CreateTensionMeterUI(canvas.transform);

            Debug.Log("[InterrogationSetup] Created Interrogation UI elements");
        }

        private static void CreateSubtitlePanel(Transform parent)
        {
            // Subtitle Panel (bottom of screen)
            GameObject panelObj = new GameObject("SubtitlePanel");
            panelObj.transform.SetParent(parent, false);
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.05f);
            panelRect.anchorMax = new Vector2(0.9f, 0.25f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);
            panelImage.raycastTarget = false;

            CanvasGroup canvasGroup = panelObj.AddComponent<CanvasGroup>();

            // Speaker Label
            GameObject speakerObj = new GameObject("SpeakerLabel");
            speakerObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform speakerRect = speakerObj.AddComponent<RectTransform>();
            speakerRect.anchorMin = new Vector2(0, 0.75f);
            speakerRect.anchorMax = new Vector2(1, 1);
            speakerRect.offsetMin = new Vector2(20, 0);
            speakerRect.offsetMax = new Vector2(-20, -10);

            TextMeshProUGUI speakerText = speakerObj.AddComponent<TextMeshProUGUI>();
            speakerText.text = "Detective";
            speakerText.fontSize = 24;
            speakerText.fontStyle = FontStyles.Bold;
            speakerText.alignment = TextAlignmentOptions.Left;
            speakerText.color = new Color(0.8f, 0.2f, 0.2f);

            // Subtitle Text
            GameObject subtitleObj = new GameObject("SubtitleText");
            subtitleObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform subtitleRect = subtitleObj.AddComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0, 0);
            subtitleRect.anchorMax = new Vector2(1, 0.75f);
            subtitleRect.offsetMin = new Vector2(20, 10);
            subtitleRect.offsetMax = new Vector2(-20, -5);

            TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "";
            subtitleText.fontSize = 28;
            subtitleText.alignment = TextAlignmentOptions.Left;
            subtitleText.color = Color.white;

            // Add SubtitleUI component
            SubtitleUI subtitleUI = panelObj.AddComponent<SubtitleUI>();
            
            // Use reflection to set private fields (or make them public/use SerializeField properly)
            var subtitleTextField = typeof(SubtitleUI).GetField("subtitleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var speakerLabelField = typeof(SubtitleUI).GetField("speakerLabel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var subtitlePanelField = typeof(SubtitleUI).GetField("subtitlePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var canvasGroupField = typeof(SubtitleUI).GetField("canvasGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (subtitleTextField != null) subtitleTextField.SetValue(subtitleUI, subtitleText);
            if (speakerLabelField != null) speakerLabelField.SetValue(subtitleUI, speakerText);
            if (subtitlePanelField != null) subtitlePanelField.SetValue(subtitleUI, panelObj);
            if (canvasGroupField != null) canvasGroupField.SetValue(subtitleUI, canvasGroup);

            EditorUtility.SetDirty(subtitleUI);
        }

        private static void CreateTensionMeterUI(Transform parent)
        {
            // Tension Meter (top right)
            GameObject meterObj = new GameObject("TensionMeter");
            meterObj.transform.SetParent(parent, false);
            
            RectTransform meterRect = meterObj.AddComponent<RectTransform>();
            meterRect.anchorMin = new Vector2(0.7f, 0.9f);
            meterRect.anchorMax = new Vector2(0.95f, 0.98f);
            meterRect.offsetMin = Vector2.zero;
            meterRect.offsetMax = Vector2.zero;

            // Background
            Image bgImage = meterObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            bgImage.raycastTarget = false;

            // Slider
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(meterObj.transform, false);
            
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.05f, 0.2f);
            sliderRect.anchorMax = new Vector2(0.7f, 0.8f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 0.5f;
            slider.interactable = false;

            // Fill Area
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
            
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = new Color(0.8f, 0.7f, 0.3f);

            slider.fillRect = fillRect;

            // Value Text
            GameObject textObj = new GameObject("ValueText");
            textObj.transform.SetParent(meterObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.72f, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI valueText = textObj.AddComponent<TextMeshProUGUI>();
            valueText.text = "50/100";
            valueText.fontSize = 18;
            valueText.alignment = TextAlignmentOptions.Center;
            valueText.color = Color.white;

            // Add TensionMeterUI component
            var tensionMeterUI = meterObj.AddComponent<Interrogation.UI.TensionMeterUI>();
            
            // Set fields via reflection
            var sliderField = typeof(Interrogation.UI.TensionMeterUI).GetField("tensionSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fillImageField = typeof(Interrogation.UI.TensionMeterUI).GetField("fillImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tensionTextField = typeof(Interrogation.UI.TensionMeterUI).GetField("tensionText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (sliderField != null) sliderField.SetValue(tensionMeterUI, slider);
            if (fillImageField != null) fillImageField.SetValue(tensionMeterUI, fillImage);
            if (tensionTextField != null) tensionTextField.SetValue(tensionMeterUI, valueText);

            EditorUtility.SetDirty(tensionMeterUI);
        }
#endif
    }
}
