using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.ValueObjects;

/// <summary>
/// Value object representing a friendly task identifier (e.g., PRJ-123)
/// </summary>
public class FriendlyId : ValueObject
{
    public string ProjectPrefix { get; }
    public int SequenceNumber { get; }
    public string Value => $"{ProjectPrefix}-{SequenceNumber}";

    private FriendlyId(string projectPrefix, int sequenceNumber)
    {
        ProjectPrefix = projectPrefix;
        SequenceNumber = sequenceNumber;
    }

    public static Result<FriendlyId> Create(string projectPrefix, int sequenceNumber)
    {
        if (string.IsNullOrWhiteSpace(projectPrefix))
            return Result.Failure<FriendlyId>("Project prefix cannot be empty");

        if (projectPrefix.Length > 10)
            return Result.Failure<FriendlyId>("Project prefix cannot exceed 10 characters");

        if (!projectPrefix.All(char.IsLetterOrDigit))
            return Result.Failure<FriendlyId>("Project prefix can only contain letters and digits");

        if (sequenceNumber <= 0)
            return Result.Failure<FriendlyId>("Sequence number must be positive");

        return Result.Success(new FriendlyId(projectPrefix.ToUpperInvariant(), sequenceNumber));
    }

    public static Result<FriendlyId> Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<FriendlyId>("Friendly ID cannot be empty");

        var parts = value.Split('-');
        if (parts.Length != 2)
            return Result.Failure<FriendlyId>("Invalid friendly ID format. Expected format: PREFIX-NUMBER");

        if (!int.TryParse(parts[1], out var sequenceNumber))
            return Result.Failure<FriendlyId>("Invalid sequence number in friendly ID");

        return Create(parts[0], sequenceNumber);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ProjectPrefix;
        yield return SequenceNumber;
    }

    public override string ToString() => Value;
}
