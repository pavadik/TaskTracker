using System.Text.RegularExpressions;
using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.ValueObjects;

public partial class Slug : ValueObject
{
    public string Value { get; private set; } = string.Empty;

    private Slug() { }

    private Slug(string value)
    {
        Value = value;
    }

    public static Result<Slug> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result.Failure<Slug>("Slug cannot be empty");

        var slug = input.Trim().ToLowerInvariant();
        slug = NonAlphanumericRegex().Replace(slug, "");
        slug = WhitespaceRegex().Replace(slug, "-");
        slug = slug.Trim('-');

        if (string.IsNullOrEmpty(slug))
            return Result.Failure<Slug>("Slug resulted in empty string after sanitizing");

        if (slug.Length < 2)
            return Result.Failure<Slug>("Slug must be at least 2 characters");

        if (slug.Length > 50)
            slug = slug[..50].TrimEnd('-');

        return Result.Success(new Slug(slug));
    }

    public static implicit operator string(Slug slug) => slug.Value;

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex("[^a-z0-9\\s-]")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex("[\\s]+")]
    private static partial Regex WhitespaceRegex();
}
