using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Manages a limited chat history for conversation context
/// Keeps only the most recent messages to maintain relevance
/// </summary>
public class ChatMemory
{
    // Maximum number of messages to retain
    private int _maxHistorySize;

    // Collection of chat messages
    private List<ChatMessage> _messages = new List<ChatMessage>();

    // Define message roles
    public enum MessageRole
    {
        System,
        User,
        Assistant
    }

    // Structure for storing individual messages
    [Serializable]
    public class ChatMessage
    {
        public MessageRole Role;
        public string Content;
        public DateTime Timestamp;

        public ChatMessage(MessageRole role, string content)
        {
            Role = role;
            Content = content;
            Timestamp = DateTime.Now;
        }

        public string GetRoleString()
        {
            return Role.ToString().ToLower();
        }
    }

    /// <summary>
    /// Constructor with configurable history size
    /// </summary>
    /// <param name="maxHistorySize">Maximum number of messages to retain (default: 5)</param>
    public ChatMemory(int maxHistorySize = 5)
    {
        _maxHistorySize = Mathf.Clamp(maxHistorySize, 3, 10);
    }

    /// <summary>
    /// Adds a new message to the chat history
    /// </summary>
    /// <param name="role">Role of the message sender</param>
    /// <param name="content">Content of the message</param>
    public void AddMessage(MessageRole role, string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            Debug.LogWarning("ChatMemory: Attempted to add empty message");
            return;
        }

        _messages.Add(new ChatMessage(role, content));

        // Maintain size limit by removing oldest messages
        if (_messages.Count > _maxHistorySize)
        {
            _messages.RemoveAt(0);
        }
    }

    /// <summary>
    /// Returns formatted chat history suitable for LLM prompt
    /// </summary>
    /// <returns>Formatted history string</returns>
    public string GetFormattedHistory()
    {
        if (_messages.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder();

        foreach (var message in _messages)
        {
            sb.AppendLine($"{message.GetRoleString()}: {message.Content}");
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Returns all messages in the current history
    /// </summary>
    /// <returns>Copy of message list</returns>
    public List<ChatMessage> GetMessages()
    {
        return new List<ChatMessage>(_messages);
    }

    /// <summary>
    /// Removes all messages from history
    /// </summary>
    public void Clear()
    {
        _messages.Clear();
    }

    /// <summary>
    /// Gets current message count
    /// </summary>
    public int Count => _messages.Count;
}