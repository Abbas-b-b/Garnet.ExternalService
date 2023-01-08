using RestSharp;

namespace Garnet.Detail.ExternalService.Rest.Utilities;

/// <summary>
/// Utilities for manipulating reqeust
/// </summary>
public class RequestUtility
{
    /// <summary>
    /// Creates a RestSharp request and set Parameters
    /// </summary>
    /// <param name="uri">The relative path of the request</param>
    /// <param name="httpMethod">Request http method</param>
    /// <param name="requestData">An object to fetch parameters from</param>
    /// <returns>RestSharp request after adding the parameters</returns>
    public static RestRequest CreateRestRequestAndAddParameters(string uri, Method httpMethod, object requestData)
    {
        var request = CreateRestRequest(uri, httpMethod);

        AddParametersToTheRequest(request, requestData);

        return request;
    }

    /// <summary>
    /// Creates a new instance of a RestSharp request
    /// </summary>
    /// <param name="uri">Relative path of the request</param>
    /// <param name="httpMethod"></param>
    /// <returns>RestSharp request</returns>
    public static RestRequest CreateRestRequest(string uri, Method httpMethod)
    {
        return new RestRequest(uri, httpMethod);
    }

    /// <summary>
    /// Add parameters to the RestSharp request. If the request is a GET, parameters will be added as query string
    /// </summary>
    /// <param name="request">RestSharp request to add parameters to</param>
    /// <param name="requestData">An object of parameters to be added to the RestSharp request</param>
    public static void AddParametersToTheRequest(RestRequest request, object requestData)
    {
        if (requestData is null)
        {
            return;
        }
        
        if (request.Method == Method.Get)
        {
            foreach (var prop in requestData.GetType().GetProperties())
            {
                var value = prop.GetValue(requestData)?.ToString();
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }
                
                request.AddParameter(prop.Name, value);
            }
        }
        else
        {
            request.AddJsonBody(requestData);
        }
    }
}