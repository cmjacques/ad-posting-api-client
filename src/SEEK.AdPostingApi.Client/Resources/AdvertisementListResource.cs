﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SEEK.AdPostingApi.Client.Hal;

namespace SEEK.AdPostingApi.Client.Resources
{
    public class AdvertisementListResource : HalResource, IEnumerable<EmbeddedAdvertisementResource>
    {
        internal AdvertisementListEmbeddedResources Embedded { get; set; }

        internal class AdvertisementListEmbeddedResources
        {
            public IEnumerable<EmbeddedAdvertisementResource> Advertisements { get; set; }
        }

        public IEnumerator<EmbeddedAdvertisementResource> GetEnumerator()
        {
            return this.Embedded.Advertisements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Embedded.Advertisements.GetEnumerator();
        }

        public async Task<AdvertisementListResource> NextPageAsync()
        {
            if (Eof)
                throw new NotSupportedException("There are no more results");

            return await this.GetResourceAsync<AdvertisementListResource>("next");
        }

        public bool Eof => !this.Links.ContainsKey("next");
    }
}