using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Custom descriptor for TriggerEventNodeVS to enhance its appearance and behavior
/// in the Visual Scripting graph editor
/// </summary>
[Descriptor(typeof(TriggerEventNode))]
public class TriggerEventNodeDescriptor : UnitDescriptor<TriggerEventNode>
{
    public TriggerEventNodeDescriptor(TriggerEventNode target) : base(target)
    {
    }

    /// <summary>
    /// Define the node title to display in the graph
    /// </summary>
    /// <returns>Formatted title string</returns>
    protected override string DefinedTitle()
    {
        return "Trigger Event on Keyword";
    }

    /// <summary>
    /// Define the node subtitle (optional text under title)
    /// </summary>
    /// <returns>Subtitle text</returns>
    protected override string DefinedSubtitle()
    {
        return "AI Response Handler";
    }

    /// <summary>
    /// Define the node summary displayed in tooltips
    /// </summary>
    /// <returns>Node summary text</returns>
    protected override string DefinedSummary()
    {
        return "Detects a specific keyword in AI response text and triggers events when found. " +
               "Connect this node after receiving an AI response to detect keywords like 'help', " +
               "'attack', or any game-specific term. The node has two output paths - one that " +
               "always triggers, and another that only triggers when the keyword is detected.";
    }

    /// <summary>
    /// Define custom icon for the node
    /// </summary>
    /// <returns>Icon for the node</returns>
    protected override EditorTexture DefinedIcon()
    {
        // Use base implementation but we could customize this if needed
        return base.DefinedIcon();
    }
}