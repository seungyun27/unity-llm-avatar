using CandyCoded.env;
using UnityEngine;

namespace UnityLLMAvatar
{
    public static class EnvManager
    {
        public static string GetApiKey(string keyName)
        {
            env.TryParseEnvironmentVariable(keyName, out string key);
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($"API key '{keyName}' is not set. Please set the {keyName} in .env file.");
            }
            return key;
        }
    }
}