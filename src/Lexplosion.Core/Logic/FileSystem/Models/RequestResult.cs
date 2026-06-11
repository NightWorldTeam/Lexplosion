using System.Net;

namespace Lexplosion.Logic.FileSystem.Models
{
    public readonly struct RequestResult
    {
        public readonly bool IsSucces => State == RequestResultState.Succes;
        public readonly RequestResultState State;
        public readonly HttpStatusCode? StatusCode;
        public readonly string Content;

        private RequestResult(RequestResultState state, string content, HttpStatusCode? statusCode)
        {
            State = state;
            Content = content;
            StatusCode = statusCode;
        }

        public static RequestResult Success(string content, HttpStatusCode? statusCode = null)
        {
            return new RequestResult(RequestResultState.Succes, content, statusCode);
        }

        public static RequestResult Error(HttpStatusCode? statusCode = null)
        {
            if (statusCode == null)
            {

                return new RequestResult(RequestResultState.NetworkError, null, null);
            }
            else
            {
                return new RequestResult(RequestResultState.RequestError, null, statusCode);
            }
        }

    }
}
