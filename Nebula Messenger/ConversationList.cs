using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Nebula_Messenger.Server;

namespace Nebula_Messenger
{
    public class ConversationList : BaseAdapter
    {
        Context context;
        public Dictionary<string, int> ConvIds { get; private set; }
        public List<Conversation> Conversations { get; private set; }

        public ConversationList(List<Conversation> conversations)
        {
            Conversations = conversations;
            ConvIds = new Dictionary<string, int>();
            for (int i = 0; i < Conversations.Count; i++)
                if (Conversations[i].id != null)
                    ConvIds.TryAdd(Conversations[i].id, i);
        }
        public void SetContext(Context _context)
        {
            context = _context;
        }

        public override int Count => Conversations.Count;

        public void MoveToTop(string id)
        {
            if(ConvIds.TryGetValue(id, out int position))
            {
                if (position == 0) return;
                Conversation conversation = Conversations[position];
                Conversations.RemoveAt(position);
                Conversations.Insert(0, conversation);
                for (int i = 0; i <= position; i++)
                    if (Conversations[i].id != null)
                        ConvIds[Conversations[i].id] = i;
            }
        }
        public void SetConvId(string id, int position)
        {
            ConvIds.TryAdd(id, position);
        }
        public void Add(Conversation conversation)
        {
            ConvIds.TryAdd(conversation.id, Conversations.Count);
            Conversations.Add(conversation);
        }
        public void RemoveAt(int position)
        {
            ConvIds.Remove(Conversations[position].id);
            Conversations.RemoveAt(position);
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LinearLayout layout;

            if (convertView == null)
            {
                layout = new LinearLayout(context)
                {
                    Orientation = Orientation.Vertical
                };
            }
            else
            {
                layout = (LinearLayout)convertView;
                layout.RemoveAllViews();
            }

            TextView nameView = new TextView(context)
            {
                Text = Conversations[position].title,
                TextSize = 20
            };
            nameView.SetPadding(50, 20, 50, 20);

            if (!Conversations[position].read)
            {
                nameView.SetTypeface(null, TypefaceStyle.Bold);
                nameView.SetTextColor(Color.Rgb(0, 112, 197));
            }

            layout.AddView(nameView);

            return layout;
        }

        public override Object GetItem(int position) { return null; }

        public override long GetItemId(int position) { return position; }
    }
}