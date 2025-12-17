namespace TaskTracker.Application.Common.Exceptions;

/// <summary>
/// Exception for business rule violations
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
