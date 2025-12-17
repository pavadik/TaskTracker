using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.ValueObjects;

/// <summary>
/// Value object representing an email address
/// </summary>
public class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>("Email cannot be empty");

        email = email.Trim().ToLowerInvariant();

        if (email.Length > 256)
            return Result.Failure<Email>("Email cannot exceed 256 characters");

        // Basic email validation
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
            return Result.Failure<Email>("Invalid email format");

        var dotIndex = email.LastIndexOf('.');
        if (dotIndex <= atIndex + 1 || dotIndex == email.Length - 1)
            return Result.Failure<Email>("Invalid email format");

        return Result.Success(new Email(email));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
