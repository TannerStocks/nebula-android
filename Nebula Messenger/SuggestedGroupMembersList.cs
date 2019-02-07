using Android.Content;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Nebula_Messenger.Server;
using System.Collections.Generic;

namespace Nebula_Messenger
{
    internal class SuggestedGroupMembersList: BaseAdapter
    {
        Context context;
        List<Friend> friends;
        List<string> prevMembers;

        public SuggestedGroupMembersList(Context _context, List<string> _prevMembers)
        {
            context = _context;
            friends = new List<Friend>();
            prevMembers = _prevMembers ?? new List<string>();
        }

        public override int Count => friends.Count;

        public void FilterFriends(string query, Dictionary<string, string> ignore)
        {
            friends.Clear();

            if (string.IsNullOrEmpty(query)) return;

            foreach (KeyValuePair<string, string> friend in GlobalData.Friends)
                if (friend.Value.StartsWith(query, System.StringComparison.OrdinalIgnoreCase)
                    && !ignore.ContainsKey(friend.Key) && !prevMembers.Contains(friend.Key))
                    friends.Add(
                        new Friend()
                        {
                            username = friend.Key,
                            name = friend.Value
                        });
        }

        public Friend GetFriend(int position)
        {
            return friends[position];
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LinearLayout layout;

            if (convertView == null)
            {
                layout = new LinearLayout(context)
                {
                    Orientation = Orientation.Horizontal
                };
            }
            else
            {
                layout = (LinearLayout)convertView;
                layout.RemoveAllViews();
            }

            TextView nameView = new TextView(context)
            {
                Text = friends[position].name,
                TextSize = 20,
            };
            nameView.SetPadding(50, 20, 50, 20);

            layout.AddView(nameView);

            return layout;
        }

        public override Object GetItem(int position) { return null; }

        public override long GetItemId(int position) { return position; }
    }
}