using UnityEngine;
using Unity.VisualScripting;

/// <summary>
/// Runtime initializer for Visual Scripting nodes.
/// Ensures nodes are properly registered in a built game.
/// </summary>
public class VSNodeInitializer : MonoBehaviour
{
    [SerializeField] private bool _registerOnAwake = true;
    [SerializeField] private bool _logRegistration = true;
    [SerializeField] private float _initWaitTime = 0.5f; // Time to wait before attempting registration
    [SerializeField] private int _maxRetries = 3; // Maximum number of registration attempts

    // Singleton instance
    private static VSNodeInitializer _instance;
    private int _retryCount = 0;

    /// <summary>
    /// Register nodes on component awake if configured
    /// </summary>
    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        // Keep object across scene changes if needed
        DontDestroyOnLoad(gameObject);

        // Register nodes at startup if enabled
        if (_registerOnAwake)
        {
            // Use a small delay to ensure VS system has initialized
            if (Application.isPlaying)
            {
                Invoke("RegisterNodes", _initWaitTime);
            }
            else
            {
                RegisterNodes();
            }
        }
    }

    /// <summary>
    /// Manually register nodes at runtime
    /// </summary>
    public void RegisterNodes()
    {
        try
        {
            VSNodeRegistry.RegisterNodes();

            if (_logRegistration)
            {
                Debug.Log("VSNodeInitializer: Registered Visual Scripting nodes at runtime");
            }

            _retryCount = 0; // Reset retry count on success
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"VSNodeInitializer: Failed to register nodes: {ex.Message}");

            // Try again if we haven't exceeded the retry limit
            if (_retryCount < _maxRetries)
            {
                _retryCount++;

                if (_logRegistration)
                {
                    Debug.Log($"VSNodeInitializer: Retrying registration (attempt {_retryCount} of {_maxRetries})...");
                }

                // Retry with increasing delay
                float retryDelay = _initWaitTime * (1f + _retryCount * 0.5f);
                Invoke("RegisterNodes", retryDelay);
            }
            else
            {
                Debug.LogError($"VSNodeInitializer: Failed to register nodes after {_maxRetries} attempts");
            }
        }
    }

    /// <summary>
    /// Force registration of nodes with multiple retries
    /// </summary>
    public void ForceRegisterNodes()
    {
        _retryCount = 0;
        CancelInvoke("RegisterNodes");
        RegisterNodes();
    }
}