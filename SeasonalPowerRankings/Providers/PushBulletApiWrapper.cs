using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChallongeMatchViewer.Providers
{
    public class PushbulletApiWrapper
    {
        public const string PUSHBULLET_API_URL = "https://api.pushbullet.com/v2/";

        /// <summary>Pushes the message to the specified channel.</summary>
        /// <param name="apikey">The Pushbullet API key, generated from https://www.pushbullet.com/account. </param>
        /// <param name="channeltag">The Pushbullet Channel to send the message.</param>
        /// <param name="title">The title.</param>
        /// <param name="message">The message to send.</param>
        public static bool PushToChannel(string apikey, string channeltag, string title, string message)
        {
            if (string.IsNullOrEmpty(apikey) || string.IsNullOrEmpty(channeltag))
                return false;

            var requestUrl = "pushes";

            var client = new RestClient(PUSHBULLET_API_URL);
            client.Authenticator = new HttpBasicAuthenticator(apikey, string.Empty);

            var request = new RestRequest(requestUrl, Method.POST);
            request.AddParameter("channel_tag", channeltag);
            request.AddParameter("type", "note");
            request.AddParameter("title", title ?? string.Empty);
            request.AddParameter("body", message ?? string.Empty);

            var response = client.Execute(request);

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        /// <summary>Pushes the message to the Pushbullet account specified by the account's email address.</summary>
        /// <param name="apikey">The Pushbullet API key, generated from https://www.pushbullet.com/account. </param>
        /// <param name="email">The email address associated with a Pushbullet account to send the message.</param>
        /// <param name="title">The title.</param>
        /// <param name="message">The message to send.</param>
        public static bool PushToEmail(string apikey, string email, string title, string message)
        {
            if (string.IsNullOrEmpty(apikey) || string.IsNullOrEmpty(email))
                return false;

            var requestUrl = "pushes";

            var client = new RestClient(PUSHBULLET_API_URL);
            client.Authenticator = new HttpBasicAuthenticator(apikey, string.Empty);

            var request = new RestRequest(requestUrl, Method.POST);
            request.AddParameter("email", email);
            request.AddParameter("type", "note");
            request.AddParameter("title", title ?? string.Empty);
            request.AddParameter("body", message ?? string.Empty);

            var response = client.Execute(request);

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}