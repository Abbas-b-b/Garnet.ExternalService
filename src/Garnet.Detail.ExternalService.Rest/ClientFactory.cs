using System;
using System.Net;
using System.Net.Http;
using Garnet.Standard.ExternalService.Configurations;
using RestSharp;

namespace Garnet.Detail.ExternalService.Rest;

internal static class ClientFactory
{
    public static RestClient CreateRestClient(ClientConfiguration clientConfiguration)
    {
        var httpClientHandler = new HttpClientHandler();

        if (!string.IsNullOrWhiteSpace(clientConfiguration.ProxyAddress))
        {
            httpClientHandler.Proxy = new WebProxy(clientConfiguration.ProxyAddress);
        }

        if (clientConfiguration.IgnoreCertificateValidation)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback =
                (message, certificate, chain, sslPolicyErrors) => true;
        }


        var options = new RestClientOptions
        {
            BaseUrl = new Uri(clientConfiguration.BaseUri),
            ConfigureMessageHandler = _ => httpClientHandler
        };

        return CreateRestClient(options);
    }

    public static RestClient CreateRestClient(RestClientOptions restClientOptions)
    {
        return new RestClient(restClientOptions);
    }
}