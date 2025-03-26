using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Custom descriptor for MultiKeywordTriggerNode to enhance its appearance and behavior
/// in the Visual Scripting graph editor
/// </summary>
[Descriptor(typeof(MultiKeywordTriggerNode))]
public class MultiKeywordTriggerNodeDescriptor : UnitDescriptor<MultiKeywordTriggerNode>
{
    public MultiKeywordTriggerNodeDescriptor(MultiKeywordTriggerNode target) : base(target)
    {
    }

    /// <summary>
    /// Define the node title to display in the graph
    /// </summary>
    /// <returns>Formatted title string</returns>
    protected override string DefinedTitle()
    {
        return "Multi-Keyword Detector";
    }

    /// <summary>
    /// Define the node subtitle (optional text under title)
    /// </summary>
    /// <returns>Subtitle text</returns>
    protected override string DefinedSubtitle()
    {
        return "Advanced AI Response Parser";
    }

    /// <summary>
    /// Define the node summary displayed in tooltips
    /// </summary>
    /// <returns>Node summary text</returns>
    protected override string DefinedSummary()
    {
        return "Detects multiple keywords in AI response text and provides detailed matching information. " +
               "This node is more flexible than the basic Trigger Event node, supporting comma-separated " +
               "keywords and 'match all' mode. Use this node for complex game scenarios where you need " +
               "to analyze AI responses for multiple potential triggers or data points.";
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