using UnityEngine;
using System;
using System.Text;

/// <summary>
/// ScriptableObject for storing API keys and configuration for SKUnityCore
/// </summary>
[CreateAssetMenu(fileName = "SKConfig", menuName = "SK Unity/Core Config", order = 1)]
public class SKUnityCoreConfig : ScriptableObject
{
    // API Key settings (stored encrypted)
    [SerializeField, HideInInspector] private string _openAIKey = "";
    [SerializeField, HideInInspector] private string _azureOpenAIKey = "";
    [SerializeField] private string _azureOpenAIEndpoint = "";
    [SerializeField, HideInInspector] private string _anthropicKey = "";

    // Properties for API keys with encryption/decryption
    public string OpenAIKey
    {
        get => Decrypt(_openAIKey);
        set => _openAIKey = Encrypt(value);
    }

    public string AzureOpenAIKey
    {
        get => Decrypt(_azureOpenAIKey);
        set => _azureOpenAIKey = Encrypt(value);
    }

    public string AzureOpenAIEndpoint
    {
        get => _azureOpenAIEndpoint;
        set => _azureOpenAIEndpoint = value;
    }

    public string AnthropicKey
    {
        get => Decrypt(_anthropicKey);
        set => _anthropicKey = Encrypt(value);
    }

    // Simple encryption - not secure for production
    private string Encrypt(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] result = ApplyXOR(bytes);
        return Convert.ToBase64String(result);
    }

    // Decrypt the stored string
    private string Decrypt(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        try
        {
            byte[] bytes = Convert.FromBase64String(input);
            byte[] result = ApplyXOR(bytes);
            return Encoding.UTF8.GetString(result);
        }
        catch
        {
            return string.Empty;
        }
    }

    // XOR with a device-specific key
    private byte[] ApplyXOR(byte[] data)
    {
        // Use device ID as encryption key
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        byte[] key = Encoding.UTF8.GetBytes(deviceId);

        // Apply XOR operation
        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ key[i % key.Length]);
        }

        return result;
    }
}