using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Support.Constraints;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Nebula_Messenger.Server;

namespace Nebula_Messenger.Source.Messages
{
    public class MessageList : BaseAdapter, View.IOnLongClickListener
    {
        readonly Context context;
        List<Message> messages;
        readonly Action<string, int> longClickAction;

        int selection = -1;
        Action hideLastTime = delegate { };

        public MessageList(Context _context, List<Message> _messages, Action<string, int> _longClickAction)
        {
            context = _context;
            messages = _messages;
            longClickAction = _longClickAction;
        }

        public override int Count => messages.Count;

        public void Add(Message newMessage)
        {
            messages.Add(newMessage);
        }
        public void RemoveAt(int position)
        {
            messages.RemoveAt(position);
        }
        public string GetId(int position)
        {
            return messages[position]._id;
        }
        public string GetLastId()
        {
            return messages.Last()._id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            ConstraintLayout layout = (ConstraintLayout)inflater.Inflate(Resource.Layout.message, parent, false);

            Message currentMessage = messages[position];

            TextView senderView = layout.FindViewById<TextView>(Resource.Id.senderView);

            bool fromCurrentUser = currentMessage.sender == GlobalData.Username;
            if (!fromCurrentUser)
            {
                senderView.Visibility = ViewStates.Visible;
                if (GlobalData.Friends.TryGetValue(currentMessage.sender, out string senderName))
                    senderView.Text = senderName;
                else
                    senderView.Text = currentMessage.sender;
            }

            TextView timeView = layout.FindViewById<TextView>(Resource.Id.timeView);
            timeView.Text = currentMessage.dateTime;

            void hideThisTime()
            {
                timeView.Visibility = ViewStates.Gone;
            }
            if (position == selection)
                timeView.Visibility = ViewStates.Visible;

            TextView bodyView = layout.FindViewById<TextView>(Resource.Id.bodyView);
            bodyView.Text = currentMessage.body;
            bodyView.SetMaxWidth((int)(parent.Width * 0.8));    //Auto percents would be better - Centering positioning and bias
            bool bigEmojis = ShouldShowBigEmojis(currentMessage.body);
            if (bigEmojis)  //use style instead of just size, including device default sizes
                bodyView.TextSize = 40;
            else
                bodyView.TextSize = 18;
            bodyView.SetTag(Resource.Id.messageId, currentMessage._id);
            bodyView.SetTag(Resource.Id.messagePosition, position);
            bodyView.SetOnLongClickListener(this);
            bodyView.Click += delegate
            {
                hideLastTime();
                if (selection == position || timeView.Visibility == ViewStates.Visible)
                {
                    selection = -1;
                    timeView.Visibility = ViewStates.Gone;
                }
                else
                {
                    hideLastTime = hideThisTime;
                    selection = position;
                    timeView.Visibility = ViewStates.Visible;
                }
            };

            ConstraintLayout.LayoutParams bodyLayoutParams = (ConstraintLayout.LayoutParams)bodyView.LayoutParameters;
            ConstraintLayout.LayoutParams senderLayoutParams = (ConstraintLayout.LayoutParams)senderView.LayoutParameters;
            ConstraintLayout.LayoutParams timeLayoutParams = (ConstraintLayout.LayoutParams)timeView.LayoutParameters;

            Drawable messageBubble;
            if (fromCurrentUser)
            {
                messageBubble = context.GetDrawable(Resource.Drawable.messageBubbleR);
                bodyLayoutParams.EndToEnd = ConstraintLayout.LayoutParams.ParentId;

                timeLayoutParams.EndToEnd = Resource.Id.bodyView;
                senderLayoutParams.EndToStart = Resource.Id.timeView;
            }
            else
            {
                messageBubble = context.GetDrawable(Resource.Drawable.messageBubbleL);
                bodyLayoutParams.StartToStart = ConstraintLayout.LayoutParams.ParentId;

                senderLayoutParams.StartToStart = Resource.Id.bodyView;
                timeLayoutParams.StartToEnd = Resource.Id.senderView;
            }
            if (!bigEmojis)
                ViewCompat.SetBackground(bodyView, messageBubble);

            return layout;
        }

        public bool OnLongClick(View v)
        {
            selection = -1;
            hideLastTime();
            longClickAction.Invoke((string)v.GetTag(Resource.Id.messageId), (int)v.GetTag(Resource.Id.messagePosition));
            return true;
        }

        private bool ShouldShowBigEmojis(string text)
        {
            string filterdText = text.Replace(" ", "");

            Regex rgx = new Regex(@"\p{Cs}");

            MatchCollection matches = rgx.Matches(filterdText);
            if (matches.Count == 0 || matches.Count > 3 * 2)
                return false;

            string leftovers = rgx.Replace(filterdText, "");
            if (leftovers.Length != 0)
                return false;

            int numSpaces = text.Count(f => f == ' ');
            if (numSpaces > 3)
                return false;

            return true;
        }

        public override Java.Lang.Object GetItem(int position) { return null; }

        public override long GetItemId(int position) { return position; }
    }
}