namespace SportPneus.Api.Shared;

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);

public static class TextNormalization
{
    public static string? Digits(string? value) => string.IsNullOrWhiteSpace(value) ? null : new string(value.Where(char.IsDigit).ToArray());
    public static string Required(string value) => value.Trim();
    public static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    public static string Plate(string value) => new(value.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
    public static string Code(string value) => value.Trim().ToUpperInvariant();
}
