﻿using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using SEEK.AdPostingApi.Client.Models;
using SEEK.AdPostingApi.Client.Resources;

namespace SEEK.AdPostingApi.Client
{
    public class AdPostingApiClient : IAdPostingApiClient
    {
        private readonly IOAuth2TokenClient _tokenClient;
        private IndexResource _indexResource;
        private readonly Lazy<Task> _ensureIndexResourceInitialised;
        private readonly Lazy<Hal.Client> _client;

        public AdPostingApiClient(string id, string secret, Environment env = Environment.Production) : this(id, secret, env.GetAttribute<EnvironmentUrlAttribute>().Uri)
        {

        }

        public AdPostingApiClient(string id, string secret, Uri adPostingUri) : this(adPostingUri, new OAuth2TokenClient(id, secret))
        {
        }

        internal AdPostingApiClient(Uri adPostingUri, IOAuth2TokenClient tokenClient)
        {
            this._ensureIndexResourceInitialised = new Lazy<Task>(() => this.InitialiseIndexResource(adPostingUri), LazyThreadSafetyMode.ExecutionAndPublication);
            this._tokenClient = tokenClient;
            this._client = new Lazy<Hal.Client>(() => {
              return new Hal.Client(new HttpClient(AddMessageHandler(new AdPostingApiMessageHandler(new OAuthMessageHandler(tokenClient)))));
            });
        }

        protected virtual HttpMessageHandler AddMessageHandler(HttpMessageHandler innerMessageHandler)
        {
            return innerMessageHandler;
        }

        private Task EnsureIndexResourceInitialised()
        {
            return this._ensureIndexResourceInitialised.Value;
        }

        internal async Task InitialiseIndexResource(Uri adPostingUri)
        {
            this._indexResource = await this._client.Value.GetResourceAsync<IndexResource>(adPostingUri);
        }

        public async Task<AdvertisementResource> CreateAdvertisementAsync(Advertisement advertisement)
        {
            if (advertisement == null)
            {
                throw new ArgumentNullException(nameof(advertisement));
            }

            await this.EnsureIndexResourceInitialised();

            return await this._indexResource.CreateAdvertisementAsync(advertisement);
        }

        public async Task<AdvertisementResource> ExpireAdvertisementAsync(Guid advertisementId)
        {
            await this.EnsureIndexResourceInitialised();

            return await this.ExpireAdvertisementAsync(this._indexResource.GenerateAdvertisementUri(advertisementId));
        }

        public async Task<AdvertisementResource> ExpireAdvertisementAsync(Uri uri)
        {
            return await this._client.Value.PatchResourceAsync<AdvertisementResource, ExpireAdvertisementJsonPatch>(uri, new ExpireAdvertisementJsonPatch());
        }

        public async Task<AdvertisementResource> GetAdvertisementAsync(Guid advertisementId)
        {
            await this.EnsureIndexResourceInitialised();

            return await this.GetAdvertisementAsync(this._indexResource.GenerateAdvertisementUri(advertisementId));
        }

        public async Task<AdvertisementResource> GetAdvertisementAsync(Uri uri)
        {
            return await this._client.Value.GetResourceAsync<AdvertisementResource>(uri);
        }

        public async Task<ProcessingStatus> GetAdvertisementStatusAsync(Guid advertisementId)
        {
            await this.EnsureIndexResourceInitialised();

            return await this.GetAdvertisementStatusAsync(this._indexResource.GenerateAdvertisementUri(advertisementId));
        }

        public async Task<ProcessingStatus> GetAdvertisementStatusAsync(Uri uri)
        {
            HttpResponseHeaders httpResponseHeaders = await this._client.Value.HeadResourceAsync<AdvertisementResource>(uri);

            return (ProcessingStatus)Enum.Parse(typeof(ProcessingStatus), httpResponseHeaders.GetValues("Processing-Status").Single());
        }

        public async Task<AdvertisementSummaryPageResource> GetAllAdvertisementsAsync(string advertiserId = null)
        {
            await this.EnsureIndexResourceInitialised();

            return await this._indexResource.GetAllAdvertisements(advertiserId);
        }

        public async Task<AdvertisementResource> UpdateAdvertisementAsync(Guid advertisementId, Advertisement advertisement)
        {
            if (advertisement == null)
                throw new ArgumentNullException(nameof(advertisement));

            await this.EnsureIndexResourceInitialised();

            return await this.UpdateAdvertisementAsync(this._indexResource.GenerateAdvertisementUri(advertisementId), advertisement);
        }

        public async Task<AdvertisementResource> UpdateAdvertisementAsync(Uri uri, Advertisement advertisement)
        {
            if (advertisement == null)
                throw new ArgumentNullException(nameof(advertisement));

            return await this._client.Value.PutResourceAsync<AdvertisementResource, Advertisement>(uri, advertisement);
        }

        public void Dispose()
        {
            this._tokenClient.Dispose();
            this._client.Value.Dispose();
        }
    }
}