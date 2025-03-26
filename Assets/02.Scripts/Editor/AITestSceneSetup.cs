using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
/// <summary>
/// Editor utility to quickly set up a test scene for AIAgent
/// Creates all necessary GameObjects and components for testing
/// </summary>
public class AITestSceneSetup : EditorWindow
{
    // Core component references
    private SKUnityCoreConfig _coreConfig;

    // Test keywords
    private string _testKeywords = "hello,goodbye,help";

    /// <summary>
    /// Show the setup window
    /// </summary>
    [MenuItem("SK Unity/Create Test Scene")]
    public static void ShowWindow()
    {
        GetWindow<AITestSceneSetup>("AI Test Scene Setup");
    }

    /// <summary>
    /// Draw the editor window GUI
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Label("AI Agent Test Scene Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("This tool will create a complete test scene for the AIAgent.\n" +
                               "Make sure you have TMPro package installed.", MessageType.Info);

        EditorGUILayout.Space();

        _coreConfig = (SKUnityCoreConfig)EditorGUILayout.ObjectField(
            "Core Config Asset:",
            _coreConfig,
            typeof(SKUnityCoreConfig),
            false
        );

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Test Keywords (comma separated):");
        _testKeywords = EditorGUILayout.TextField(_testKeywords);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Test Scene", GUILayout.Height(30)))
        {
            CreateTestScene();
        }
    }

    /// <summary>
    /// Create a complete test scene
    /// </summary>
    private void CreateTestScene()
    {
        if (!ValidateDependencies())
            return;

        // Create AI system components
        GameObject aiSystem = CreateAISystem();
        GameObject aiAgent = CreateAIAgent(aiSystem);
        GameObject uiCanvas = CreateUICanvas();
        GameObject eventTester = CreateEventTester(aiAgent);

        // Set up tester component
        SetupAITester(uiCanvas, aiSystem, aiAgent);

        // Log success
        Debug.Log("AI Test Scene created successfully!");

        // Select the canvas for easy access
        Selection.activeGameObject = uiCanvas;
    }

    /// <summary>
    /// Validate required dependencies
    /// </summary>
    /// <returns>True if all dependencies are met</returns>
    private bool ValidateDependencies()
    {
        // Check if current scene is empty or user wants to proceed
        if (GameObject.FindObjectOfType<Camera>() != null)
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Scene Not Empty",
                "The current scene is not empty. It's recommended to create the test scene in a new scene. Proceed anyway?",
                "Proceed",
                "Cancel"
            );

            if (!proceed)
                return false;
        }

        // Check for core config
        if (_coreConfig == null)
        {
            EditorUtility.DisplayDialog(
                "Missing Core Config",
                "Please assign an SKUnityCoreConfig asset before creating the test scene.",
                "OK"
            );
            return false;
        }

        // Check for TMPro
        if (!IsTMProAvailable())
        {
            EditorUtility.DisplayDialog(
                "Missing TextMeshPro",
                "TextMeshPro package is required but not found. Please install it from Package Manager.",
                "OK"
            );
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check if TextMeshPro is available
    /// </summary>
    /// <returns>True if TMPro is available</returns>
    private bool IsTMProAvailable()
    {
        // Simple check by trying to use TMPro types
        try
        {
            var tmp = typeof(TextMeshProUGUI);
            return tmp != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Create AI system with core components
    /// </summary>
    /// <returns>AI System GameObject</returns>
    private GameObject CreateAISystem()
    {
        // Create system object
        GameObject aiSystem = new GameObject("AI_System");

        // Add core components
        SKUnityCore core = aiSystem.AddComponent<SKUnityCore>();

        // Assign config
        SerializedObject serializedCore = new SerializedObject(core);
        var configProperty = serializedCore.FindProperty("_config");
        configProperty.objectReferenceValue = _coreConfig;
        serializedCore.ApplyModifiedProperties();

        // Add request queue
        aiSystem.AddComponent<SKRequestQueue>();

        return aiSystem;
    }

    /// <summary>
    /// Create AI agent
    /// </summary>
    /// <param name="aiSystem">Reference to AI system</param>
    /// <returns>AI Agent GameObject</returns>
    private GameObject CreateAIAgent(GameObject aiSystem)
    {
        // Create agent object
        GameObject aiAgent = new GameObject("AI_Agent");

        // Add AIAgent component
        AIAgent agent = aiAgent.AddComponent<AIAgent>();

        // Set up references
        SerializedObject serializedAgent = new SerializedObject(agent);
        var coreProperty = serializedAgent.FindProperty("_semanticCore");
        coreProperty.objectReferenceValue = aiSystem.GetComponent<SKUnityCore>();
        serializedAgent.ApplyModifiedProperties();

        return aiAgent;
    }

    /// <summary>
    /// Create UI canvas with all UI elements
    /// </summary>
    /// <returns>Canvas GameObject</returns>
    private GameObject CreateUICanvas()
    {
        // Create canvas
        GameObject canvas = new GameObject("UI_Canvas");
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();

        // Create panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvas.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.offsetMin = new Vector2(20, 20);
        panelRect.offsetMax = new Vector2(-20, -20);

        // Create title
        GameObject title = CreateTextObject("Title", panel.transform, "AI Agent Test", 24);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = new Vector2(10, -50);
        titleRect.offsetMax = new Vector2(-10, -10);

        // Create status text
        GameObject status = CreateTextObject("StatusText", panel.transform, "Ready to test.", 14);
        RectTransform statusRect = status.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 1);
        statusRect.anchorMax = new Vector2(1, 1);
        statusRect.offsetMin = new Vector2(10, -80);
        statusRect.offsetMax = new Vector2(-10, -50);

        // Create output scroll view
        GameObject scrollView = CreateScrollView(panel.transform, "OutputScrollView");
        RectTransform scrollViewRect = scrollView.GetComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0, 0.2f);
        scrollViewRect.anchorMax = new Vector2(1, 0.9f);
        scrollViewRect.offsetMin = new Vector2(10, 10);
        scrollViewRect.offsetMax = new Vector2(-10, -10);

        // Create content and output text
        GameObject outputText = CreateTextObject("OutputText", scrollView.transform.Find("Viewport/Content"), "", 16);
        outputText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
        RectTransform outputTextRect = outputText.GetComponent<RectTransform>();
        outputTextRect.anchorMin = new Vector2(0, 0);
        outputTextRect.anchorMax = new Vector2(1, 1);
        outputTextRect.offsetMin = new Vector2(5, 5);
        outputTextRect.offsetMax = new Vector2(-5, -5);

        // Create input area
        GameObject inputGroup = new GameObject("InputGroup");
        inputGroup.transform.SetParent(panel.transform, false);
        RectTransform inputGroupRect = inputGroup.AddComponent<RectTransform>();
        inputGroupRect.anchorMin = new Vector2(0, 0);
        inputGroupRect.anchorMax = new Vector2(1, 0.2f);
        inputGroupRect.offsetMin = new Vector2(10, 10);
        inputGroupRect.offsetMax = new Vector2(-10, -10);

        // Create input field
        GameObject inputField = CreateInputField(inputGroup.transform, "InputField", "Type your message here...");
        RectTransform inputFieldRect = inputField.GetComponent<RectTransform>();
        inputFieldRect.anchorMin = new Vector2(0, 0.3f);
        inputFieldRect.anchorMax = new Vector2(1, 1);
        inputFieldRect.offsetMin = new Vector2(0, 0);
        inputFieldRect.offsetMax = new Vector2(0, 0);

        // Create buttons
        GameObject buttonGroup = new GameObject("ButtonGroup");
        buttonGroup.transform.SetParent(inputGroup.transform, false);
        RectTransform buttonGroupRect = buttonGroup.AddComponent<RectTransform>();
        buttonGroupRect.anchorMin = new Vector2(0, 0);
        buttonGroupRect.anchorMax = new Vector2(1, 0.3f);
        buttonGroupRect.offsetMin = new Vector2(0, 0);
        buttonGroupRect.offsetMax = new Vector2(0, -5);

        // Create send button
        GameObject sendButton = CreateButton(buttonGroup.transform, "SendButton", "Send");
        RectTransform sendButtonRect = sendButton.GetComponent<RectTransform>();
        sendButtonRect.anchorMin = new Vector2(0.7f, 0);
        sendButtonRect.anchorMax = new Vector2(1, 1);
        sendButtonRect.offsetMin = new Vector2(5, 0);
        sendButtonRect.offsetMax = new Vector2(0, 0);

        // Create clear button
        GameObject clearButton = CreateButton(buttonGroup.transform, "ClearButton", "Clear");
        RectTransform clearButtonRect = clearButton.GetComponent<RectTransform>();
        clearButtonRect.anchorMin = new Vector2(0.35f, 0);
        clearButtonRect.anchorMax = new Vector2(0.7f, 1);
        clearButtonRect.offsetMin = new Vector2(5, 0);
        clearButtonRect.offsetMax = new Vector2(-5, 0);

        // Create cancel button
        GameObject cancelButton = CreateButton(buttonGroup.transform, "CancelButton", "Cancel");
        RectTransform cancelButtonRect = cancelButton.GetComponent<RectTransform>();
        cancelButtonRect.anchorMin = new Vector2(0, 0);
        cancelButtonRect.anchorMax = new Vector2(0.35f, 1);
        cancelButtonRect.offsetMin = new Vector2(0, 0);
        cancelButtonRect.offsetMax = new Vector2(-5, 0);

        // Create keyword indicator
        GameObject keywordPanel = new GameObject("KeywordPanel");
        keywordPanel.transform.SetParent(panel.transform, false);
        Image keywordImage = keywordPanel.AddComponent<Image>();
        keywordImage.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
        RectTransform keywordRect = keywordPanel.GetComponent<RectTransform>();
        keywordRect.anchorMin = new Vector2(0.7f, 0.9f);
        keywordRect.anchorMax = new Vector2(1, 1);
        keywordRect.offsetMin = new Vector2(10, 10);
        keywordRect.offsetMax = new Vector2(-10, -10);

        // Create keyword text
        GameObject keywordText = CreateTextObject("KeywordText", keywordPanel.transform, "Keyword", 14);
        keywordText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        RectTransform keywordTextRect = keywordText.GetComponent<RectTransform>();
        keywordTextRect.anchorMin = new Vector2(0, 0);
        keywordTextRect.anchorMax = new Vector2(1, 1);

        // Disable keyword panel initially
        keywordPanel.SetActive(false);

        return canvas;
    }

    /// <summary>
    /// Create event tester object
    /// </summary>
    /// <param name="aiAgent">AI Agent reference</param>
    /// <returns>Event tester GameObject</returns>
    private GameObject CreateEventTester(GameObject aiAgent)
    {
        // Create event tester object
        GameObject eventTester = new GameObject("EventTester");

        // Add TriggerEventNode component
        TriggerEventNode triggerNode = eventTester.AddComponent<TriggerEventNode>();

        // Set up references
        SerializedObject serializedTrigger = new SerializedObject(triggerNode);
        var agentProperty = serializedTrigger.FindProperty("_aiAgent");
        agentProperty.objectReferenceValue = aiAgent.GetComponent<AIAgent>();

        // Set up keyword
        var keywordProperty = serializedTrigger.FindProperty("_keywordToDetect");
        keywordProperty.stringValue = "hello";

        // Enable debug mode
        var debugProperty = serializedTrigger.FindProperty("_debugMode");
        debugProperty.boolValue = true;

        serializedTrigger.ApplyModifiedProperties();

        return eventTester;
    }

    /// <summary>
    /// Set up AIAgentTester component on canvas
    /// </summary>
    /// <param name="canvas">Canvas GameObject</param>
    /// <param name="aiSystem">AI System GameObject</param>
    /// <param name="aiAgent">AI Agent GameObject</param>
    private void SetupAITester(GameObject canvas, GameObject aiSystem, GameObject aiAgent)
    {
        // Add tester component
        AIAgentTester tester = canvas.AddComponent<AIAgentTester>();

        // Get references
        Transform panel = canvas.transform.Find("Panel");
        Transform inputField = panel.Find("InputGroup/InputField");
        Transform sendButton = panel.Find("InputGroup/ButtonGroup/SendButton");
        Transform clearButton = panel.Find("InputGroup/ButtonGroup/ClearButton");
        Transform cancelButton = panel.Find("InputGroup/ButtonGroup/CancelButton");
        Transform statusText = panel.Find("StatusText");
        Transform outputText = panel.Find("OutputScrollView/Viewport/Content/OutputText");
        Transform scrollView = panel.Find("OutputScrollView");
        Transform keywordPanel = panel.Find("KeywordPanel");
        Transform keywordText = panel.Find("KeywordPanel/KeywordText");

        // Set up references
        SerializedObject serializedTester = new SerializedObject(tester);

        serializedTester.FindProperty("_semanticCore").objectReferenceValue = aiSystem.GetComponent<SKUnityCore>();
        serializedTester.FindProperty("_aiAgent").objectReferenceValue = aiAgent.GetComponent<AIAgent>();
        serializedTester.FindProperty("_inputField").objectReferenceValue = inputField.GetComponent<TMP_InputField>();
        serializedTester.FindProperty("_sendButton").objectReferenceValue = sendButton.GetComponent<Button>();
        serializedTester.FindProperty("_clearButton").objectReferenceValue = clearButton.GetComponent<Button>();
        serializedTester.FindProperty("_cancelButton").objectReferenceValue = cancelButton.GetComponent<Button>();
        serializedTester.FindProperty("_statusText").objectReferenceValue = statusText.GetComponent<TextMeshProUGUI>();
        serializedTester.FindProperty("_outputText").objectReferenceValue = outputText.GetComponent<TextMeshProUGUI>();
        serializedTester.FindProperty("_outputScrollRect").objectReferenceValue = scrollView.GetComponent<ScrollRect>();
        serializedTester.FindProperty("_keywordIndicator").objectReferenceValue = keywordPanel.gameObject;
        serializedTester.FindProperty("_lastKeywordText").objectReferenceValue = keywordText.GetComponent<TextMeshProUGUI>();

        // Set test keywords
        string[] keywords = _testKeywords.Split(',');
        serializedTester.FindProperty("_testKeywords").arraySize = keywords.Length;
        for (int i = 0; i < keywords.Length; i++)
        {
            serializedTester.FindProperty("_testKeywords").GetArrayElementAtIndex(i).stringValue = keywords[i].Trim();
        }

        serializedTester.ApplyModifiedProperties();
    }

    /// <summary>
    /// Create a TextMeshPro text object
    /// </summary>
    /// <param name="name">GameObject name</param>
    /// <param name="parent">Parent transform</param>
    /// <param name="text">Initial text</param>
    /// <param name="fontSize">Font size</param>
    /// <returns>Text GameObject</returns>
    private GameObject CreateTextObject(string name, Transform parent, string text, int fontSize)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);

        return textObject;
    }

    /// <summary>
    /// Create a TMP InputField
    /// </summary>
    /// <param name="parent">Parent transform</param>
    /// <param name="name">GameObject name</param>
    /// <param name="placeholder">Placeholder text</param>
    /// <returns>InputField GameObject</returns>
    private GameObject CreateInputField(Transform parent, string name, string placeholder)
    {
        GameObject inputObject = new GameObject(name);
        inputObject.transform.SetParent(parent, false);

        // Add image component
        Image image = inputObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Create text area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputObject.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = new Vector2(0, 0);
        textAreaRect.anchorMax = new Vector2(1, 1);
        textAreaRect.offsetMin = new Vector2(10, 6);
        textAreaRect.offsetMax = new Vector2(-10, -6);

        // Create text component
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.color = Color.white;
        textComponent.fontSize = 18;
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Create placeholder
        GameObject placeholderObject = new GameObject("Placeholder");
        placeholderObject.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI placeholderComponent = placeholderObject.AddComponent<TextMeshProUGUI>();
        placeholderComponent.text = placeholder;
        placeholderComponent.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        placeholderComponent.fontSize = 18;
        placeholderComponent.enabled = true;
        RectTransform placeholderRect = placeholderObject.GetComponent<RectTransform>();
        placeholderRect.anchorMin = new Vector2(0, 0);
        placeholderRect.anchorMax = new Vector2(1, 1);
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;

        // Set up input field
        TMP_InputField inputField = inputObject.AddComponent<TMP_InputField>();
        inputField.textComponent = textComponent;
        inputField.placeholder = placeholderComponent;
        inputField.textViewport = textAreaRect;
        inputField.transition = Selectable.Transition.ColorTint;

        return inputObject;
    }

    /// <summary>
    /// Create a Button with TextMeshPro text
    /// </summary>
    /// <param name="parent">Parent transform</param>
    /// <param name="name">GameObject name</param>
    /// <param name="buttonText">Button text</param>
    /// <returns>Button GameObject</returns>
    private GameObject CreateButton(Transform parent, string name, string buttonText)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        // Add image component
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        // Create text component
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform, false);
        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = buttonText;
        textComponent.color = Color.white;
        textComponent.fontSize = 18;
        textComponent.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Set up button
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        // Set button colors
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        colors.selectedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        button.colors = colors;

        return buttonObject;
    }

    /// <summary>
    /// Create a ScrollView with viewport and content
    /// </summary>
    /// <param name="parent">Parent transform</param>
    /// <param name="name">GameObject name</param>
    /// <returns>ScrollView GameObject</returns>
    private GameObject CreateScrollView(Transform parent, string name)
    {
        GameObject scrollObject = new GameObject(name);
        scrollObject.transform.SetParent(parent, false);

        // Add required components
        RectTransform scrollRect = scrollObject.AddComponent<RectTransform>();
        Image scrollImage = scrollObject.AddComponent<Image>();
        scrollImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Create viewport
        GameObject viewportObject = new GameObject("Viewport");
        viewportObject.transform.SetParent(scrollObject.transform, false);
        RectTransform viewportRect = viewportObject.AddComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0, 0);
        viewportRect.anchorMax = new Vector2(1, 1);
        viewportRect.offsetMin = new Vector2(5, 5);
        viewportRect.offsetMax = new Vector2(-5, -5);
        Image viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        Mask viewportMask = viewportObject.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        // Create content
        GameObject contentObject = new GameObject("Content");
        contentObject.transform.SetParent(viewportObject.transform, false);
        RectTransform contentRect = contentObject.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.sizeDelta = new Vector2(0, 300);
        contentRect.pivot = new Vector2(0.5f, 1f);

        // Set up scroll view
        ScrollRect scrollComponent = scrollObject.AddComponent<ScrollRect>();
        scrollComponent.content = contentRect;
        scrollComponent.viewport = viewportRect;
        scrollComponent.horizontal = false;
        scrollComponent.vertical = true;
        scrollComponent.scrollSensitivity = 10;
        scrollComponent.movementType = ScrollRect.MovementType.Clamped;

        return scrollObject;
    }
}
#endif