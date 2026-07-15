namespace warehouse_management_api.Error_response;

public class ApiErrorResponse(
    string errorCode,
    string message,
    string traceId,
    IDictionary<string, string[]>? errors = null)
{
    public string ErrorCode { get; init; } = errorCode;
    public string Message { get; init; } = message;
    public string TraceId { get; init; } = traceId;
    public IDictionary<string, string[]>? Errors { get; init; } = errors;
}