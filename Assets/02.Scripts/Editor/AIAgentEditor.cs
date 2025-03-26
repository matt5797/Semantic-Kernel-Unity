using UnityEngine;
using UnityEditor;
using System.Linq;

#if UNITY_EDITOR
/// <summary>
/// Custom inspector for AIAgent component with simplified approach
/// Combines default inspector with custom testing tools
/// </summary>
[CustomEditor(typeof(AIAgent))]
public class AIAgentEditor : Editor
{
    // UI state variables
    private bool _showTestingTools = true;
    private string _testInput = "Hello, how are you today?";
    private Vector2 _responseScrollPosition;

    /// <summary>
    /// Draw and handle the custom inspector GUI
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Get reference to the target component
        AIAgent agent = (AIAgent)target;

        // Draw header
        EditorGUILayout.Space();
        DrawHeaderSection(agent);

        // Use default inspector to show all serialized properties
        // This avoids issues with manually finding properties
        EditorGUILayout.Space();
        DrawDefaultInspector();

        // Add custom testing tools section
        EditorGUILayout.Space(10);
        DrawTestingTools(agent);

        // Check for changes in play mode
        if (EditorApplication.isPlaying && GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    /// <summary>
    /// Draw header with status information
    /// </summary>
    /// <param name="agent">The AIAgent being inspected</param>
    private void DrawHeaderSection(AIAgent agent)
    {
        EditorGUILayout.BeginHorizontal();

        // Display title
        GUILayout.Label("AI Agent Configuration", EditorStyles.boldLabel);

        // Display agent status if in play mode
        if (EditorApplication.isPlaying)
        {
            GUIStyle statusStyle = new GUIStyle(EditorStyles.label);

            if (agent.IsProcessing)
            {
                statusStyle.normal.textColor = Color.yellow;
                GUILayout.Label("Status: Processing...", statusStyle, GUILayout.Width(150));
            }
            else
            {
                statusStyle.normal.textColor = Color.green;
                GUILayout.Label("Status: Ready", statusStyle, GUILayout.Width(150));
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draw testing tools section
    /// </summary>
    /// <param name="agent">The AIAgent being inspected</param>
    private void DrawTestingTools(AIAgent agent)
    {
        // Only show testing section in play mode
        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter play mode to test the AI Agent", MessageType.Info);
            return;
        }

        _showTestingTools = EditorGUILayout.Foldout(_showTestingTools, "Testing Tools", true, EditorStyles.foldoutHeader);

        if (_showTestingTools)
        {
            EditorGUI.indentLevel++;

            // Text field for test input
            EditorGUILayout.LabelField("Test Input", EditorStyles.boldLabel);
            _testInput = EditorGUILayout.TextField(_testInput, GUILayout.Height(40));

            // Buttons row
            EditorGUILayout.BeginHorizontal();

            // Send button
            bool canSend = !agent.IsProcessing;
            GUI.enabled = canSend;

            if (GUILayout.Button("Send Test Message", GUILayout.Height(30)))
            {
                if (!string.IsNullOrEmpty(_testInput))
                {
                    agent.ProcessInput(_testInput);
                }
            }

            // Cancel button
            GUI.enabled = agent.IsProcessing;
            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            {
                agent.CancelRequest();
            }

            // Clear history button
            GUI.enabled = true;
            if (GUILayout.Button("Clear History", GUILayout.Height(30)))
            {
                agent.ClearChatHistory();
            }

            EditorGUILayout.EndHorizontal();

            // Display chat history if any exists
            var chatHistory = agent.GetChatHistory();
            if (chatHistory != null && chatHistory.Any())
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Chat History", EditorStyles.boldLabel);

                _responseScrollPosition = EditorGUILayout.BeginScrollView(
                    _responseScrollPosition,
                    GUILayout.Height(150)
                );

                // Show chat history messages
                foreach (var message in chatHistory)
                {
                    EditorGUILayout.BeginHorizontal();

                    // Role label with role-specific color
                    GUIStyle roleStyle = new GUIStyle(EditorStyles.boldLabel);
                    roleStyle.normal.textColor = message.Role == ChatMemory.MessageRole.User ?
                        new Color(0.2f, 0.4f, 0.7f) : new Color(0.1f, 0.6f, 0.3f);

                    EditorGUILayout.LabelField(message.Role.ToString() + ":",
                        roleStyle, GUILayout.Width(70));

                    // Message content with word wrap
                    GUIStyle contentStyle = new GUIStyle(EditorStyles.textArea);
                    contentStyle.wordWrap = true;
                    EditorGUILayout.TextArea(message.Content, contentStyle);

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(5);
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUI.indentLevel--;
        }
    }

    /// <summary>
    /// Repaint the inspector when something changes
    /// </summary>
    public void OnInspectorUpdate()
    {
        if (EditorApplication.isPlaying)
        {
            Repaint();
        }
    }
}
#endif