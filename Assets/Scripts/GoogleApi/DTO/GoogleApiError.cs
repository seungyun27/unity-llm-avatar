using System;
using UnityEngine.Networking;

namespace UnityLLMAvatar.GoogleApi.DTO
{
    [Serializable]
    public struct ErrorType
    {
        public int Code;
        public string Message;
    }
    
    [Serializable]
    public struct GoogleApiError
    {
        public ErrorType Error;

        public GoogleApiError(UnityWebRequest request)
        {
            Error = new ErrorType()
            {
                Code = (int)request.responseCode,
                Message = request.error
            };
        }
    }
}