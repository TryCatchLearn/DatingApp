namespace API.Errors;

public class ApiException
{
    public ApiException(int statusCode, string message, string details)
    {
        StatusCode = statusCode;
        Message = message;
        Details = details;
    }

    public int StatusCode { get; }
    public string Message { get; }
    public string Details { get; }
}
