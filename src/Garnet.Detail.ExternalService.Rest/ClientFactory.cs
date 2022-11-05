using System;
using Garnet.Standard.ExternalService.Configurations;
using RestSharp;

namespace Garnet.Detail.ExternalService.Rest;

internal static class ClientFactory
{
    public static RestClient CreateRestClient(ClientConfiguration clientConfiguration)
    {
        var options = new RestClientOptions
        {
            BaseUrl = new Uri(clientConfiguration.BaseUri)
        };

        return CreateRestClient(options);
    }

    public static RestClient CreateRestClient(RestClientOptions restClientOptions)
    {
        return new RestClient(restClientOptions);
    }
}