using System.Net;

namespace DotBoil.Entities
{
    public class BaseResponse
    {
        public object Data { get; private set; }
        public bool IsSuccess { get; private set; }
        public IEnumerable<string> Messages { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }

        public BaseResponse()
        {
            Messages = new List<string>();
        }

        public BaseResponse(object data, HttpStatusCode statusCode)
        {
            Data = data;
            IsSuccess = (int)statusCode >= 200 && (int)statusCode < 400;
            StatusCode = statusCode;
        }

        public BaseResponse(object data, HttpStatusCode statusCode, IEnumerable<string> messages)
        {
            Data = data;
            IsSuccess = (int)statusCode >= 200 && (int)statusCode < 400;
            Messages = messages;
            StatusCode = statusCode;
        }

        public BaseResponse(HttpStatusCode statusCode)
        {
            Data = new { };
            IsSuccess = (int)statusCode >= 200 && (int)statusCode < 400;
            Messages = new List<string>();
            StatusCode = statusCode;
        }

        public BaseResponse(HttpStatusCode statusCode, IEnumerable<string> messages)
        {
            Data = new { };
            IsSuccess = (int)statusCode >= 200 && (int)statusCode < 400;
            Messages = messages;
            StatusCode = statusCode;
        }

        public static BaseResponse Response(object data, HttpStatusCode statusCode) => new BaseResponse(data, statusCode);

        public static BaseResponse Response(object data, HttpStatusCode statusCode, params string[] messages) => new BaseResponse(data, statusCode, messages);

        public static BaseResponse Response(HttpStatusCode statusCode) => new BaseResponse(statusCode);

        public static BaseResponse Response(HttpStatusCode statusCode, params string[] messages) => new BaseResponse(statusCode, messages);
    }

    public class BaseResponse<T> where T : class
    {
        public T Data { get; private set; }
        public bool IsSuccess { get; private set; }
        public IEnumerable<string> Messages { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
    }
}
