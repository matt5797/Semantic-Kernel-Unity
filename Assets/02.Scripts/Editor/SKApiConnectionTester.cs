#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor tool for testing API connections with different LLM providers
/// </summary>
public class SKApiConnectionTester : EditorWindow
{
    private SKUnityCoreConfig _config;
    private SKUnityCore _testCore;
    private GameObject _testObject;

    // Test results tracking
    private Dictionary<string, TestResult> _testResults = new Dictionary<string, TestResult>();
    private bool _isTestingAny = false;

    // Test status info
    private class TestResult
    {
        public bool? Success { get; set; } = null;
        public bool IsRunning { get; set; } = false;
        public string Message { get; set; } = "Not tested yet";
        public float StartTime { get; set; } = 0f;
    }

    // Supported providers for testing
    private readonly string[] _providerNames = {
        "OpenAI",
        "Azure OpenAI",
        "Anthropic"
    };

    // Request IDs for cancellation
    private Dictionary<string, string> _requestIds = new Dictionary<string, string>();

    [MenuItem("SK Unity/API Connection Tester")]
    public static void ShowWindow()
    {
        GetWindow<SKApiConnectionTester>("API Connection Tester");
    }

    private void OnEnable()
    {
        // Initialize test results dictionary
        foreach (string provider in _providerNames)
        {
            _testResults[provider] = new TestResult();
        }
    }

    private void OnDisable()
    {
        CleanupTestObject();
    }

    /// <summary>
    /// Ensure test object is created and configured
    /// </summary>
    private void EnsureTestObjectExists()
    {
        if (_testObject == null)
        {
            _testObject = new GameObject("SKApiTestObject");
            _testObject.hideFlags = HideFlags.HideAndDontSave;
            _testCore = _testObject.AddComponent<SKUnityCore>();
        }
    }

    /// <summary>
    /// Clean up test object
    /// </summary>
    private void CleanupTestObject()
    {
        if (_testObject != null)
        {
            DestroyImmediate(_testObject);
            _testObject = null;
            _testCore = null;
        }
    }

    private void OnGUI()
    {
        // Config selection
        EditorGUILayout.LabelField("API Connection Tester", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Select config asset
        EditorGUI.BeginChangeCheck();
        _config = (SKUnityCoreConfig)EditorGUILayout.ObjectField(
            "SK Config Asset", _config, typeof(SKUnityCoreConfig), false);

        if (EditorGUI.EndChangeCheck() && _config != null)
        {
            // Reset test results when config changes
            foreach (string provider in _providerNames)
            {
                _testResults[provider] = new TestResult();
            }
        }

        EditorGUILayout.Space();

        // Show message if no config selected
        if (_config == null)
        {
            EditorGUILayout.HelpBox("Please select a SKUnityCoreConfig asset to test API connections.",
                MessageType.Info);
            return;
        }

        // Test All button
        EditorGUI.BeginDisabledGroup(_isTestingAny);
        if (GUILayout.Button("Test All Connections"))
        {
            TestAllConnections();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        // Individual test sections
        DrawProviderTestSection("OpenAI", SKUnityCore.LLMProvider.OpenAI);
        EditorGUILayout.Space();
        DrawProviderTestSection("Azure OpenAI", SKUnityCore.LLMProvider.AzureOpenAI);
        EditorGUILayout.Space();
        DrawProviderTestSection("Anthropic", SKUnityCore.LLMProvider.Anthropic);
    }

    /// <summary>
    /// Draw UI section for testing a specific provider
    /// </summary>
    /// <param name="providerName">Display name of the provider</param>
    /// <param name="provider">LLM provider enum value</param>
    private void DrawProviderTestSection(string providerName, SKUnityCore.LLMProvider provider)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Provider header
        EditorGUILayout.LabelField(providerName, EditorStyles.boldLabel);

        // Display test status
        TestResult result = _testResults[providerName];

        // Result info
        if (result.IsRunning)
        {
            float runningTime = (float)(EditorApplication.timeSinceStartup - result.StartTime);
            EditorGUILayout.LabelField($"Status: Testing... ({runningTime:F1}s)");

            // Auto-repaint to update time
            Repaint();
        }
        else if (result.Success.HasValue)
        {
            EditorGUILayout.LabelField("Status:", EditorStyles.boldLabel);
            if (result.Success.Value)
            {
                EditorGUILayout.LabelField("✓ Connection Successful", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField("✗ Connection Failed", EditorStyles.boldLabel);
            }

            EditorGUILayout.LabelField("Message:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(result.Message, EditorStyles.wordWrappedLabel);
        }
        else
        {
            EditorGUILayout.LabelField("Status: Not tested yet");
        }

        // Test/Cancel buttons
        EditorGUILayout.BeginHorizontal();

        // Test button
        EditorGUI.BeginDisabledGroup(result.IsRunning || _isTestingAny);
        if (GUILayout.Button("Test Connection"))
        {
            TestConnection(providerName, provider);
        }
        EditorGUI.EndDisabledGroup();

        // Cancel button
        EditorGUI.BeginDisabledGroup(!result.IsRunning);
        if (GUILayout.Button("Cancel Test"))
        {
            CancelTest(providerName);
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Test a specific provider connection
    /// </summary>
    /// <param name="providerName">Provider display name</param>
    /// <param name="provider">LLM provider enum value</param>
    private void TestConnection(string providerName, SKUnityCore.LLMProvider provider)
    {
        if (_config == null) return;

        // Setup test environment
        EnsureTestObjectExists();

        // Mark test as running
        _testResults[providerName].IsRunning = true;
        _testResults[providerName].Success = null;
        _testResults[providerName].Message = "Running test...";
        _testResults[providerName].StartTime = (float)EditorApplication.timeSinceStartup;
        _isTestingAny = true;

        // Set config
        var configField = typeof(SKUnityCore).GetField("_config",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        configField?.SetValue(_testCore, _config);

        // Initialize core with provider
        _testCore.Initialize(provider);

        // Start test
        string requestId = _testCore.TestConnection(
            provider,
            (success) => OnTestCompleted(providerName, success),
            15f // 15 second timeout
        );

        // Store request ID for potential cancellation
        _requestIds[providerName] = requestId;

        // Force repaint to update status
        Repaint();
    }

    /// <summary>
    /// Test all configured API connections
    /// </summary>
    private void TestAllConnections()
    {
        if (_config == null) return;

        TestConnection("OpenAI", SKUnityCore.LLMProvider.OpenAI);
        TestConnection("Azure OpenAI", SKUnityCore.LLMProvider.AzureOpenAI);
        TestConnection("Anthropic", SKUnityCore.LLMProvider.Anthropic);
    }

    /// <summary>
    /// Cancel an ongoing test
    /// </summary>
    /// <param name="providerName">Provider display name</param>
    private void CancelTest(string providerName)
    {
        if (!_testResults[providerName].IsRunning) return;

        if (_testCore != null && _requestIds.TryGetValue(providerName, out string requestId))
        {
            _testCore.CancelRequest(requestId);
        }

        // Update status
        _testResults[providerName].IsRunning = false;
        _testResults[providerName].Message = "Test was cancelled";

        // Check if any tests are still running
        CheckAllTestsCompleted();

        // Force repaint
        Repaint();
    }

    /// <summary>
    /// Handle test completion callback
    /// </summary>
    /// <param name="providerName">Provider display name</param>
    /// <param name="success">Test result</param>
    private void OnTestCompleted(string providerName, bool success)
    {
        // Update test result
        if (_testResults.ContainsKey(providerName))
        {
            _testResults[providerName].IsRunning = false;
            _testResults[providerName].Success = success;
            _testResults[providerName].Message = success ?
                "Connection successful! API responded correctly." :
                "Connection failed. Check API key and network connection.";
        }

        // Check if all tests have completed
        CheckAllTestsCompleted();

        // Force repaint to update UI
        Repaint();
    }

    /// <summary>
    /// Check if all tests have completed and update global testing state
    /// </summary>
    private void CheckAllTestsCompleted()
    {
        bool anyRunning = false;
        foreach (var result in _testResults.Values)
        {
            if (result.IsRunning)
            {
                anyRunning = true;
                break;
            }
        }

        _isTestingAny = anyRunning;
    }

    private void Update()
    {
        // Keep repainting while any tests are running
        if (_isTestingAny)
        {
            Repaint();
        }
    }
}
#endif