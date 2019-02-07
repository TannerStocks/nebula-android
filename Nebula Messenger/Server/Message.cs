using Android.Widget;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace Nebula_Messenger.Server
{
    public class Message
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
        public bool isPool;
        public string groupChat = "HELLO";

        const string routeRoot = "messages";

        const string OnMessageEvent = "message";
        const string OnTypingEvent = "typing";
        const string OnDoneTypingEvent = "nottyping";

        public static Action OnMessageProcess = delegate { };
        public static Action<string, string> OnTypingProcess = delegate { };
        public static Action<string, string> OnDoneTypingProcess = delegate { };

        static bool connected = false;
        public static void ConnectSocket()
        {
            if (connected) return;
            ServerStuff.Socket.Connect();
            ServerStuff.Socket.On(OnMessageEvent, OnMessage);
            ServerStuff.Socket.On(OnTypingEvent, OnTyping);
            ServerStuff.Socket.On(OnDoneTypingEvent, OnDoneTyping);
            connected = true;
        }
        public static void DisconnectSocket()
        {
            ServerStuff.Socket.Off(OnMessageEvent);
            ServerStuff.Socket.Off(OnTypingEvent);
            ServerStuff.Socket.Off(OnDoneTypingEvent);
        }

        class TypingObj
        {
            public string id;
            public string friend;
        }
        public static void EmitTyping()
        {
            TypingObj typingObj = new TypingObj()
            {
                id = GlobalData.CurrentConversation.id,
                friend = GlobalData.Username
            };
            string typingJson = JsonConvert.SerializeObject(typingObj);

            ServerStuff.Socket.Emit("currently-typing", typingJson);
        }
        public static void EmitDoneTyping()
        {
            TypingObj typingObj = new TypingObj()
            {
                id = GlobalData.CurrentConversation.id,
                friend = GlobalData.Username
            };
            string typingJson = JsonConvert.SerializeObject(typingObj);

            ServerStuff.Socket.Emit("done-typing", typingJson);
        }
        static void OnTyping(string jsonBody)
        {
            TypingObj typingObj;
            try
            {
                typingObj = JsonConvert.DeserializeObject<TypingObj>(jsonBody);
            }
            catch (JsonSerializationException)
            {
                GlobalData.MainActivity.RunOnUiThread(EditUi);
                void EditUi()
                {
                    Toast.MakeText(GlobalData.MainActivity, "Error in OnTyping", ToastLength.Short).Show();
                }
                return;
            }
            OnTypingProcess(typingObj.id, typingObj.friend);
        }
        static void OnDoneTyping(string jsonBody)
        {
            TypingObj typingObj;
            try
            {
                typingObj = JsonConvert.DeserializeObject<TypingObj>(jsonBody);
            }
            catch (JsonSerializationException)
            {
                GlobalData.MainActivity.RunOnUiThread(EditUi);
                void EditUi()
                {
                    Toast.MakeText(GlobalData.MainActivity, "Error in OnDoneTyping", ToastLength.Short).Show();
                }
                return;
            }
            OnDoneTypingProcess(typingObj.id, typingObj.friend);
        }

        private static void OnMessage(string jsonBody)
        {
            Message message;
            try
            {
                message = JsonConvert.DeserializeObject<Message>(jsonBody);
            }
            catch (JsonSerializationException)
            {
                GlobalData.MainActivity.RunOnUiThread(EditUi);
                void EditUi()
                {
                    Toast.MakeText(GlobalData.MainActivity, "Error Receiving Message", ToastLength.Short).Show();
                }
                return;
            }

            if (message.id == null) return;

            if (message.isGroupChat)
            {
                if (GlobalData.GroupConvList.ConvIds.TryGetValue(message.id, out int position))
                {
                    GlobalData.MainActivity.RunOnUiThread(editUi);
                    void editUi()
                    {
                        GlobalData.GroupConvList.Conversations[position].SetReadOutOfDate(message._id);
                        GlobalData.GroupConvList.MoveToTop(message.id);
                        GlobalData.GroupConvList.NotifyDataSetChanged();
                    }
                }
            }
            else if(!message.isPool)
            {
                if (GlobalData.FriendConvList.ConvIds.TryGetValue(message.id, out int position))
                {
                    GlobalData.MainActivity.RunOnUiThread(editUi);
                    void editUi()
                    {
                        GlobalData.FriendConvList.Conversations[position].SetReadOutOfDate(message._id);
                        GlobalData.FriendConvList.MoveToTop(message.id);
                        GlobalData.FriendConvList.NotifyDataSetChanged();
                    }
                }
            }

            if (GlobalData.CurrentConversation == null) return;
            
            if (message.id == string.Empty && message.sender == GlobalData.CurrentConversation.topic
                || GlobalData.CurrentConversation.id != null && message.id == GlobalData.CurrentConversation.id)
            {
                if (GlobalData.CurrentConvType == Conversation.Type.Friend
                    || GlobalData.CurrentConvType == Conversation.Type.Group)
                    GlobalData.CurrentConversation.SetReadUpToDate();
                GlobalData.MainActivity.RunOnUiThread(editUi);
                void editUi()
                {
                    GlobalData.CurrentMessageList.Add(message);
                    GlobalData.CurrentMessageList.NotifyDataSetChanged();
                    OnMessageProcess();
                };
            }
        }
        public void SendAndProcess(Action<ServerStuff.SuccessResponse> process, bool sendSocket = true)
        {
            if (sendSocket)
            {
                string messageJson = JsonConvert.SerializeObject(this);
                ServerStuff.Socket.Emit("add-message", messageJson);
            }

            if (GlobalData.CurrentConvType == Conversation.Type.Friend
                || GlobalData.CurrentConvType == Conversation.Type.Group)
                ServerStuff.Request(routeRoot + "/send", this, response);
            else if (GlobalData.CurrentConvType == Conversation.Type.Pool)
                ServerStuff.Request("pools/send", this, response);

            void response(string responseJson)
            {
                ServerStuff.SuccessResponse successObj =
                    JsonConvert.DeserializeObject<ServerStuff.SuccessResponse>(responseJson);
                process(successObj);
            }
        }
        public static void GetAndProcessMessages(Action<List<Message>> process)
        {
            object jsonBody = new
            {
                GlobalData.CurrentConversation.id,
                token = GlobalData.Token
            };

            ServerStuff.Request(routeRoot + "/getmsgswithidtoken", jsonBody, response);

            void response(IRestResponse obj)
            {
                if (string.IsNullOrEmpty(obj.Content)) return;

                Messages messages = JsonConvert.DeserializeObject<Messages>(obj.Content);

                process(messages.msgs);
            }
        }
        class Messages
        {
            public List<Message> msgs;
        }
        public static void DeleteMessageAndProcess(string id, Action<ServerStuff.SuccessResponse> process)
        {
            object jsonBody = new { id };

            ServerStuff.Request(ServerStuff.Messages.DeleteMessage, jsonBody, response);

            void response(IRestResponse obj)
            {
                if (string.IsNullOrEmpty(obj.Content)) return;

                ServerStuff.SuccessResponse successObj =
                    JsonConvert.DeserializeObject<ServerStuff.SuccessResponse>(obj.Content);

                process(successObj);
            }
        }
    }
}