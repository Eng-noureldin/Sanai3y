namespace Sanai3y.Services.Common;

// Custom domain exception representing business rule violations
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }
}