using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor utility to set up the Interrogation UI Canvas with all necessary components.
/// Run from Unity menu: Tools > Interrogation > Setup UI
/// </summary>
public class InterrogationUISetup : EditorWindow
{
    [MenuItem("Tools/Interrogation/Setup UI Canvas")]
    public static void SetupUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("InterrogationCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create dark background
        GameObject bgGO = CreatePanel("Background", canvasGO.transform);
        bgGO.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 1f);
        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        
        // === TOP PANEL: Detective Dialogue ===
        GameObject topPanel = CreatePanel("DetectivePanel", canvasGO.transform);
        topPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f, 0.95f);
        RectTransform topRT = topPanel.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0.05f, 0.7f);
        topRT.anchorMax = new Vector2(0.95f, 0.95f);
        topRT.offsetMin = Vector2.zero;
        topRT.offsetMax = Vector2.zero;
        
        // Detective label
        GameObject detectiveLabel = CreateTextElement("DetectiveLabel", topPanel.transform, "DETECTIVE", 24);
        RectTransform labelRT = detectiveLabel.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0, 0.8f);
        labelRT.anchorMax = new Vector2(1, 1);
        labelRT.offsetMin = new Vector2(20, 0);
        labelRT.offsetMax = new Vector2(-20, -10);
        detectiveLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.3f, 0.3f);
        detectiveLabel.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        
        // Detective dialogue text
        GameObject detectiveText = CreateTextElement("DetectiveDialogueText", topPanel.transform, "\"The detective's words will appear here...\"", 32);
        RectTransform textRT = detectiveText.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0);
        textRT.anchorMax = new Vector2(1, 0.8f);
        textRT.offsetMin = new Vector2(30, 20);
        textRT.offsetMax = new Vector2(-30, -10);
        detectiveText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
        
        // === CENTER PANEL: Inner Monologue ===
        GameObject centerPanel = CreatePanel("InnerMonologuePanel", canvasGO.transform);
        centerPanel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f, 0.8f);
        RectTransform centerRT = centerPanel.GetComponent<RectTransform>();
        centerRT.anchorMin = new Vector2(0.1f, 0.45f);
        centerRT.anchorMax = new Vector2(0.9f, 0.65f);
        centerRT.offsetMin = Vector2.zero;
        centerRT.offsetMax = Vector2.zero;
        
        GameObject monologueText = CreateTextElement("InnerMonologueText", centerPanel.transform, "<i>Your inner thoughts will guide you...</i>", 24);
        RectTransform monoRT = monologueText.GetComponent<RectTransform>();
        monoRT.anchorMin = Vector2.zero;
        monoRT.anchorMax = Vector2.one;
        monoRT.offsetMin = new Vector2(20, 10);
        monoRT.offsetMax = new Vector2(-20, -10);
        monologueText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.6f, 0.7f);
        monologueText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Italic;
        monologueText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        
        // === SUSPICION METER ===
        GameObject suspicionPanel = CreatePanel("SuspicionPanel", canvasGO.transform);
        suspicionPanel.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        RectTransform suspRT = suspicionPanel.GetComponent<RectTransform>();
        suspRT.anchorMin = new Vector2(0.05f, 0.35f);
        suspRT.anchorMax = new Vector2(0.95f, 0.42f);
        suspRT.offsetMin = Vector2.zero;
        suspRT.offsetMax = Vector2.zero;
        
        // Suspicion label
        GameObject suspLabel = CreateTextElement("SuspicionLabel", suspicionPanel.transform, "SUSPICION", 18);
        RectTransform suspLabelRT = suspLabel.GetComponent<RectTransform>();
        suspLabelRT.anchorMin = new Vector2(0, 0);
        suspLabelRT.anchorMax = new Vector2(0.15f, 1);
        suspLabelRT.offsetMin = new Vector2(10, 0);
        suspLabelRT.offsetMax = Vector2.zero;
        suspLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
        
        // Suspicion bar background
        GameObject barBG = CreatePanel("SuspicionBarBG", suspicionPanel.transform);
        barBG.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform barBGRT = barBG.GetComponent<RectTransform>();
        barBGRT.anchorMin = new Vector2(0.15f, 0.2f);
        barBGRT.anchorMax = new Vector2(0.85f, 0.8f);
        barBGRT.offsetMin = Vector2.zero;
        barBGRT.offsetMax = Vector2.zero;
        
        // Suspicion bar fill
        GameObject barFill = CreatePanel("SuspicionFill", barBG.transform);
        barFill.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
        barFill.GetComponent<Image>().type = Image.Type.Filled;
        barFill.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;
        barFill.GetComponent<Image>().fillAmount = 0.3f;
        RectTransform fillRT = barFill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        
        // Suspicion percent text
        GameObject suspPercent = CreateTextElement("SuspicionPercentText", suspicionPanel.transform, "0%", 20);
        RectTransform percentRT = suspPercent.GetComponent<RectTransform>();
        percentRT.anchorMin = new Vector2(0.85f, 0);
        percentRT.anchorMax = new Vector2(1, 1);
        percentRT.offsetMin = Vector2.zero;
        percentRT.offsetMax = new Vector2(-10, 0);
        suspPercent.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineRight;
        
        // === BOTTOM PANEL: Mask Buttons ===
        GameObject bottomPanel = CreatePanel("MaskButtonPanel", canvasGO.transform);
        bottomPanel.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.15f, 0.95f);
        RectTransform bottomRT = bottomPanel.GetComponent<RectTransform>();
        bottomRT.anchorMin = new Vector2(0.05f, 0.05f);
        bottomRT.anchorMax = new Vector2(0.95f, 0.32f);
        bottomRT.offsetMin = Vector2.zero;
        bottomRT.offsetMax = Vector2.zero;
        
        // Horizontal layout for buttons
        HorizontalLayoutGroup hlg = bottomPanel.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.padding = new RectOffset(30, 30, 20, 20);
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        
        // Create 4 mask buttons
        string[] maskNames = { "The Stoic", "The Victim", "The Hothead", "The Charmer" };
        Color[] maskColors = {
            new Color(0.3f, 0.5f, 0.8f, 1f),   // Logic - Blue
            new Color(0.8f, 0.4f, 0.6f, 1f),   // Emotion - Pink
            new Color(0.8f, 0.3f, 0.3f, 1f),   // Aggression - Red
            new Color(0.8f, 0.7f, 0.3f, 1f)    // Charm - Gold
        };
        
        for (int i = 0; i < 4; i++)
        {
            GameObject buttonGO = CreateMaskButton($"MaskButton_{i}", bottomPanel.transform, maskNames[i], maskColors[i]);
        }
        
        // === GAME OVER PANEL (hidden by default) ===
        GameObject gameOverPanel = CreatePanel("GameOverPanel", canvasGO.transform);
        gameOverPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.9f);
        RectTransform goRT = gameOverPanel.GetComponent<RectTransform>();
        goRT.anchorMin = Vector2.zero;
        goRT.anchorMax = Vector2.one;
        goRT.offsetMin = Vector2.zero;
        goRT.offsetMax = Vector2.zero;
        gameOverPanel.SetActive(false);
        
        GameObject gameOverText = CreateTextElement("GameOverText", gameOverPanel.transform, "GAME OVER", 64);
        RectTransform goTextRT = gameOverText.GetComponent<RectTransform>();
        goTextRT.anchorMin = new Vector2(0, 0.4f);
        goTextRT.anchorMax = new Vector2(1, 0.7f);
        goTextRT.offsetMin = Vector2.zero;
        goTextRT.offsetMax = Vector2.zero;
        gameOverText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        
        // Restart button
        GameObject restartBtn = CreateButton("RestartButton", gameOverPanel.transform, "TRY AGAIN");
        RectTransform restartRT = restartBtn.GetComponent<RectTransform>();
        restartRT.anchorMin = new Vector2(0.35f, 0.25f);
        restartRT.anchorMax = new Vector2(0.65f, 0.35f);
        restartRT.offsetMin = Vector2.zero;
        restartRT.offsetMax = Vector2.zero;
        
        // === Create GameManager and UIManager GameObjects ===
        GameObject managerGO = new GameObject("GameManager");
        managerGO.AddComponent<GameManager>();
        
        GameObject uiManagerGO = new GameObject("UIManager");
        UIManager uiManager = uiManagerGO.AddComponent<UIManager>();
        
        // Mark scene dirty for saving
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("[InterrogationUISetup] UI Canvas created successfully! Remember to:");
        Debug.Log("  1. Assign CardData assets to GameManager.masks array");
        Debug.Log("  2. Assign DialogueNode assets to GameManager.dialogueSequence array");
        Debug.Log("  3. Wire up UIManager serialized fields to UI elements");
        Debug.Log("  4. Add MaskButton component to each mask button and assign CardData");
        
        Selection.activeGameObject = canvasGO;
    }
    
    private static GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        panel.AddComponent<RectTransform>();
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        return panel;
    }
    
    private static GameObject CreateTextElement(string name, Transform parent, string text, int fontSize)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);
        textGO.AddComponent<RectTransform>();
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return textGO;
    }
    
    private static GameObject CreateMaskButton(string name, Transform parent, string buttonText, Color accentColor)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);
        buttonGO.AddComponent<RectTransform>();
        
        Image btnImage = buttonGO.AddComponent<Image>();
        btnImage.color = new Color(0.15f, 0.15f, 0.18f, 1f);
        
        Button btn = buttonGO.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.18f, 1f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.12f, 1f);
        colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        btn.colors = colors;
        
        // Add MaskButton component
        buttonGO.AddComponent<MaskButton>();
        
        // Accent bar at top
        GameObject accent = CreatePanel("Accent", buttonGO.transform);
        accent.GetComponent<Image>().color = accentColor;
        RectTransform accentRT = accent.GetComponent<RectTransform>();
        accentRT.anchorMin = new Vector2(0, 0.9f);
        accentRT.anchorMax = Vector2.one;
        accentRT.offsetMin = Vector2.zero;
        accentRT.offsetMax = Vector2.zero;
        
        // Mask name
        GameObject nameText = CreateTextElement("NameText", buttonGO.transform, buttonText, 22);
        RectTransform nameRT = nameText.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0, 0.4f);
        nameRT.anchorMax = new Vector2(1, 0.85f);
        nameRT.offsetMin = new Vector2(10, 0);
        nameRT.offsetMax = new Vector2(-10, 0);
        nameText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        
        // Durability text
        GameObject durabilityText = CreateTextElement("DurabilityText", buttonGO.transform, "3/3", 18);
        RectTransform durRT = durabilityText.GetComponent<RectTransform>();
        durRT.anchorMin = new Vector2(0, 0.1f);
        durRT.anchorMax = new Vector2(1, 0.4f);
        durRT.offsetMin = new Vector2(10, 0);
        durRT.offsetMax = new Vector2(-10, 0);
        durabilityText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.6f, 0.6f);
        
        return buttonGO;
    }
    
    private static GameObject CreateButton(string name, Transform parent, string buttonText)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);
        buttonGO.AddComponent<RectTransform>();
        
        Image btnImage = buttonGO.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.35f, 1f);
        
        Button btn = buttonGO.AddComponent<Button>();
        
        GameObject textGO = CreateTextElement("Text", buttonGO.transform, buttonText, 24);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        
        return buttonGO;
    }
    
    [MenuItem("Tools/Interrogation/Clear Save Data")]
    public static void ClearSaveData()
    {
        GameSaveData.ClearSave();
        Debug.Log("[InterrogationUISetup] Save data cleared!");
    }
}
