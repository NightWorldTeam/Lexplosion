using System.Net;

namespace Lexplosion.Logic.FileSystem.Models
{
    public readonly struct DownloadFileResult
    {
        public readonly bool IsSucces => State == RequestResultState.Succes;
        public readonly RequestResultState State;
        public readonly HttpStatusCode? StatusCode;

        private DownloadFileResult(RequestResultState state)
        {
            State = state;
        }

        private DownloadFileResult(RequestResultState state, HttpStatusCode? statusCode)
        {
            State = state;
            StatusCode = statusCode;
        }

        public static DownloadFileResult Success(HttpStatusCode statusCode)
        {
            return new DownloadFileResult(RequestResultState.Succes, statusCode);
        }

        public static DownloadFileResult RequestError(HttpStatusCode statusCode)
        {
            return new DownloadFileResult(RequestResultState.RequestError, statusCode);
        }

        public static DownloadFileResult NetworkError()
        {
            return new DownloadFileResult(RequestResultState.NetworkError, null);
        }

        public static DownloadFileResult HandleError()
        {
            return new DownloadFileResult(RequestResultState.HandleError);
        }

        public static DownloadFileResult DownloadError(HttpStatusCode? statusCode)
        {
            if (statusCode == null)
            {
                return new DownloadFileResult(RequestResultState.NetworkError, null);
            }
            else
            {
                return new DownloadFileResult(RequestResultState.RequestError, statusCode);
            }
        }

    }
}

