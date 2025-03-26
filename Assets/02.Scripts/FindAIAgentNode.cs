using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Visual Scripting node to find an AIAgent in the scene
/// Helps connections to AIAgent when direct references aren't available
/// </summary>
[UnitTitle("Find AI Agent")]
[UnitCategory("SK Unity/AI Agents")]
public class FindAIAgentNode : Unit
{
    // Input ports
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput inputTrigger; // Execution flow input

    [DoNotSerialize]
    public ValueInput agentName; // Optional agent GameObject name

    [DoNotSerialize]
    public ValueInput searchInChildren; // Whether to search in children of specified GameObject

    [DoNotSerialize]
    public ValueInput gameObject; // Optional GameObject to search from

    // Output ports
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlOutput outputTrigger; // Execution flow continues

    [DoNotSerialize]
    public ValueOutput foundAgent; // Found AIAgent reference

    [DoNotSerialize]
    public ValueOutput wasFound; // Whether an agent was found

    // Cached data
    private AIAgent _lastFoundAgent = null;
    private bool _lastFoundResult = false;

    /// <summary>
    /// Define node ports and connections
    /// </summary>
    protected override void Definition()
    {
        // Define input control flow port
        inputTrigger = ControlInput("inputTrigger", FindAgent);

        // Define input value ports
        agentName = ValueInput<string>("Agent Name", "");
        searchInChildren = ValueInput<bool>("Search In Children", true);
        gameObject = ValueInput<GameObject>("Game Object", null);

        // Define output control flow port
        outputTrigger = ControlOutput("outputTrigger");

        // Define output value ports
        foundAgent = ValueOutput<AIAgent>("Agent", flow => _lastFoundAgent);
        wasFound = ValueOutput<bool>("Found", flow => _lastFoundResult);

        // Define succession
        Succession(inputTrigger, outputTrigger);
    }

    /// <summary>
    /// Execute the AI agent search when the node is triggered
    /// </summary>
    /// <param name="flow">Execution flow data</param>
    /// <returns>Output control port to continue execution</returns>
    private ControlOutput FindAgent(Flow flow)
    {
        // Reset previous result
        _lastFoundAgent = null;
        _lastFoundResult = false;

        // Get input values
        string nameToFind = flow.GetValue<string>(agentName);
        bool searchChildren = flow.GetValue<bool>(searchInChildren);
        GameObject targetObject = flow.GetValue<GameObject>(gameObject);

        // Try to find the agent
        if (targetObject != null)
        {
            // Search in specified GameObject
            if (searchChildren)
            {
                _lastFoundAgent = targetObject.GetComponentInChildren<AIAgent>();
            }
            else
            {
                _lastFoundAgent = targetObject.GetComponent<AIAgent>();
            }
        }
        else if (!string.IsNullOrEmpty(nameToFind))
        {
            // Search by name
            GameObject namedObject = GameObject.Find(nameToFind);
            if (namedObject != null)
            {
                _lastFoundAgent = namedObject.GetComponent<AIAgent>();
            }
        }
        else
        {
            // Find any AIAgent in scene
            _lastFoundAgent = Object.FindObjectOfType<AIAgent>();
        }

        // Update found status
        _lastFoundResult = (_lastFoundAgent != null);

        // Continue execution flow
        return outputTrigger;
    }
}