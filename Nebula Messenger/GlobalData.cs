using Android.App;
using Android.Gms.Maps.Model;
using Android.Widget;
using Java.Lang;
using Nebula_Messenger.Server;
using Nebula_Messenger.Source.Messages;
using Nebula_Messenger.Source.Pools;
using System.Collections.Generic;
using System.Linq;

namespace Nebula_Messenger
{
    public static class GlobalData
    {
        public static Activity MainActivity;
        public static string Token;

        public static string Name;
        public static string Username;
        public static ConversationList FriendConvList { get; private set; }
        public static ConversationList GroupConvList { get; private set; }
        public static PoolList PoolConvList { get; private set; }

        public static Conversation.Type CurrentConvType { get; private set; }
        public static int CurrentConvIndex { get; private set; }
        public static Conversation CurrentConversation { get; private set; }
        public static MessageList CurrentMessageList;

        public static Dictionary<string, string> Friends;
        public static List<string> FriendRequests = new List<string>();

        public static LatLng Location;

        public static void Init(Activity activity, Conversation.FriendsAndConvs user)
        {
            MainActivity = activity;

            Name = user.name;
            //Username = user.username;

            Friends = new Dictionary<string, string>();
            foreach (Friend friend in user.friends)
                if (!Friends.TryAdd(friend.username, friend.name))
                {
                    MainActivity.RunOnUiThread(EditUi);
                    void EditUi()
                    {
                        Toast.MakeText(MainActivity, "Duplicate Friend - Tell Shelby to fix the server.", ToastLength.Short).Show();
                    }
                }

            List<Conversation> friendConvs = new List<Conversation>();
            List<Conversation> groupConvs = new List<Conversation>();

            for (int i = 0; i < user.convs.Count; i++)
            {
                user.convs[i].read = user.convs[i].IsReadUpToDate();

                int numInvolved = user.convs[i].involved.Count(f => f == ':') + 1;
                if (numInvolved == 2)
                    friendConvs.Add(user.convs[i]);
                else if (numInvolved > 2)
                {
                    user.convs[i].CreateGroupTitle();
                    user.convs[i].topic = user.convs[i].id;
                    groupConvs.Add(user.convs[i]);
                }
                else
                    throw new IllegalArgumentException("conversation.involved attribute invalid format");
            }
            int numFriendConvs = friendConvs.Count;
            foreach (Friend friend in user.friends)
            {
                bool matched = false;
                string involved = string.Concat(Username, ':', friend.username, ';');
                for (int i = 0; i < numFriendConvs; i++)
                {
                    if (friendConvs[i].involved.Contains(friend.username + ':')         //not bug tight
                        || friendConvs[i].involved.Contains(':' + friend.username + ';'))
                    {
                        friendConvs[i].title = friend.name;
                        friendConvs[i].topic = friend.username;
                        friendConvs[i].involved = involved;
                        matched = true;
                    }
                }
                if (!matched)
                {
                    Conversation newConversation = new Conversation()
                    {
                        title = friend.name,
                        topic = friend.username,
                        involved = involved,
                        read = true
                    };
                    friendConvs.Add(newConversation);
                }
            }
            groupConvs.Sort();
            friendConvs.Sort();
            FriendConvList = new ConversationList(friendConvs);
            GroupConvList = new ConversationList(groupConvs);
            PoolConvList = new PoolList();
        }

        public static void ClearData()
        {
            Name = null;
            Username = null;
            FriendConvList = null;
            GroupConvList = null;
            CurrentConvIndex = -1;
            CurrentConvType = Conversation.Type.Invalid;
            CurrentMessageList = null;
        }

        public static void AssignCurrentConversation(Conversation.Type type, int index)
        {
            CurrentConvType = type;
            CurrentConvIndex = index;

            switch (type)
            {
                case Conversation.Type.Friend:
                    CurrentConversation = FriendConvList.Conversations[index];
                    break;
                case Conversation.Type.Group:
                    CurrentConversation = GroupConvList.Conversations[index];
                    break;
                case Conversation.Type.Pool:
                    CurrentConversation = PoolConvList.GetConversationObj(index);
                    break;
            }
        }
        public static void EditCurrentConversation(Conversation conversation)
        {
            switch (CurrentConvType)
            {
                case Conversation.Type.Friend:
                    FriendConvList.Conversations[CurrentConvIndex] = conversation;
                    break;
                case Conversation.Type.Group:
                    GroupConvList.Conversations[CurrentConvIndex] = conversation;
                    break;
                case Conversation.Type.Pool:
                    //PoolConvList.Conversations[currentConvIndex] = conversation;
                    break;
            }
        }
        public static void ClearCurrentConversation()
        {
            CurrentConvType = Conversation.Type.Invalid;
            CurrentConvIndex = -1;
            CurrentConversation = null;
        }

        public static void AddFriend(Friend friend)
        {
            Conversation newConversation = new Conversation()
            {
                title = friend.name,
                topic = friend.username,
                involved = string.Concat(Username, ':', friend.username, ';')
            };
            FriendConvList.Add(newConversation);
        }
        public static void AddGroup(Conversation groupConversation, bool assignAsCurrentConv = false)
        {
            GroupConvList.Add(groupConversation);
            if (assignAsCurrentConv)
                AssignCurrentConversation(Conversation.Type.Group, GroupConvList.Count - 1);
        }
        public static string GetConversationId(Conversation.Type convType, int index)
        {
            switch (convType)
            {
                case Conversation.Type.Friend:
                    return FriendConvList.Conversations[index].id;
                case Conversation.Type.Group:
                    return GroupConvList.Conversations[index].id;
                case Conversation.Type.Pool:
                    return PoolConvList.GetConversationObj(index).id;
                default:
                    return null;
            }
        }
    }
}