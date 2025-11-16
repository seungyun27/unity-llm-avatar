using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using LLMUnity;
using UnityEngine;
using UnityLLMAvatar.GoogleApi;

namespace UnityLLMAvatar
{
    [Serializable]
    struct LogEntry
    {
        public long timestamp;
        public string level;
        public string function;
        public long line;
        public string msg;
    }

    [Serializable]
    public struct ConversationRecord
    {
        [Name("model")] public string model { get; set; }
        [Name("GPU Layer")] public int gpuLayer { get; set; }
        [Name("temperature")] public float temperature { get; set; }
        [Name("system_prompt")] public string systemPrompt { get; set; }
        [Name("system_prompt_length")] public int systemPromptLength { get; set; }
        [Name("request")] public string request { get; set; }
        [Name("request_prompt") ] public string requestPrompt { get; set; }
        [Name("prompt_tokens")] public int promptTokens { get; set; }
        [Name("prompt_eval_time_ms")] public float promptEvalTime_ms { get; set; }
        [Name("response")] public string response { get; set; }
        [Name("eval_tokens")] public int evalTokens { get; set; }
        [Name("eval_time_ms")] public float evalTime_ms { get; set; }
        [Name("total_tokens")] public int totalTokens { get; set; }
        [Name("total_time_ms")] public float totalTime_ms { get; set; }
        // [Name("tts_time_ms")] public float ttsTime_ms { get; set; }
        // [Name("stt_time_ms")] public float sttTime_ms { get; set; }
    }

    public class LLMPerformanceLogger : MonoBehaviour
    {
        public string TTS_script = "Hello!";
        public VoiceScriptableObject TTS_voice;
        
        [SerializeField]
        private LLMUnity.LLM _llm;

        [SerializeField]
        private LLMCharacter _llmCharacter;

        private string logFilePath;
        private ConversationRecord currentRecord;
        private const string LLM_REQUEST_PREFIX = "[LLM_REQUEST]";
        private const string LLM_RESPONSE_PREFIX = "[LLM_RESPONSE]";
        private const string LLM_CHARACTER_PREFIX = "[LLM_CHARACTER]";
        private const string TTS_API_PREFIX = "[TTS_API]";
        private const string STT_API_PREFIX = "[STT_API]";

        private void Awake()
        {
            var logDir = Path.Combine(Application.dataPath, "LLMTestResult222~");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var modelName = _llm.model.Replace(".gguf", "");
            logFilePath = Path.Combine(logDir, $"{modelName}___{timestamp}.csv");

            currentRecord = new ConversationRecord
            {
                model = _llm.model.Replace(".gguf", ""),
                temperature = _llmCharacter.temperature,
                gpuLayer = _llm.numGPULayers,
            };

            using var writer = new StreamWriter(logFilePath, true);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteHeader<ConversationRecord>();
            csv.NextRecord();
            Application.logMessageReceived += HandleLog;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (logString.Contains("slot print_timing:") && logString.Contains("prompt eval time ="))
            {
                ParseAndLogTiming(logString);
            }
            else if(logString.StartsWith(LLM_CHARACTER_PREFIX))
            {
                currentRecord.requestPrompt = logString.Substring(LLM_CHARACTER_PREFIX.Length);
            }
            else if (logString.StartsWith(LLM_REQUEST_PREFIX))
            {
                currentRecord.request = logString.Substring(LLM_REQUEST_PREFIX.Length);
                currentRecord.systemPrompt = _llmCharacter.prompt;
                currentRecord.systemPromptLength = _llmCharacter.prompt.Length;
            }
            else if (logString.StartsWith(LLM_RESPONSE_PREFIX))
            {
                currentRecord.response = logString.Substring(LLM_RESPONSE_PREFIX.Length);
                SaveCurrentRecord();
            }
            /*else if (logString.StartsWith(TTS_API_PREFIX))
            {
                string timing = logString.Substring(TTS_API_PREFIX.Length);
                if (float.TryParse(timing.Trim(), out float ttsTime))
                {
                    currentRecord.ttsTime_ms = ttsTime;
                }

                SaveCurrentRecord();

                currentRecord = new ConversationRecord
                {
                    model = _llm.model.Replace(".gguf", ""),
                    temperature = _llmCharacter.temperature,
                    systemPrompt = _llmCharacter.prompt,
                    systemPromptLength = _llmCharacter.prompt.Length
                };
            }
            else if (logString.StartsWith(STT_API_PREFIX))
            {
                string timing = logString.Substring(STT_API_PREFIX.Length);
                if (float.TryParse(timing.Trim(), out float sttTime))
                {
                    currentRecord.sttTime_ms = sttTime;
                }
            }*/
        }

        private void SaveCurrentRecord()
        {
            using var writer = new StreamWriter(logFilePath, true);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecord(currentRecord);
            csv.NextRecord();
        }

        private void ParseAndLogTiming(string logString)
        {
            /*
                slot print_timing: id  0 | task 0 | 
                prompt eval time =   10156.53 ms /   683 tokens (   14.87 ms per token,    67.25 tokens per second)
                eval time =       0.17 ms /     1 tokens (    0.17 ms per token,  6024.10 tokens per second)
                total time =   10156.70 ms /   684 tokens
            */

            string[] lines = logString.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            // Find line with `"function": "print_timings"`
            var stats = lines.Select(line => JsonUtility.FromJson<LogEntry>(line))
                .Where(entry => entry.function == "print_timings")
                .Select(entry => entry.msg)
                .SelectMany(msg => msg.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                .ToList();

            foreach (string stat in stats)
            {
                if (stat.Contains("prompt eval time ="))
                {
                    string[] parts = stat.Split(new[] { '=', '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        float.TryParse(parts[1].Trim().Split(' ')[0], out var time);
                        currentRecord.promptEvalTime_ms = time;
                        int.TryParse(parts[2].Trim().Split(' ')[0], out var tokens);
                        currentRecord.promptTokens = tokens;
                    }
                }
                else if (stat.Contains("eval time ="))
                {
                    string[] parts = stat.Split(new[] { '=', '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        float.TryParse(parts[1].Trim().Split(' ')[0], out var time);
                        currentRecord.evalTime_ms = time;
                        int.TryParse(parts[2].Trim().Split(' ')[0], out var tokens);
                        currentRecord.evalTokens = tokens;
                    }
                }
                else if (stat.Contains("total time ="))
                {
                    string[] parts = stat.Split(new[] { '=', '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        float.TryParse(parts[1].Trim().Split(' ')[0], out var time);
                        currentRecord.totalTime_ms = time;
                        int.TryParse(parts[2].Trim().Split(' ')[0], out var tokens);
                        currentRecord.totalTokens = tokens;
                    }
                }
            }
        }
    }
}
