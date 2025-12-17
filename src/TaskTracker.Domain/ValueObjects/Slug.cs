using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.ValueObjects;

/// <summary>
/// Value object representing a slug (URL-friendly identifier)
/// </summary>
public class Slug : ValueObject
{
    public string Value { get; }

    private Slug(string value)
    {
        Value = value;
    }

    public static Result<Slug> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Slug>("Slug cannot be empty");

        var slug = Normalize(value);

        if (slug.Length < 2)
            return Result.Failure<Slug>("Slug must be at least 2 characters long");

        if (slug.Length > 50)
            return Result.Failure<Slug>("Slug cannot exceed 50 characters");

        return Result.Success(new Slug(slug));
    }

    private static string Normalize(string input)
    {
        // Convert to lowercase, replace spaces and special chars with hyphens
        var normalized = input.ToLowerInvariant().Trim();
        
        // Replace non-alphanumeric with hyphens
        var result = new char[normalized.Length];
        for (int i = 0; i < normalized.Length; i++)
        {
            result[i] = char.IsLetterOrDigit(normalized[i]) ? normalized[i] : '-';
        }
        
        var slug = new string(result);
        
        // Remove multiple consecutive hyphens
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");
        
        // Remove leading/trailing hyphens
        return slug.Trim('-');
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
