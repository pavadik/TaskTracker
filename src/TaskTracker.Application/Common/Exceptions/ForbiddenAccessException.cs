namespace TaskTracker.Application.Common.Exceptions;

/// <summary>
/// Exception for forbidden access
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base("Access to this resource is forbidden.")
    {
    }

    public ForbiddenAccessException(string message) : base(message)
    {
    }
}
