using System.Diagnostics;
using System.Xml;
using Newtonsoft.Json;
using UnityEngine;

namespace UnityLLMAvatar
{
    public static class XMLParser
    {
        public static LLMResponse ParseLLMResponse(string xmlString)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            if (xmlDoc == null)
            {
                return new();
            }

            var response = new LLMResponse();
            response.topic = GetInnerText(xmlDoc, "/response/topic");
            response.analysis = GetInnerText(xmlDoc, "/response/analysis");
            response.answer = GetInnerText(xmlDoc, "/response/answer");
            return response;
        }

        private static string GetInnerText(XmlDocument xmlDoc, string xpath)
        {
            var node = xmlDoc.SelectSingleNode(xpath);
            return node?.InnerText ?? "";
        }
    }

    public struct LLMResponse
    {
        public string topic;
        public string analysis;
        public string answer; 

        public override string ToString()
        {
            return JsonConvert.SerializeObject(
                this,
                Newtonsoft.Json.Formatting.Indented
            );
        }
    }
}