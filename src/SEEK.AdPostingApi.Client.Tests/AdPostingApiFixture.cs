﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using SEEK.AdPostingApi.Client;
using SEEK.AdPostingApi.Client.Models;

namespace SEEK.AdPostingApi.Client.Tests
{
    public class AdPostingApiFixture : IDisposable
    {
        static AdPostingApiFixture()
        {
            // See https://github.com/dennisdoomen/fluentassertions/issues/305 - ShouldBeEquivalentTo fails with objects from the System namespace.
            // Due to this, we need to change the IsValueType predicate so that it does not assume System.Exception and derivatives of it in the System namespace are value types.
            AssertionOptions.IsValueType = type => (type.Namespace == typeof(int).Namespace) && !(type == typeof(Exception) || type.IsSubclassOf(typeof(Exception)));
        }

        public AdPostingApiFixture(AdPostingApiPactService adPostingApiPactService)
        {
            this.AdPostingApiService = adPostingApiPactService.MockProviderService;
            this.AdPostingApiService.ClearInteractions();
            this.AdPostingApiServiceBaseUri = adPostingApiPactService.MockProviderServiceBaseUri;
        }

        public IMockProviderService AdPostingApiService { get; }

        public Uri AdPostingApiServiceBaseUri { get; }

        public void Dispose()
        {
            this.AdPostingApiService.VerifyInteractions();
        }

        public AdPostingApiClient GetClient(OAuth2Token token)
        {
            var oAuthClient = Mock.Of<IOAuth2TokenClient>(c => c.GetOAuth2TokenAsync() == Task.FromResult(token));

            return new AdPostingApiClient(this.AdPostingApiServiceBaseUri, oAuthClient);
        }

        public void RegisterIndexPageInteractions(OAuth2Token token)
        {
            const string advertisementLink = "/advertisement";

            this.AdPostingApiService
                .UponReceiving($"a request to retrieve API links with Bearer {token.AccessToken}")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = "/",
                    Headers = new Dictionary<string, string>
                    {
                        { "Accept", "application/hal+json" },
                        { "Authorization", $"Bearer {token.AccessToken}" }
                    }
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 200,
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/hal+json; charset=utf-8" }
                    },
                    Body = new
                    {
                        _links = new
                        {
                            advertisements = new
                            {
                                href = advertisementLink + "{?advertiserId}",
                                templated = true
                            },
                            advertisement = new
                            {
                                href = advertisementLink + "/{advertisementId}",
                                templated = true
                            }
                        }
                    }
                });
        }
    }
}