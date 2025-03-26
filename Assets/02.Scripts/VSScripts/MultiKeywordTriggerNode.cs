using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Visual Scripting node that detects multiple keywords in AI responses
/// Provides more advanced keyword detection for complex game scenarios
/// </summary>
[UnitTitle("Multi-Keyword Detector")]
[UnitCategory("SK Unity/Events")]
public class MultiKeywordTriggerNode : Unit
{
    // Constants
    private const char KEYWORD_SEPARATOR = ',';

    // Input ports
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput inputTrigger; // Execution flow input

    [DoNotSerialize]
    public ValueInput responseText; // AI response text to analyze

    [DoNotSerialize]
    public ValueInput keywords; // Comma-separated list of keywords

    [DoNotSerialize]
    public ValueInput caseSensitive; // Whether to use case-sensitive matching

    [DoNotSerialize]
    public ValueInput matchAll; // Whether all keywords must be found

    // Output ports
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlOutput outputTrigger; // Execution flow output - always triggered

    [DoNotSerialize]
    public ControlOutput matchedTrigger; // Execution flow when keywords are matched

    [DoNotSerialize]
    public ValueOutput keywordsMatched; // Boolean indicating if keywords were matched

    [DoNotSerialize]
    public ValueOutput matchedKeywords; // List of matched keywords

    [DoNotSerialize]
    public ValueOutput matchCount; // Number of matches found

    // Node state
    private bool _matched = false;
    private readonly List<string> _matchedList = new List<string>();
    private int _count = 0;

    /// <summary>
    /// Define node ports and connections
    /// </summary>
    protected override void Definition()
    {
        // Define input control flow
        inputTrigger = ControlInput("inputTrigger", CheckForKeywords);

        // Define input values
        responseText = ValueInput<string>("Response Text", "");
        keywords = ValueInput<string>("Keywords (comma separated)", "");
        caseSensitive = ValueInput<bool>("Case Sensitive", false);
        matchAll = ValueInput<bool>("Match All Keywords", false);

        // Define output control flow
        outputTrigger = ControlOutput("outputTrigger");
        matchedTrigger = ControlOutput("keywordsMatched");

        // Define output values
        keywordsMatched = ValueOutput<bool>("Were Matched", flow => _matched);
        matchedKeywords = ValueOutput<List<string>>("Matched Keywords", flow => _matchedList);
        matchCount = ValueOutput<int>("Match Count", flow => _count);

        // Define port relationships
        Requirement(responseText, inputTrigger);
        Requirement(keywords, inputTrigger);
        Succession(inputTrigger, outputTrigger);
    }

    /// <summary>
    /// Execute the keyword check when the node is triggered
    /// </summary>
    /// <param name="flow">Execution flow data</param>
    /// <returns>Output control port to continue execution</returns>
    private ControlOutput CheckForKeywords(Flow flow)
    {
        // Reset state
        _matched = false;
        _matchedList.Clear();
        _count = 0;

        // Get input values
        string response = flow.GetValue<string>(responseText);
        string keywordList = flow.GetValue<string>(keywords);
        bool isCaseSensitive = flow.GetValue<bool>(caseSensitive);
        bool needsMatchAll = flow.GetValue<bool>(matchAll);

        // Skip check if inputs are invalid
        if (string.IsNullOrEmpty(response) || string.IsNullOrEmpty(keywordList))
        {
            return outputTrigger;
        }

        // Parse keywords
        string[] keywordArray = keywordList.Split(KEYWORD_SEPARATOR);

        // Prepare comparison text
        string compText = isCaseSensitive ? response : response.ToLower();

        // Check each keyword
        foreach (string k in keywordArray)
        {
            string keyword = k.Trim();
            if (string.IsNullOrEmpty(keyword)) continue;

            string compKeyword = isCaseSensitive ? keyword : keyword.ToLower();

            if (compText.Contains(compKeyword))
            {
                _matchedList.Add(keyword);
                _count++;
            }
        }

        // Determine if matched based on mode
        _matched = needsMatchAll
            ? _count == keywordArray.Length
            : _count > 0;

        // Return appropriate output path
        if (_matched)
        {
            // For debugging in Unity Editor
            if (Application.isEditor)
            {
                Debug.Log($"MultiKeywordTriggerNode: Matched {_count} keywords");
            }

            return matchedTrigger;
        }

        return outputTrigger;
    }
}