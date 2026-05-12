namespace Resilient_AI_Gateway.Exceptions;

public class InvalidApiKeyException : Exception
{
    public InvalidApiKeyException(string message) : base(message)
    {
    }
}