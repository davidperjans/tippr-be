namespace Application.Common
{
    public sealed record Error(
        ErrorType Type,
        string Message,
        string? Code = null,
        Dictionary<string, string[]>? ValidationErrors = null
    );
}
