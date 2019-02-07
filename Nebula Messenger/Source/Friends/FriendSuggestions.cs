using Android.Content;
using Android.Support.Constraints;
using Android.Views;
using Android.Widget;
using Nebula_Messenger.Server;
using System;
using System.Collections.Generic;

namespace Nebula_Messenger.Source.Friends
{
    public class FriendSuggestions : BaseAdapter
    {
        bool friendRequests;
        List<string> friends;
        Action<bool> respond;

        public FriendSuggestions(bool _friendRequests, Action<bool> _respond)
        {
            friendRequests = _friendRequests;
            respond = _respond;

            if (friendRequests)
                friends = GlobalData.FriendRequests;
            else
                friends = new List<string>();
        }
        public override int Count => friends.Count;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LayoutInflater inflater = (LayoutInflater)GlobalData.MainActivity.GetSystemService(Context.LayoutInflaterService);
            ConstraintLayout layout = (ConstraintLayout)inflater.Inflate(Resource.Layout.FriendSuggestion, parent, false);

            TextView friendView = layout.FindViewById<TextView>(Resource.Id.friendView);
            friendView.Text = friends[position];

            ImageButton addFriendButton = layout.FindViewById<ImageButton>(Resource.Id.addFriendButton);
            if (friendRequests)
                addFriendButton.Click += delegate
                {
                    AcceptRequest(friends[position]);
                };
            else
                addFriendButton.Click += delegate
                {
                    SendRequest(friends[position]);
                };

            return layout;
        }

        void SendRequest(string friend)
        {
            Friend.SendRequestAndProcess(friend, process);
            void process(ServerStuff.SuccessResponse successResponse)
            {
                if (successResponse.success)
                {
                    //stop suggesting this user
                }
                else
                {
                    GlobalData.MainActivity.RunOnUiThread(editUi);
                    void editUi()
                    {
                        Toast.MakeText(GlobalData.MainActivity, successResponse.msg, ToastLength.Short).Show();
                    }
                }
                respond(successResponse.success);
            }
        }
        void AcceptRequest(string friend)
        {
            Friend.AcceptRequestAndProcess(friend, process);

            void process(ServerStuff.SuccessResponse successResponse)
            {
                if (successResponse.success)
                {
                    Friend newFriend = new Friend()
                    {
                        username = friend,
                        name = successResponse.friend
                    };
                    GlobalData.MainActivity.RunOnUiThread(editUi);
                    void editUi()
                    {
                        GlobalData.FriendRequests.Remove(friend);
                        NotifyDataSetChanged();

                        GlobalData.AddFriend(newFriend);
                        GlobalData.FriendConvList.NotifyDataSetChanged();
                    }
                }
                else
                {
                    GlobalData.MainActivity.RunOnUiThread(editUi);
                    void editUi()
                    {
                        Toast.MakeText(GlobalData.MainActivity, successResponse.msg, ToastLength.Short).Show();
                    }
                }
                respond(successResponse.success);
            }
        }
        public void UpdateSuggestions(string searchQuery)
        {
            if(string.IsNullOrEmpty(searchQuery))
            {
                GlobalData.MainActivity.RunOnUiThread(editUi);
                void editUi()
                {
                    friends = new List<string>();
                    NotifyDataSetChanged();
                }
                return;
            }

            Friend.SearchUsersAndProcess(searchQuery, process);

            void process(List<string> contains)
            {
                List<string> matches = new List<string>();
                foreach (string friend in contains)
                    if (friend.StartsWith(searchQuery, System.StringComparison.OrdinalIgnoreCase)
                        && !GlobalData.Friends.ContainsKey(friend) && friend != GlobalData.Username)
                        matches.Add(friend);
                GlobalData.MainActivity.RunOnUiThread(editUi);
                void editUi()
                {
                    friends = matches;
                    NotifyDataSetChanged();
                }
            }
        }

        public override Java.Lang.Object GetItem(int position) { return null; }

        public override long GetItemId(int position) { return position; }
    }
}