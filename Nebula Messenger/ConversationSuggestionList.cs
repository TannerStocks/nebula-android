using Android.Content;
using Android.Views;
using Android.Widget;

namespace Nebula_Messenger
{
    class ConversationSuggestionList : BaseAdapter
    {
        Context context;
        string[] suggestions;

        public ConversationSuggestionList(Context _context)
        {
            context = _context;
            suggestions = new string[0];
        }

        public void SetSuggestions(string[] _suggestions)
        {
            suggestions = _suggestions;
        }

        public override int Count => suggestions.Length;

        public override Java.Lang.Object GetItem(int position)
        {
            return suggestions[position];
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            TextView suggestionView;

            if (convertView == null)
            {
                suggestionView = new TextView(context);
                suggestionView.TextSize = 20;
                suggestionView.SetPadding(50, 20, 0, 20);
            }
            else
                suggestionView = (TextView)convertView;

            suggestionView.Text = suggestions[position];

            return suggestionView;
        }

    }
}