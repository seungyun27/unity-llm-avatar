using System.Text;
using Newtonsoft.Json;

namespace UnityLLMAvatar.Utility
{
    public static class StructExtensions
    {
        public static byte[] ToBytes<T>(this T payload) where T : struct
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
        }
    }
}