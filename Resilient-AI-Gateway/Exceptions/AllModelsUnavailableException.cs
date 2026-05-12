namespace Resilient_AI_Gateway.Exceptions;

public class AllModelsUnavailableException : Exception
{
    public AllModelsUnavailableException(string message) : base(message)
    {
    }

    public AllModelsUnavailableException(string message, Exception inner) : base(message, inner)
    {
    }
}