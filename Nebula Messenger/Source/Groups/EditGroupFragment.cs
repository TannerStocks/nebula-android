using Android.OS;
using Android.Support.Constraints;
using Android.Support.V4.App;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Flexbox;
using Nebula_Messenger.Server;
using Nebula_Messenger.Source.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using Message = Nebula_Messenger.Server.Message;

namespace Nebula_Messenger.Source.Groups
{
    public class EditGroupFragment : DialogFragment
    {
        bool newGroup;

        View fragmentLayout;
        EditText friendToAddView;
        FlexboxLayout membersLayout;
        SuggestedGroupMembersList suggestedFriendsList;

        Dictionary<string, string> members = new Dictionary<string, string>();

        public EditGroupFragment(bool _newGroup)
        {
            newGroup = _newGroup;
        }
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            Dialog.Window.SetSoftInputMode(SoftInput.StateVisible);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            fragmentLayout = inflater.Inflate(Resource.Layout.EditGroup, container, false);

            TextView title = fragmentLayout.FindViewById<TextView>(Resource.Id.title);
            if (newGroup)
                title.Text = "Create Group";
            else
                title.Text = "Add Members";

            friendToAddView = fragmentLayout.FindViewById<EditText>(Resource.Id.friendToAddView);
            friendToAddView.TextChanged += UpdateSuggestions;

            membersLayout = fragmentLayout.FindViewById<FlexboxLayout>(Resource.Id.membersLayout);

            ListView suggestedFriendsView = fragmentLayout.FindViewById<ListView>(Resource.Id.suggestedFriendList);
            if (newGroup)
                suggestedFriendsList = new SuggestedGroupMembersList(Activity, null);
            else
            {
                List<string> currentMembers = GlobalData.CurrentConversation.involved.TrimEnd(';').Split(':').ToList();
                suggestedFriendsList = new SuggestedGroupMembersList(Activity, currentMembers);
            }
            suggestedFriendsView.Adapter = suggestedFriendsList;
            suggestedFriendsView.ItemClick += AddFriend;

            if (container == null)
            {
                DisplayMetrics displayMetrics = new DisplayMetrics();
                Activity.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
                suggestedFriendsView.LayoutParameters.Width = (int)(displayMetrics.WidthPixels * 0.86);
                suggestedFriendsView.LayoutParameters.Height = (int)(displayMetrics.HeightPixels * 0.15);
            }

            if (newGroup)
            {
                ConstraintLayout bottomLayout = fragmentLayout.FindViewById<ConstraintLayout>(Resource.Id.bottomLayout);
                bottomLayout.Visibility = ViewStates.Visible;

                ImageButton sendButton = fragmentLayout.FindViewById<ImageButton>(Resource.Id.sendMessageButton);
                sendButton.Click += SendMessage;
            }
            else
            {
                LinearLayout buttonsLayout = fragmentLayout.FindViewById<LinearLayout>(Resource.Id.buttonsLayout);
                buttonsLayout.Visibility = ViewStates.Visible;
                Button addMembersButton = fragmentLayout.FindViewById<Button>(Resource.Id.addMembersButton);
                addMembersButton.Click += AddNewMembers;
                Button cancelButton = fragmentLayout.FindViewById<Button>(Resource.Id.cancelButton);
                cancelButton.Click += delegate
                {
                    Dialog.Window.SetSoftInputMode(SoftInput.StateHidden);
                    Dismiss();
                };
            }

            return fragmentLayout;
        }

        private void UpdateSuggestions(object sender, TextChangedEventArgs e)
        {
            suggestedFriendsList.FilterFriends(e.Text.ToString(), members);
            suggestedFriendsList.NotifyDataSetChanged();
        }
        
        private void AddFriend(object sender, AdapterView.ItemClickEventArgs e)
        {
            Friend friend = suggestedFriendsList.GetFriend(e.Position);
            members.Add(friend.username, friend.name);
            MaterialChipView.Chip chip = new MaterialChipView.Chip(Activity)
            {
                ChipText = friend.name,
                Closable = true
            };
            FlexboxLayout.LayoutParams layoutParams = new FlexboxLayout.LayoutParams(
                    ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            layoutParams.SetMargins(4, 4, 4, 4);
            chip.LayoutParameters = layoutParams;
            chip.Close += delegate
            {
                string removedFriend = friend.name;
                members.Remove(friend.username);
                membersLayout.RemoveView(chip);
                friendToAddView.Text = removedFriend;
                friendToAddView.SetSelection(friendToAddView.Text.Length);
            };
            membersLayout.AddView(chip);
            friendToAddView.Text = string.Empty;
        }

        private void AddNewMembers(object sender, EventArgs e)
        {
            string involved = string.Concat(string.Join(":", members.Keys), ":", GlobalData.CurrentConversation.involved);
            Conversation.EditGroupMembers(involved, process);
            void process(ServerStuff.SuccessResponse successResponse)
            {
                if (successResponse.success)
                    GlobalData.MainActivity.RunOnUiThread(editUi);
                void editUi()
                {
                    GlobalData.CurrentConversation.involved = involved;
                    GlobalData.CurrentConversation.CreateGroupTitle();
                    //update lastMsgRead
                    GlobalData.GroupConvList.MoveToTop(GlobalData.CurrentConversation.id);
                    GlobalData.GroupConvList.NotifyDataSetChanged();
                    Dialog.Window.SetSoftInputMode(SoftInput.StateHidden);
                    Dismiss();
                }
            }
        }
        void SendMessage(object sender, EventArgs e)
        {
            EditText newMessageView = fragmentLayout.FindViewById<EditText>(Resource.Id.newMessageView);

            if (newMessageView.Text == string.Empty) return;

            string dateTime = DateTime.Now.ToString("dd/MM/yyyy 'at' HH:mm:ss");

            string involved = string.Concat(GlobalData.Username, ":", string.Join(":", members.Keys), ";");

            Message message =
                new Message
                {
                    topic = null,
                    sender = GlobalData.Username,
                    body = newMessageView.Text,
                    _id = null,
                    id = null,
                    convId = involved,
                    dateTime = dateTime,
                    isGroupChat = true,
                    read = false
                };

            message.SendAndProcess(process, false);

            void process(ServerStuff.SuccessResponse successResponse)
            {
                if (successResponse.success)
                {
                    successResponse.conv.CreateGroupTitle();
                    successResponse.conv.read = true;
                    Activity.RunOnUiThread(_editUi);
                    void _editUi()
                    {
                        GlobalData.AddGroup(successResponse.conv, true);
                        GlobalData.GroupConvList.MoveToTop(successResponse.conv.id);
                        GlobalData.GroupConvList.NotifyDataSetChanged();
                        Toast.MakeText(Activity, "Conversation Successfully Created", ToastLength.Short).Show();
                    }
                    Activity.StartActivity(typeof(ConversationActivity));
                    Dismiss();
                }
                else
                {
                    Activity.RunOnUiThread(editUi);
                    void editUi()
                    {
                        Toast.MakeText(Activity, "Error Creating Conversation", ToastLength.Short).Show();
                    }
                }
            }
        }
    }
}