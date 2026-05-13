namespace Resilient_AI_Gateway.Logging;

public class RequestLogger : IRequestLogger
{
    private readonly LoggingChannel _channel;

    public RequestLogger(LoggingChannel channel)
    {
        _channel = channel;
    }

    public bool TryWrite(RequestLogDocument logDocument)
    {
        return _channel.Writer.TryWrite(logDocument);
    }

    public void Log(RequestLogDocument logDocument)
    {
        TryWrite(logDocument);
    }
}