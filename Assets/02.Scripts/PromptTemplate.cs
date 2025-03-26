using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using UnityEngine;

/// <summary>
/// Manages prompt templates with variable substitution for consistent AI responses
/// Implements IPromptTemplate from Semantic Kernel
/// </summary>
public class PromptTemplate : Microsoft.SemanticKernel.IPromptTemplate
{
    // Template string with placeholders like {{variable}}
    private string _templateText;

    // Dictionary to store variable values
    private Dictionary<string, string> _variables = new Dictionary<string, string>();

    // Regex for finding variables in template
    private static readonly Regex _variablePattern = new Regex(@"\{\{([a-zA-Z0-9_]+)\}\}", RegexOptions.Compiled);

    /// <summary>
    /// Creates a new prompt template with the specified template text
    /// </summary>
    /// <param name="templateText">Template text with {{variable}} placeholders</param>
    public PromptTemplate(string templateText)
    {
        _templateText = templateText ?? string.Empty;
    }

    /// <summary>
    /// Sets a variable value for substitution
    /// </summary>
    /// <param name="key">Variable name without {{ }} brackets</param>
    /// <param name="value">Value to substitute</param>
    public void SetVariable(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("PromptTemplate: Variable key cannot be null or empty");
            return;
        }

        _variables[key] = value;
    }

    /// <summary>
    /// Sets multiple variables at once
    /// </summary>
    /// <param name="variables">Dictionary of variable names and values</param>
    public void SetVariables(Dictionary<string, string> variables)
    {
        if (variables == null) return;

        foreach (var pair in variables)
        {
            SetVariable(pair.Key, pair.Value);
        }
    }

    /// <summary>
    /// Gets the formatted prompt with all variables substituted
    /// </summary>
    /// <returns>Formatted prompt text</returns>
    public string GetFormattedPrompt()
    {
        // Use regex to find and replace all variables
        string result = _variablePattern.Replace(_templateText, match =>
        {
            string varName = match.Groups[1].Value;
            if (_variables.TryGetValue(varName, out string value))
            {
                return value;
            }

            // Keep the placeholder if no value is found
            Debug.LogWarning($"PromptTemplate: Variable {varName} not found");
            return match.Value;
        });

        return result;
    }

    /// <summary>
    /// Clears all variable values
    /// </summary>
    public void ClearVariables()
    {
        _variables.Clear();
    }

    /// <summary>
    /// Implements IPromptTemplate.RenderAsync using the internal formatting logic
    /// </summary>
    /// <param name="kernel">Semantic Kernel instance</param>
    /// <param name="arguments">Kernel arguments containing variable values</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task with rendered prompt string</returns>
    public Task<string> RenderAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken = default)
    {
        // Convert KernelArguments to variables
        if (arguments != null)
        {
            foreach (var key in arguments.Names)
            {
                if (arguments.TryGetValue(key, out object value))
                {
                    SetVariable(key, value?.ToString() ?? string.Empty);
                }
            }
        }

        // Return formatted prompt as completed task
        return Task.FromResult(GetFormattedPrompt());
    }

    /// <summary>
    /// Updates the template text
    /// </summary>
    /// <param name="templateText">New template text</param>
    public void UpdateTemplate(string templateText)
    {
        _templateText = templateText ?? string.Empty;
    }
}