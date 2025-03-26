using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Custom descriptor for QueryAINode to enhance its appearance and behavior
/// in the Visual Scripting graph editor
/// </summary>
[Descriptor(typeof(QueryAINode))]
public class QueryAINodeDescriptor : UnitDescriptor<QueryAINode>
{
    public QueryAINodeDescriptor(QueryAINode target) : base(target)
    {
    }

    /// <summary>
    /// Define the node title to display in the graph
    /// </summary>
    /// <returns>Formatted title string</returns>
    protected override string DefinedTitle()
    {
        return "Query AI Agent";
    }

    /// <summary>
    /// Define the node subtitle (optional text under title)
    /// </summary>
    /// <returns>Subtitle text</returns>
    protected override string DefinedSubtitle()
    {
        return "AI Integration";
    }

    /// <summary>
    /// Define the node summary displayed in tooltips
    /// </summary>
    /// <returns>Node summary text</returns>
    protected override string DefinedSummary()
    {
        return "Sends a query to an AI Agent and returns the response. " +
               "This node connects to an AIAgent component to process AI queries. " +
               "It sends the input text to the agent and waits for a response. " +
               "The node provides status outputs to track processing state and errors. " +
               "Connect the Agent port to a Game Object with an AIAgent component.";
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