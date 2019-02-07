using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Nebula_Messenger.Source.Friends
{
    public class AddFriendFragment : DialogFragment
    {
        View fragmentLayout;
        TextInputEditText newFriendView;
        FriendSuggestions suggestionList;
        FriendSuggestions requestList;

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            Dialog.Window.SetSoftInputMode(SoftInput.StateVisible);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            fragmentLayout = inflater.Inflate(Resource.Layout.addFriend_layout, container, false);

            if (container == null)
            {
                DisplayMetrics displayMetrics = new DisplayMetrics();
                Activity.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
                fragmentLayout.SetMinimumWidth((int)(displayMetrics.WidthPixels * 0.86));
            }

            newFriendView = fragmentLayout.FindViewById<TextInputEditText>(Resource.Id.newFriendView);
            newFriendView.TextChanged += UpdateSuggestions;

            ListView suggestionsView = fragmentLayout.FindViewById<ListView>(Resource.Id.suggestionsView);
            suggestionsView.Adapter = suggestionList = new FriendSuggestions(false, respondToRequestSent);

            void respondToRequestSent(bool success)
            {
                if (success)
                {
                    GlobalData.MainActivity.RunOnUiThread(editUi);
                    void editUi()
                    {
                        newFriendView.Text = string.Empty;
                    }
                }
            }

            TextView requestsTitle = fragmentLayout.FindViewById<TextView>(Resource.Id.requestsTitle);
            if (GlobalData.FriendRequests.Count > 0)
                requestsTitle.Visibility = ViewStates.Visible;

            ListView friendRequestsView = fragmentLayout.FindViewById<ListView>(Resource.Id.friendRequestsView);
            friendRequestsView.Adapter = requestList = new FriendSuggestions(true, respondToRequestAccepted);

            void respondToRequestAccepted(bool success)
            {
                //idk
            }

            return fragmentLayout;
        }

        public void UpdateSuggestions(object sender, TextChangedEventArgs e)
        {
            suggestionList.UpdateSuggestions(e.Text.ToString());
        }
    }
}