using System;
using CandyCoded.env;

namespace UnityLLMAvatar.Utility
{
    public static class EnvManager
    {
        public static string GetApiKey(string keyName)
        {
            return env.TryParseEnvironmentVariable(keyName, out string key)
                ? key
                : throw new InvalidOperationException(
                    $"API key '{keyName}' is not set. Please set the {keyName} in .env file.");
        }
    }
}