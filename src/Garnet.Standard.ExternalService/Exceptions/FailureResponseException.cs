using System;

namespace Garnet.Standard.ExternalService.Exceptions;

/// <summary>
/// An exception that is used when a failed response has been received
/// </summary>
public class FailureResponseException : Exception
{
    /// <summary>
    /// An exception that is used when a failed response has been received
    /// </summary>
    public FailureResponseException() : base("The request has been responded with failure status")
    {
    }
}