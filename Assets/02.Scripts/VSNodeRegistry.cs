using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Central registry for all Visual Scripting nodes in the AI system.
/// Acts as a single entry point for registering node types and categories.
/// </summary>
[InitializeOnLoad] // Automatically initialize in editor
public static class VSNodeRegistry
{
    // Define category paths
    private const string BaseCategory = "SK Unity/";
    public const string AICategory = BaseCategory + "AI Agents/";
    public const string DialogueCategory = BaseCategory + "Dialogue/";
    public const string EventsCategory = BaseCategory + "Events/";

    // Track registered node types
    private static readonly List<Type> _registeredNodeTypes = new List<Type>();

    // Static constructor called by Unity when editor loads
    static VSNodeRegistry()
    {
        // Register nodes in editor
#if UNITY_EDITOR
        RegisterNodesWithVS();
#endif
    }

    /// <summary>
    /// Register all SK Unity custom nodes with Visual Scripting
    /// </summary>
    public static void RegisterNodes()
    {
        // Clear previously registered nodes to avoid duplicates
        _registeredNodeTypes.Clear();

        try
        {
            // Register node types by adding them to the list
            // The Visual Scripting system will discover these through attributes
            RegisterNodeType<QueryAINode>();
            RegisterNodeType<TriggerEventNode>();
            RegisterNodeType<MultiKeywordTriggerNode>();
            RegisterNodeType<FindAIAgentNode>();

            Debug.Log("VSNodeRegistry: Successfully registered all nodes");

#if UNITY_EDITOR
            // In Unity Editor, force a refresh of the node database
            Unity.VisualScripting.VSUsageUtility.isVisualScriptingUsed = true;
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"VSNodeRegistry: Failed to register nodes - {ex.Message}");
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// Register nodes with Visual Scripting system
    /// </summary>
    private static void RegisterNodesWithVS()
    {
        // Add our node provider to the static extensions list
        UnitBase.staticUnitsExtensions.Add(GetNodeOptions);

        Debug.Log("VSNodeRegistry: Added node provider to Visual Scripting");
    }

    /// <summary>
    /// Provides node options to Visual Scripting system
    /// </summary>
    /// <returns>Collection of unit options</returns>
    private static IEnumerable<IUnitOption> GetNodeOptions()
    {
        // Ensure our nodes are registered
        if (_registeredNodeTypes.Count == 0)
        {
            RegisterNodes();
        }

        // Create and return unit options
        // The actual list is empty because the nodes will be discovered by the VS system
        // through their attributes, but we need this hook for the system to recognize our nodes
        return new List<IUnitOption>();
    }

    /// <summary>
    /// Register a single node type
    /// </summary>
    /// <typeparam name="T">Node type to register (must derive from Unit)</typeparam>
    private static void RegisterNodeType<T>() where T : Unit
    {
        // Skip if already registered
        if (_registeredNodeTypes.Contains(typeof(T)))
            return;

        try
        {
            // Add to tracking list
            _registeredNodeTypes.Add(typeof(T));

            Debug.Log($"VSNodeRegistry: Registered node {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"VSNodeRegistry: Failed to register {typeof(T).Name} - {ex.Message}");
        }
    }

    /// <summary>
    /// Check if a node type is registered
    /// </summary>
    /// <typeparam name="T">Node type to check</typeparam>
    /// <returns>True if registered</returns>
    public static bool IsNodeRegistered<T>() where T : Unit
    {
        return _registeredNodeTypes.Contains(typeof(T));
    }

    /// <summary>
    /// Get a list of all registered node types
    /// </summary>
    /// <returns>Read-only list of registered node types</returns>
    public static IReadOnlyList<Type> GetRegisteredNodeTypes()
    {
        return _registeredNodeTypes.AsReadOnly();
    }
}