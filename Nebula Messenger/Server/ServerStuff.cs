using Firebase.Messaging;
using RestSharp;
using SocketIO.Client;
using System;
using System.Collections.Generic;

namespace Nebula_Messenger.Server
{
    public static class ServerStuff
    {
        const string Url = "http://159.89.152.215:3000/";
        const string masterTopic = "master";
        static RestClient client;
        static SocketIO.Client.Socket socket;

        static ServerStuff()
        {
            client = new RestClient(Url);
            socket = IO.Socket(Url);
            SubscribeTo(masterTopic);
        }

        public static void SubscribeTo(string topic)
        {
            FirebaseMessaging.Instance.SubscribeToTopic(topic);
        }
        public static void SubscribeToUsername()
        {
            if (!string.IsNullOrEmpty(GlobalData.Username))
                FirebaseMessaging.Instance.SubscribeToTopic(GlobalData.Username);
        }
        public static void UnsubscribeFrom(string topic)
        {
            FirebaseMessaging.Instance.UnsubscribeFromTopic(topic);
        }
        public static void UnsubscribeFromUsername()
        {
            if (!string.IsNullOrEmpty(GlobalData.Username))
                FirebaseMessaging.Instance.UnsubscribeFromTopic(GlobalData.Username);
        }
        public static void Unsubscribe()
        {
            FirebaseMessaging.Instance.UnsubscribeFromTopic(masterTopic);
            if (!string.IsNullOrEmpty(GlobalData.Username))
                FirebaseMessaging.Instance.UnsubscribeFromTopic(GlobalData.Username);
        }

        public static void Request(string route, object jsonBody,
            Action<IRestResponse> response, Method method = Method.POST)
        {
            RestRequest request = new RestRequest(route, method);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddJsonBody(jsonBody);

            client.ExecuteAsync(request, response);
        }
        public static void Request(string route, object jsonBody,
            Action<string> response, Method method = Method.POST)
        {
            RestRequest request = new RestRequest(route, method);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddJsonBody(jsonBody);

            client.ExecuteAsync(request, restResponse);
            void restResponse(IRestResponse obj)
            {
                if (!string.IsNullOrEmpty(obj.Content))
                    response(obj.Content);
            }
        }

        public static class Socket
        {
            public static void On(string eventName, Action<string> messageListener)
            {   
                socket.On(eventName, listener);
                void listener(object[] obj)
                {
                    messageListener(obj[0].ToString());
                }
            }
            public static void Off(string eventName)
            {
                socket.Off(eventName);
            }
            public static void Emit(string eventName, string message)
            {
                socket.Emit(eventName, message);
            }
            public static void Disconnect()
            {
                socket.Disconnect();
            }
            public static void Connect()
            {
                if (socket.IsConnected) return;
                socket.Connect();
                Emit("user_connected", GlobalData.Username);
            }
        }

        public struct SuccessResponse
        {
            public bool success;
            public string msg;
            public string id;
            public string token;
            public string friend;
            public Conversation conv;
        }

        public static class Users
        {
            const string root = "users";
            public const string Authenticate = root + "/authenticate";
            public const string Register = root + "/register";
            public const string GetUser = root + "/getuser";
            public const string GetFriendsAndConvs = root + "/getfriendsandconvstoken";
            public const string RefreshToken = root + "/refreshtoken";
            public struct Obj
            {
                public bool success;
            }
        }
        public static class Friends
        {
            const string root = "friends";
            public const string GetFriend = root + "/getfriend";
            public const string GetFriends = root + "/getfriends";
            public const string AddFriend = root + "/addfriend";
            public const string SearchFriends = root + "/searchfriends";
            public struct Suggestions
            {
                public string[] friends;
            }
            public struct Obj
            {
                public struct Friend
                {
                    public string name;
                    public string username;
                    public string lastMessage;
                }
                public Friend[] friends;
            }
            public struct New
            {
                public bool success;
                public string msg;
                public string id;
                public string friend;
            }
        }/*
        public static class Groups
        {
            const string root = "groups";
            public const string GetGroup = root + "/getgroup";
            public const string GetGroups = root + "/getgroups";
            public const string AddGroup = root + "/addgroup";
            public struct Obj
            {
                public struct Group
                {
                    public string involved;
                    public string id;
                }
                public Group[] groups;
            }
        }*/
        public static class Conversations
        {
            const string root = "conversations";
            public const string GetConversation = root + "/getconv";
            public const string GetConversations = root + "/getconvs";
            public const string AddConversation = root + "/addconv";
            public const string DelteConversation = root + "/deleteconv";
            public const string UpdateConversation = root + "/updateconv";
            public const string UpdateLastRead = root + "/updatelastread";
            public struct Obj
            {
                public struct Conversation
                {
                    public string involved;
                    public string id;
                }
                public Conversation[] conv;
            }
        }
        public static class Messages
        {
            const string root = "messages";
            public const string Send = root + "/send";
            public const string GetMessages = root + "/getmsgswithidtoken";
            public const string DeleteMessage = root + "/deletemsg";
            public struct Obj
            {
                public struct Message
                {
                    public string topic;
                    public string sender;
                    public string body;
                    public string _id;      //msgid
                    public string id;       //convid
                    public string convId;   //involved
                    public string dateTime;
                    public bool isGroupChat;
                    public bool read;       //dep
                }
                public List<Message> msgs;
            }
        }
    }
}