using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

#if UNITY_EDITOR
/// <summary>
/// Editor-only component to ensure Visual Scripting nodes are properly registered
/// during editor initialization and when entering play mode.
/// </summary>
[InitializeOnLoad]
public static class VSNodeRegistrySetup
{
    // Static constructor called on editor startup
    static VSNodeRegistrySetup()
    {
        // Register when editor starts
        RegisterNodesOnStartup();

        // Subscribe to editor events
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
    }

    /// <summary>
    /// Register nodes when Unity editor starts
    /// </summary>
    private static void RegisterNodesOnStartup()
    {
        // Ensure nodes are registered when editor starts
        EditorApplication.delayCall += () =>
        {
            VSNodeRegistry.RegisterNodes();
            TriggerVisualScriptingRefresh();
            Debug.Log("VSNodeRegistry: Nodes registered during editor startup");
        };
    }

    /// <summary>
    /// Handle play mode state changes
    /// </summary>
    /// <param name="state">New play mode state</param>
    private static void PlayModeStateChanged(PlayModeStateChange state)
    {
        // Register nodes when entering play mode
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            VSNodeRegistry.RegisterNodes();
            Debug.Log("VSNodeRegistry: Nodes registered when entering play mode");
        }
    }

    /// <summary>
    /// Menu item to manually register nodes and rebuild the VS database
    /// </summary>
    [MenuItem("SK Unity/Register Visual Scripting Nodes")]
    private static void RegisterNodesMenuItem()
    {
        VSNodeRegistry.RegisterNodes();

        // Trigger a Visual Scripting database rebuild
        TriggerVisualScriptingRefresh();

        Debug.Log("VSNodeRegistry: Nodes registered manually");

        // Show success notification
        EditorUtility.DisplayDialog(
            "Nodes Registered",
            "All SK Unity nodes have been registered with Visual Scripting. " +
            "You may need to restart Unity for changes to take effect.",
            "OK");
    }

    /// <summary>
    /// Helper method to trigger a Visual Scripting database refresh
    /// </summary>
    private static void TriggerVisualScriptingRefresh()
    {
        try
        {
            // Mark Visual Scripting as being used in the project
            VSUsageUtility.isVisualScriptingUsed = true;

            // Attempt to force a node database rebuild
            BoltCore.Configuration.Save();

            // Try to update the assembly settings via reflection if possible
            try
            {
                // Attempt to get types via reflection to avoid direct dependency
                var assemblyUtility = System.Type.GetType("Unity.VisualScripting.AssemblyUtility, Unity.VisualScripting.Core");
                if (assemblyUtility != null)
                {
                    var updateMethod = assemblyUtility.GetMethod("UpdateSettings",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (updateMethod != null)
                    {
                        updateMethod.Invoke(null, null);
                        Debug.Log("VSNodeRegistry: Updated assembly settings via reflection");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"VSNodeRegistry: Failed to update assembly settings: {ex.Message}");
            }

            Debug.Log("VSNodeRegistry: Triggered Visual Scripting database refresh");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"VSNodeRegistry: Failed to refresh Visual Scripting database: {ex.Message}");
        }
    }
}
#endif