using System.Net;

namespace InternetGrounding.Tools.Models;

/// <summary>
/// Exception thrown when an error occurs during interaction with the Gemini API.
/// </summary>
public class GeminiApiException : Exception
{
    /// <summary>
    /// Gets the HTTP status code associated with the API error, if available.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeminiApiException"/> class.
    /// </summary>
    public GeminiApiException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeminiApiException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public GeminiApiException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeminiApiException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public GeminiApiException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeminiApiException"/> class with a specified error message and HTTP status code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="statusCode">The HTTP status code associated with the API error.</param>
    public GeminiApiException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeminiApiException"/> class with a specified error message, HTTP status code, and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="statusCode">The HTTP status code associated with the API error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public GeminiApiException(string message, HttpStatusCode statusCode, Exception innerException) : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}