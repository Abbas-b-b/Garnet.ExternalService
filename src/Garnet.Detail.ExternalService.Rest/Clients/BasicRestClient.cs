using System.Linq;
using System.Threading.Tasks;
using Garnet.Detail.ExternalService.Rest.Utilities;
using Garnet.Standard.ExternalService.Configurations;
using Garnet.Standard.ExternalService.Exceptions;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Garnet.Detail.ExternalService.Rest.Clients;

/// <summary>
/// A basic rest client for simple requests
/// </summary>
public abstract class BasicRestClient
{
    /// <summary>
    /// RestSharp client for handling requests
    /// </summary>
    protected readonly RestClient Client;
    
    /// <summary>
    /// Information for configuring RestSharp client
    /// </summary>
    protected readonly ClientConfiguration ClientConfiguration;
    
    /// <summary>
    /// 
    /// </summary>
    protected readonly ILogger<BasicRestClient> Logger;

    /// <summary>
    /// A basic rest client for simple requests
    /// </summary>
    /// <param name="clientConfiguration">To configure client with base uri</param>
    /// <param name="logger"></param>
    public BasicRestClient(ClientConfiguration clientConfiguration, ILogger<BasicRestClient> logger)
    {
        ClientConfiguration = clientConfiguration;
        Logger = logger;
        Client = CreateRestClient();
    }

    /// <summary>
    /// Sends the request using the configured client. Use <see cref="SendRequestAsync{TRequest,TResponse}"/> for hiding the RestSharp and leveraging failure handling and others
    /// </summary>
    /// <param name="request">RestSharp request to send</param>
    /// <returns>RestResponse</returns>
    public virtual Task<RestResponse> SendRequestAsync(RestRequest request)
    {
        return
            Client.ExecuteAsync(request);
    }

    /// <summary>
    /// Sends the request using the configured client.
    /// </summary>
    /// <param name="uri">Relative path for the request</param>
    /// <param name="httpMethod">HttpMethod of the request</param>
    /// <param name="request">Request parameters which will be added to the body or query parameters in respect to the request method (GET or not)</param>
    /// <typeparam name="TRequest">Type of the request object</typeparam>
    /// <typeparam name="TResponse">Type of the response object</typeparam>
    /// <returns>Response object if request is success</returns>
    public virtual async Task<TResponse> SendRequestAsync<TRequest, TResponse>(string uri, Method httpMethod,
        TRequest request)
        where TRequest : class
        where TResponse : class
    {
        var restRequest = RequestUtility.CreateRestRequestAndAddParameters(uri, httpMethod, request);

        LogRequestBeforeSending(restRequest);
        var restResponse = await SendRequestAsync(restRequest);
        LogResponseReceived(restResponse);

        if (IsSuccessResponse(restResponse))
        {
            return DeserializeResponse<TResponse>(restResponse);
        }

        LogFailedResponseReceived(restRequest, restResponse);
        return HandleFailure<TResponse>(restResponse);
    }

    /// <summary>
    /// What to do on failure response
    /// </summary>
    /// <param name="restResponse">The failure response</param>
    /// <typeparam name="TResponse">Type of the response</typeparam>
    /// <returns>What to return if failure occured</returns>
    /// <exception cref="FailureResponseException">Default behavior</exception>
    public virtual TResponse HandleFailure<TResponse>(RestResponse restResponse) where TResponse : class
    {
        throw new FailureResponseException();
    }

    /// <summary>
    /// A method to determine whether the response is a failed one or not. On default it is determined based on the status code
    /// </summary>
    /// <param name="response">RestSharp RestResponse</param>
    /// <returns>whether the response a failure or not</returns>
    public virtual bool IsSuccessResponse(RestResponse response)
    {
        return response.IsSuccessful;
    }

    /// <summary>
    /// This method is called inside the constructor once and apply the configuration
    /// </summary>
    /// <returns>RestSharp client</returns>
    protected virtual RestClient CreateRestClient()
    {
        return ClientFactory.CreateRestClient(ClientConfiguration);
    }

    /// <summary>
    /// Deserialize response
    /// </summary>
    /// <param name="restResponse">To get the response content from</param>
    /// <typeparam name="TResponse">The type to deserialize to</typeparam>
    /// <returns>Deserialized object</returns>
    public virtual TResponse DeserializeResponse<TResponse>(RestResponse restResponse) where TResponse : class
    {
        var result = Client.Deserialize<TResponse>(restResponse);

        if (result.Data is not null)
        {
            return result.Data;
        }

        Logger.LogError("Could not deserialize response content {content}. The error {error} and exception {@exception}", 
            result.Content, result.ErrorMessage, result.ErrorException);

        throw new ResponseDeserializeException();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="restRequest"></param>
    protected void LogRequestBeforeSending(RestRequest restRequest)
    {
        Logger.LogInformation("A {httpMethod} request is about to send to {uri}",
            restRequest.Method,
            restRequest.Resource);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="restResponse"></param>
    protected void LogResponseReceived(RestResponse restResponse)
    {
        Logger.LogInformation("A response received with status {status}", restResponse.StatusCode);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="restRequest"></param>
    /// <param name="restResponse"></param>
    protected void LogFailedResponseReceived(RestRequest restRequest, RestResponse restResponse)
    {
        Logger.LogError(restResponse.ErrorException,
            "A {httpMethod} request to {uri} with parameters {@parameters} has been failed with status {status} and error: {error} and content: {content}",
            restRequest.Method,
            restRequest.Resource,
            restRequest.Parameters.ToList(),
            restResponse.StatusCode,
            restResponse.ErrorMessage,
            restResponse.Content);
    }
}