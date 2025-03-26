using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Visual Scripting node that detects keywords in AI responses and triggers events
/// Enables game events based on AI responses without coding
/// </summary>
[UnitTitle("Trigger Event on Keyword")]
[UnitCategory("SK Unity/Events")]
public class TriggerEventNode : Unit
{
    // Input ports
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput inputTrigger; // Execution flow input

    [DoNotSerialize]
    public ValueInput responseText; // AI response text to analyze

    [DoNotSerialize]
    public ValueInput keywordToDetect; // Keyword to search for

    [DoNotSerialize]
    public ValueInput caseSensitive; // Whether to use case-sensitive matching

    [DoNotSerialize]
    public ValueInput customEventParameter; // Optional custom parameter to pass with event

    // Output ports
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlOutput outputTrigger; // Execution flow output - always triggered

    [DoNotSerialize]
    public ControlOutput keywordDetectedTrigger; // Execution flow when keyword is found

    [DoNotSerialize]
    public ValueOutput keywordDetected; // Boolean indicating if keyword was found

    [DoNotSerialize]
    public ValueOutput detectedKeyword; // The keyword that was detected

    // Node state
    private bool _wasDetected = false;
    private string _lastKeyword = "";

    /// <summary>
    /// Define node ports and connections
    /// </summary>
    protected override void Definition()
    {
        // Define input control flow
        inputTrigger = ControlInput("inputTrigger", CheckForKeyword);

        // Define input values
        responseText = ValueInput<string>("Response Text", "");
        keywordToDetect = ValueInput<string>("Keyword", "");
        caseSensitive = ValueInput<bool>("Case Sensitive", false);
        customEventParameter = ValueInput<string>("Event Parameter", "");

        // Define output control flow
        outputTrigger = ControlOutput("outputTrigger");
        keywordDetectedTrigger = ControlOutput("keywordDetected");

        // Define output values
        keywordDetected = ValueOutput<bool>("Was Detected", flow => _wasDetected);
        detectedKeyword = ValueOutput<string>("Detected Keyword", flow => _lastKeyword);

        // Define port relationships
        Requirement(responseText, inputTrigger);
        Requirement(keywordToDetect, inputTrigger);
        Succession(inputTrigger, outputTrigger);
    }

    /// <summary>
    /// Execute the keyword check when the node is triggered
    /// </summary>
    /// <param name="flow">Execution flow data</param>
    /// <returns>Output control port to continue execution</returns>
    private ControlOutput CheckForKeyword(Flow flow)
    {
        // Reset state
        _wasDetected = false;
        _lastKeyword = "";

        // Get input values
        string response = flow.GetValue<string>(responseText);
        string keyword = flow.GetValue<string>(keywordToDetect);
        bool isCaseSensitive = flow.GetValue<bool>(caseSensitive);

        // Skip check if inputs are invalid
        if (string.IsNullOrEmpty(response) || string.IsNullOrEmpty(keyword))
        {
            return outputTrigger;
        }

        // Check for keyword based on case sensitivity
        bool detected = isCaseSensitive
            ? response.Contains(keyword)
            : response.ToLower().Contains(keyword.ToLower());

        // Update state if detected
        if (detected)
        {
            _wasDetected = true;
            _lastKeyword = keyword;

            // For debugging in Unity Editor
            if (Application.isEditor)
            {
                Debug.Log($"TriggerEventNodeVS: Detected keyword '{keyword}' in response");
            }

            // Return the keyword detected output
            return keywordDetectedTrigger;
        }

        // Continue normal execution flow if no keyword detected
        return outputTrigger;
    }
}