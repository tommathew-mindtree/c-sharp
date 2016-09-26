﻿using System;
using PubnubApi.Interface;
using System.Collections.Generic;

namespace PubnubApi.EndPoint
{
    public class ListAllChannelGroupOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;

        public ListAllChannelGroupOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public ListAllChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public void Async(PNCallback<PNChannelGroupsListAllResult> callback)
        {
            GetAllChannelGroup(callback);
        }

        internal void GetAllChannelGroup(PNCallback<PNChannelGroupsListAllResult> callback)
        {
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary);

            Uri request = urlBuilder.BuildGetAllChannelGroupRequest();

            RequestState<PNChannelGroupsListAllResult> requestState = new RequestState<PNChannelGroupsListAllResult>();
            requestState.ResponseType = PNOperationType.ChannelGroupGet;
            requestState.Callback = callback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<PNChannelGroupsListAllResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNChannelGroupsListAllResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
