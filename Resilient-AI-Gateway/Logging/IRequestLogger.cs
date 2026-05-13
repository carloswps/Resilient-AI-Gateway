namespace Resilient_AI_Gateway.Logging;

public interface IRequestLogger
{
    /// <summary>
    /// Attempts to log a request using the provided log document, which contains extensive
    /// details about the request, such as the client ID, endpoint, status code, latency,
    /// and other related metrics.
    /// </summary>
    /// <param name="logDocument">
    /// An instance of <see cref="RequestLogDocument"/> containing all relevant data
    /// about the request to be logged, including request ID, timestamp, client information,
    /// endpoint details, HTTP method, model usage, retries, fallback status, error information,
    /// and performance metrics.
    /// </param>
    /// <returns>
    /// A boolean indicating whether the logging operation was successful. Returns true if
    /// the request was logged successfully, otherwise returns false.
    /// </returns>
    bool TryWrite(RequestLogDocument logDocument);

    /// <summary>
    /// Logs a request using the provided log document, which contains detailed
    /// information about the request such as client ID, endpoint, status code, latency,
    /// and more.
    /// </summary>
    /// <param name="logDocument">
    /// An instance of <see cref="RequestLogDocument"/> containing all relevant data
    /// about the request to be logged, such as request ID, timestamp, client information,
    /// endpoint details, HTTP method, model usage, retry attempts, fallback status, error
    /// details, and performance metrics.
    /// </param>
    void Log(RequestLogDocument logDocument);
}