﻿using Marvin.JsonPatch;
using Marvin.JsonPatch.Operations;
using SEEK.AdPostingApi.Client.Hal;

namespace SEEK.AdPostingApi.Client.Models
{
    [MediaType("application/vnd.seek.advertisement-patch+json;version=1")]
    public class ExpireAdvertisementJsonPatch : JsonPatchDocument<AdvertisementPatch>
    {
        public ExpireAdvertisementJsonPatch()
        {
            this.Operations.Add(
                new Operation<AdvertisementPatch>(
                    OperationType.Replace.ToString().ToLower(), nameof(AdvertisementPatch.State).ToLower(), null, AdvertisementState.Expired.ToString()));
        }
    }
}