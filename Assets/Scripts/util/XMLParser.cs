using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace UnityLLMAvatar.util
{
    public static class XMLParser
    {
        public static LLMResponse ParseLLMResponse(string xmlString)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            if (xmlDoc.DocumentElement == null)
            {
                return new LLMResponse();
            }

            var response = new LLMResponse
            {
                Topic = GetInnerText(xmlDoc, "/response/topic"),
                Analysis = GetInnerText(xmlDoc, "/response/analysis"),
                Answer = GetInnerText(xmlDoc, "/response/answer")
            };
            return response;
        }

        private static string GetInnerText(XmlDocument xmlDoc, string xpath)
        {
            var node = xmlDoc.SelectSingleNode(xpath);
            return node?.InnerText ?? "";
        }
    }

    public class LLMResponse
    {
        public string Topic;
        public string Analysis;
        public string Answer; 

        public override string ToString()
        {
            return JsonConvert.SerializeObject(
                this,
                Formatting.Indented
            );
        }
    }
}