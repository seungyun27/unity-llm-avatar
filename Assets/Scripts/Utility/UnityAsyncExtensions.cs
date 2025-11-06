using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace UnityLLMAvatar.Utility
{
    public static class UnityAsyncExtensions
    {
        /// <summary>
        /// Enables `await` on `SendWebRequest()` operations.
        /// Usage: await request.SendWebRequest();
        /// </summary>
        public static TaskAwaiter<UnityWebRequest> GetAwaiter(this UnityWebRequestAsyncOperation asyncOperation)
        {
            var tcs = new TaskCompletionSource<UnityWebRequest>();
            // Sets the result when the operation completes, whether successfully or not
            asyncOperation.completed += _ => tcs.SetResult(asyncOperation.webRequest);
            return tcs.Task.GetAwaiter();
        }
    }
}