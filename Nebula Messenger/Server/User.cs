using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace Nebula_Messenger.Server
{
    public class User
    {
        public string username;
        public string name;
        public string token;

        public bool success;
        public string msg;

        public List<string> friends;
        public List<string> requestedFriends;


        const string routeRoot = "users";

        public class Credentials
        {
            public string username;
            public string password;
            public string newToken;
        }
        class AuthenticationReponse
        {
            public bool success;
            public string msg;
            public string token;
            public User user;
        }

        public static void AuthenticateAndProcess(Credentials credentials, Action<User> process)
        {
            ServerStuff.Request(routeRoot + "/authenticate", credentials, response);

            void response(IRestResponse obj)
            {
                if (string.IsNullOrEmpty(obj.Content)) return;

                AuthenticationReponse authenticationReponse = 
                    JsonConvert.DeserializeObject<AuthenticationReponse>(obj.Content);

                User user = authenticationReponse.user ?? new User();
                user.success = authenticationReponse.success;
                user.msg = authenticationReponse.msg;
                user.token = authenticationReponse.token;

                process(user);
            }
        }
    }
}