﻿using System;
using System.Net.Http;
using SEEK.AdPostingApi.Client.Hal;
using SEEK.AdPostingApi.Client.Models;
using System.Threading.Tasks;

namespace SEEK.AdPostingApi.Client.Resources
{
    [MediaType("application/vnd.seek.advertisement+json")]
    public class AdvertisementResource : HalResource
    {
        public Advertisement AdvertisementProperties{get; set;}

        public string Status { get; set; }

        public async Task SaveAsync()
        {
            await this.PutResourceAsync("self", this);
        }

    }
}