﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class FireOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;

        private object msg = null;
        private string channelName = "";
        private bool storeInHistory = false;
        private bool httpPost = false;
        private string userMetadata = "";
        private int ttl = -1;

        public FireOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public FireOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary, null)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public FireOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }

        public FireOperation Message(object message)
        {
            this.msg = message;
            return this;
        }

        public FireOperation Channel(string channelName)
        {
            this.channelName = channelName;
            return this;
        }

        //public FireOperation ShouldStore(bool store)
        //{
        //    this.storeInHistory = store;
        //    return this;
        //}

        public FireOperation Meta(string jsonMetadata)
        {
            this.userMetadata = jsonMetadata;
            return this;
        }

        public FireOperation UsePOST(bool post)
        {
            this.httpPost = post;
            return this;
        }

        ///// <summary>
        ///// tttl in hours
        ///// </summary>
        ///// <param name="ttl"></param>
        ///// <returns></returns>
        //public FireOperation Ttl(int ttl)
        //{
        //    this.ttl = ttl;
        //    return this;
        //}

        public void Async(PNCallback<PNPublishResult> callback)
        {
            Fire(this.channelName, this.msg, this.storeInHistory, this.ttl, this.userMetadata, callback);
        }

        private void Fire(string channel, object message, bool storeInHistory, int ttl, string jsonUserMetaData, PNCallback<PNPublishResult> callback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null)
            {
                throw new ArgumentException("Missing Channel or Message");
            }

            if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid publish key");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }

            if (config.EnableDebugForPushPublish)
            {
                if (message is Dictionary<string, object>)
                {
                    Dictionary<string, object> dicMessage = message as Dictionary<string, object>;
                    dicMessage.Add("pn_debug", true);
                    message = dicMessage;
                }
            }

            if (string.IsNullOrEmpty(jsonUserMetaData) || jsonLibrary.IsDictionaryCompatible(jsonUserMetaData))
            {
                jsonUserMetaData = "";
            }

            Dictionary<string, string> urlParam = new Dictionary<string, string>();
            urlParam.Add("norep", "true");

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);
            Uri request = urlBuilder.BuildPublishRequest(channel, message, storeInHistory, ttl, jsonUserMetaData, httpPost, urlParam);

            RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = PNOperationType.PNPublishOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;

            string json = "";

            if (this.httpPost)
            {
                requestState.UsePostMethod = true;
                Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
                messageEnvelope.Add("message", message);
                string postMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
                json = UrlProcessRequest<PNPublishResult>(request, requestState, false, postMessage);
            }
            else
            {
                json = UrlProcessRequest<PNPublishResult>(request, requestState, false);
            }

            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNPublishResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }

}