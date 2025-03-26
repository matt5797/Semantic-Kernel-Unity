#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for SKUnityCoreConfig to provide a user-friendly 
/// interface for setting API keys in the Unity Editor
/// </summary>
[CustomEditor(typeof(SKUnityCoreConfig))]
public class SKUnityCoreConfigEditor : Editor
{
    private bool _showOpenAI = true;
    private bool _showAzure = true;
    private bool _showAnthropic = true;

    private string _openAIKeyInput = "";
    private string _azureOpenAIKeyInput = "";
    private string _anthropicKeyInput = "";

    private SKUnityCoreConfig _config;

    private void OnEnable()
    {
        _config = (SKUnityCoreConfig)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawHeader();

        // OpenAI section
        DrawProviderSection(
            "OpenAI Settings",
            ref _showOpenAI,
            ref _openAIKeyInput,
            _config.OpenAIKey,
            (key) => _config.OpenAIKey = key
        );

        // Azure OpenAI section
        DrawAzureSection();

        // Anthropic section
        DrawProviderSection(
            "Anthropic Settings",
            ref _showAnthropic,
            ref _anthropicKeyInput,
            _config.AnthropicKey,
            (key) => _config.AnthropicKey = key
        );

        EditorGUILayout.Space();
        DrawFooter();

        serializedObject.ApplyModifiedProperties();
    }

    // Draw Azure section with endpoint field
    private void DrawAzureSection()
    {
        _showAzure = EditorGUILayout.Foldout(_showAzure, "Azure OpenAI Settings", true);
        if (_showAzure)
        {
            EditorGUI.indentLevel++;

            // API Key
            DrawKeyFields(
                ref _azureOpenAIKeyInput,
                _config.AzureOpenAIKey,
                (key) => _config.AzureOpenAIKey = key
            );

            // Endpoint URL
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Endpoint URL", EditorStyles.boldLabel);
            string endpoint = EditorGUILayout.TextField(_config.AzureOpenAIEndpoint);
            if (endpoint != _config.AzureOpenAIEndpoint)
            {
                Undo.RecordObject(_config, "Change Azure Endpoint");
                _config.AzureOpenAIEndpoint = endpoint;
                EditorUtility.SetDirty(_config);
            }

            EditorGUI.indentLevel--;
        }
    }

    // Draw header with description
    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("API Keys Configuration", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "API keys are stored with simple encryption in the asset. " +
            "Never share the config asset with API keys.",
            MessageType.Info
        );
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    // Draw footer with additional info
    private void DrawFooter()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.HelpBox(
            "To use in runtime: assign this config asset to SKUnityCore component",
            MessageType.Info
        );
        EditorGUILayout.EndVertical();
    }

    // Draw a provider section with foldout
    private void DrawProviderSection(
        string title,
        ref bool foldout,
        ref string input,
        string currentKey,
        System.Action<string> setKey)
    {
        foldout = EditorGUILayout.Foldout(foldout, title, true);
        if (foldout)
        {
            EditorGUI.indentLevel++;
            DrawKeyFields(ref input, currentKey, setKey);
            EditorGUI.indentLevel--;
        }
    }

    // Draw key input fields, status and buttons
    private void DrawKeyFields(
        ref string input,
        string currentKey,
        System.Action<string> setKey)
    {
        bool hasKey = !string.IsNullOrEmpty(currentKey);

        // Status
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Status:", GUILayout.Width(80));
        if (hasKey)
        {
            EditorGUILayout.LabelField("✓ Key Set", EditorStyles.boldLabel);
        }
        else
        {
            EditorGUILayout.LabelField("✗ No Key", EditorStyles.boldLabel);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Key input field
        EditorGUILayout.LabelField("API Key");
        input = EditorGUILayout.PasswordField(input);

        // Buttons
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Save Key"))
        {
            if (!string.IsNullOrEmpty(input))
            {
                Undo.RecordObject(_config, "Set API Key");
                setKey(input);
                input = "";
                EditorUtility.SetDirty(_config);
            }
        }

        GUI.enabled = hasKey;
        if (GUILayout.Button("Clear Key"))
        {
            Undo.RecordObject(_config, "Clear API Key");
            setKey("");
            EditorUtility.SetDirty(_config);
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }
}
#endif