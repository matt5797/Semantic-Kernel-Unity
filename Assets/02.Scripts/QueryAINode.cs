using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Visual Scripting node for querying AI agents
/// Sends input text to an AIAgent and returns the response
/// </summary>
[UnitTitle("Query AI Agent")]
[UnitCategory("SK Unity/AI Agents")]
public class QueryAINode : Unit
{
    // Input ports
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput inputTrigger; // Execution flow input

    [DoNotSerialize]
    public ValueInput agentReference; // AIAgent reference

    [DoNotSerialize]
    public ValueInput queryText; // Input text to send to AI

    [DoNotSerialize]
    public ValueInput timeout; // Optional timeout in seconds

    // Output ports
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlOutput outputTrigger; // Execution flow continues here

    [DoNotSerialize]
    public ValueOutput response; // AI response text

    [DoNotSerialize]
    public ValueOutput isProcessing; // Whether AI is still processing

    [DoNotSerialize]
    public ValueOutput hasError; // Whether an error occurred

    [DoNotSerialize]
    public ValueOutput errorMessage; // Error message if any

    // Node state
    private bool _isProcessing = false;
    private bool _hasError = false;
    private string _response = "";
    private string _errorMessage = "";
    private AIAgent _currentAgent = null;

    /// <summary>
    /// Define node ports and connections
    /// </summary>
    protected override void Definition()
    {
        // Define input control flow port
        inputTrigger = ControlInput("inputTrigger", ExecuteQuery);

        // Define input value ports
        agentReference = ValueInput<AIAgent>("Agent", null);
        queryText = ValueInput<string>("Query", "");
        timeout = ValueInput<float>("Timeout", 15f);

        // Define output control flow port
        outputTrigger = ControlOutput("outputTrigger");

        // Define output value ports
        response = ValueOutput<string>("Response", flow => _response);
        isProcessing = ValueOutput<bool>("Is Processing", flow => _isProcessing);
        hasError = ValueOutput<bool>("Has Error", flow => _hasError);
        errorMessage = ValueOutput<string>("Error", flow => _errorMessage);

        // Define requirements
        Requirement(agentReference, inputTrigger);
        Requirement(queryText, inputTrigger);
        Succession(inputTrigger, outputTrigger);
    }

    /// <summary>
    /// Execute the query when the node is triggered
    /// </summary>
    /// <param name="flow">Execution flow data</param>
    /// <returns>Output control port to continue execution</returns>
    private ControlOutput ExecuteQuery(Flow flow)
    {
        // Reset previous state
        _response = "";
        _hasError = false;
        _errorMessage = "";

        // Get input values
        AIAgent agent = flow.GetValue<AIAgent>(agentReference);
        string query = flow.GetValue<string>(queryText);
        float timeoutValue = flow.GetValue<float>(timeout);

        // Validate inputs
        if (agent == null)
        {
            _hasError = true;
            _errorMessage = "No AIAgent assigned";
            return outputTrigger;
        }

        if (string.IsNullOrEmpty(query))
        {
            _hasError = true;
            _errorMessage = "Query text is empty";
            return outputTrigger;
        }

        // Store agent reference to unsubscribe later
        _currentAgent = agent;

        // Mark as processing
        _isProcessing = true;

        try
        {
            // Subscribe to agent events
            agent.OnResponseReceived += HandleResponse;
            agent.OnError += HandleError;
            agent.OnProcessingComplete += HandleComplete;

            // Send query to agent
            agent.ProcessInput(query);
        }
        catch (Exception ex)
        {
            HandleError($"Exception: {ex.Message}");
        }

        // Continue execution flow
        return outputTrigger;
    }

    /// <summary>
    /// Handle AI response
    /// </summary>
    /// <param name="responseText">Text response from AI</param>
    private void HandleResponse(string responseText)
    {
        _response = responseText;
    }

    /// <summary>
    /// Handle processing error
    /// </summary>
    /// <param name="error">Error message</param>
    private void HandleError(string error)
    {
        _hasError = true;
        _errorMessage = error;
        _isProcessing = false;
        CleanupEventSubscriptions();
    }

    /// <summary>
    /// Handle processing completion
    /// </summary>
    /// <param name="finalResponse">Final response text</param>
    private void HandleComplete(string finalResponse)
    {
        _isProcessing = false;
        CleanupEventSubscriptions();
    }

    /// <summary>
    /// Unsubscribe from agent events
    /// </summary>
    private void CleanupEventSubscriptions()
    {
        if (_currentAgent != null)
        {
            _currentAgent.OnResponseReceived -= HandleResponse;
            _currentAgent.OnError -= HandleError;
            _currentAgent.OnProcessingComplete -= HandleComplete;
            _currentAgent = null;
        }
    }
}