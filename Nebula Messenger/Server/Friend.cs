
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace Nebula_Messenger.Server
{
    public class Friend
    {
        public string name;
        public string username;

        public static string routeRoot = "friends";

        public override string ToString()
        {
            return username;
        }
        struct Suggestions
        {
            public List<string> friends;
        }
        public static void SendRequestAndProcess(string friend, Action<ServerStuff.SuccessResponse> process)
        {
            object jsonBody = new
            {
                username = GlobalData.Username,
                token = GlobalData.Token,
                friend
            };
            ServerStuff.Request(routeRoot + "/sendrequest", jsonBody, response);

            void response(string responseJson)
            {
                ServerStuff.SuccessResponse successResponse =
                    JsonConvert.DeserializeObject<ServerStuff.SuccessResponse>(responseJson);
                process(successResponse);
            }
        }
        public static void AcceptRequestAndProcess(string friend, Action<ServerStuff.SuccessResponse> process)
        {
            object jsonBody = new
            {
                user = GlobalData.Username,
                token = GlobalData.Token,
                friend
            };
            ServerStuff.Request(routeRoot + "/addfriend", jsonBody, response);

            void response(string responseJson)
            {//is friend the name?
                ServerStuff.SuccessResponse successResponse =
                    JsonConvert.DeserializeObject<ServerStuff.SuccessResponse>(responseJson);
                process(successResponse);
            }
        }

        public static void SearchUsersAndProcess(string searchQuery, Action<List<string>> process)
        {
            object jsonBody = new { searchQuery };
            ServerStuff.Request(routeRoot + "/searchfriends", jsonBody, request);

            void request(IRestResponse obj)
            {
                if (string.IsNullOrEmpty(obj.Content)) return;

                Suggestions suggestions = JsonConvert.DeserializeObject<Suggestions>(obj.Content);

                process(suggestions.friends);
            }
        }
    }
}