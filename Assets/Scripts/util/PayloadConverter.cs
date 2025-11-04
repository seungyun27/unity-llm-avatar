using System.Text;
using Newtonsoft.Json;

namespace UnityLLMAvatar.util
{
    public static class PayloadConverter
    {
        public static byte[] ToBytes<T>(T payload) where T : class
        {
            var asJson = JsonConvert.SerializeObject(payload);
            return Encoding.UTF8.GetBytes(asJson);
        }
    }
}