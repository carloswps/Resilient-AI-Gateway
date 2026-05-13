using System.Threading.Channels;

namespace Resilient_AI_Gateway.Logging;

public class LoggingChannel
{
    private readonly Channel<RequestLogDocument> _channel;
    
    public LoggingChannel()
    {
        _channel = Channel.CreateBounded<RequestLogDocument>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
        });
    }
    public ChannelReader<RequestLogDocument> Reader => _channel.Reader;
    public ChannelWriter<RequestLogDocument> Writer => _channel.Writer;
}