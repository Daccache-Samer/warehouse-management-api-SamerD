namespace warehouse_management_api.Error_response;

public static class ApiErrorCodes
{
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string DomainRuleViolation = "DOMAIN_RULE_VIOLATION";
    public const string ServerError = "SERVER_ERROR";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
}