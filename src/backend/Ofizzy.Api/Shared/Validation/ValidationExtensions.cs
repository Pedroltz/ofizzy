using FluentValidation.Results;

namespace Ofizzy.Api.Shared.Validation;
public static class ValidationExtensions
{
    public static Dictionary<string, string[]> ToDictionary(this ValidationResult result) => result.Errors
        .GroupBy(error => error.PropertyName)
        .ToDictionary(group => char.ToLowerInvariant(group.Key[0]) + group.Key[1..], group => group.Select(error => error.ErrorMessage).Distinct().ToArray());
}
