using System;

namespace Garnet.Standard.ExternalService.Exceptions;

/// <summary>
/// An exception for response deserialize failure
/// </summary>
public class ResponseDeserializeException : Exception
{
    /// <summary>
    /// An exception for response deserialize failure
    /// </summary>
    public ResponseDeserializeException() : base("The response data could not be deserialized")
    {
    }
}