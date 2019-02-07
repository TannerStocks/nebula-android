using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace Nebula_Messenger.Server
{
    public class Conversation : IComparable
    {
        public enum Type
        {
            Friend,
            Group,
            Pool,
            Invalid
        }

        public string title;
        public string topic;
        public string involved;
        public string id;
        public string lastMessage;
        public string latestMessageTime;
        public IDictionary<string, string> lastMsgRead;
        public bool read;

        public string numMessages;

        const string routeRoot = "conversations";

        public class GetConvsObj
        {
            public string username;
            public string token;
        }
        public class FriendsAndConvs
        {
            public bool success;
            public string msg;

            public string name;
            public string username;

            public List<Friend> friends;
            public List<Conversation> convs;
        }

        public static void EditGroupMembers(string involved, Action<ServerStuff.SuccessResponse> process)
        {
            object jsonBody = new
            {
                GlobalData.CurrentConversation.id,
                involved,
                token = GlobalData.Token
            };
            ServerStuff.Request(routeRoot + "/changegroupmembers", jsonBody, response);

            void response(string responseJson)
            {
                ServerStuff.SuccessResponse successResponse =
                    JsonConvert.DeserializeObject<ServerStuff.SuccessResponse>(responseJson);
                process(successResponse);
            }
        }

        public static void GetAndProcessConversations(GetConvsObj getConvsObj, Action<FriendsAndConvs> process)
        {
            ServerStuff.Request("users/getfriendsandconvstoken", getConvsObj, response);

            void response(IRestResponse obj)
            {
                if (string.IsNullOrEmpty(obj.Content)) return;
                FriendsAndConvs user = JsonConvert.DeserializeObject<FriendsAndConvs>(obj.Content);
                process(user);
            }
        }

        public bool IsReadUpToDate(string username)
        {
            return lastMsgRead.TryGetValue(username, out string lastRead) && lastRead == lastMessage;
        }
        public bool IsReadUpToDate()
        {
            return IsReadUpToDate(GlobalData.Username);
        }
        public void SetReadUpToDate()
        {
            if (!lastMsgRead.ContainsKey(GlobalData.Username)) return;
            
            lastMsgRead[GlobalData.Username] = lastMessage;
            read = true;
        }
        public void SetReadOutOfDate(string _lastMessage)
        {
            lastMessage = _lastMessage;
            read = false;
        }
        public void CreateGroupTitle()
        {
            string newTitle = string.Empty;
            foreach (string member in involved.TrimEnd(';').Split(':'))
            {
                string memberTitle;
                if (GlobalData.Username == member)
                    memberTitle = GlobalData.Name;
                else if (GlobalData.Friends.TryGetValue(member, out string memberName))
                    memberTitle = memberName;
                else memberTitle = member;
                newTitle += memberTitle + ", ";
            }
            title = newTitle.TrimEnd(new char[] { ' ', ',' });
        }

        public int CompareTo(object obj)
        {
            return read.CompareTo(((Conversation)obj).read);
        }
    }
}
