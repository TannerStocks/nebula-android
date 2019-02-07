using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Text;
using Android.Widget;
using Nebula_Messenger.Server;
using Nebula_Messenger.Source.Groups;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Message = Nebula_Messenger.Server.Message;

namespace Nebula_Messenger.Source.Messages
{
    [Activity(Label = "ConversationActivity")]
    public class ConversationActivity : AppCompatActivity
    {
        bool typing = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window.SetSoftInputMode(Android.Views.SoftInput.StateHidden);
            SetContentView(Resource.Layout.conversation_layout);

            Button addMembersButton = FindViewById<Button>(Resource.Id.addMembersButton);
            addMembersButton.Click +=
                delegate
                {
                    EditGroupFragment editGroupFragment = new EditGroupFragment(false);
                    editGroupFragment.Show(SupportFragmentManager, editGroupFragment.Tag);
                };
            if (GlobalData.CurrentConvType == Conversation.Type.Group)
                addMembersButton.Visibility = Android.Views.ViewStates.Visible;

            ListView messagesView = FindViewById<ListView>(Resource.Id.messageListView);
            messagesView.TranscriptMode = TranscriptMode.Normal;
            messagesView.StackFromBottom = true;

            EditText newMessageView = FindViewById<EditText>(Resource.Id.newMessageView);
            if (GlobalData.CurrentConvType == Conversation.Type.Friend)
                newMessageView.TextChanged += UpdateTyping;

            ImageButton sendButton = FindViewById<ImageButton>(Resource.Id.sendMessageButton);
            sendButton.Click += SendMessage;

            if (GlobalData.CurrentConversation.id != null)
                GetMessages();
            else
                ShowNoMessages();

            Message.OnMessageProcess = UpdateResult;
            if (GlobalData.CurrentConvType == Conversation.Type.Friend)
            {
                Message.OnTypingProcess = OnTyping;
                Message.OnDoneTypingProcess = OnDoneTyping;
            }
            
            NotificationManager manager = NotificationManager.FromContext(this);
            //manager.Cancel(UserData.CurrentConversation.id);
        }

        private void UpdateTyping(object sender, TextChangedEventArgs e)
        {
            if (GlobalData.CurrentConversation.id == null) return;

            string text = e.Text.ToString();
            if(!typing && text.Length > 0)
            {
                typing = true;
                Message.EmitTyping();
            }
            else if (typing && text.Length == 0)
            {
                typing = false;
                Message.EmitDoneTyping();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Message.OnMessageProcess = delegate { };
            Message.OnTypingProcess = delegate { };
            Message.OnDoneTypingProcess = delegate { };
        }

        void OnTyping(string convId, string friend)
        {
            if (GlobalData.CurrentConversation.id == null
                || GlobalData.CurrentConversation.id != convId
                || friend == GlobalData.Username)
                return;

            RunOnUiThread(editUi);
            void editUi()
            {
                TextView currentlyTypingView = FindViewById<TextView>(Resource.Id.currentlyTypingView);
                if (GlobalData.Friends.TryGetValue(friend, out string name))
                {
                    currentlyTypingView.Text = string.Concat(name, " is typing...");
                }
                else
                {
                    currentlyTypingView.Text = string.Concat(friend, " is typing...");
                }
                currentlyTypingView.Visibility = Android.Views.ViewStates.Visible;
            }
        }

        void OnDoneTyping(string convId, string friend)
        {
            if (GlobalData.CurrentConversation == null || GlobalData.CurrentConversation.id != convId)
                return;

            RunOnUiThread(editUi);
            void editUi()
            {
                TextView currentlyTypingView = FindViewById<TextView>(Resource.Id.currentlyTypingView);
                currentlyTypingView.Visibility = Android.Views.ViewStates.Gone;
            }
        }

        void SetReadUpToDate()
        {
            object jsonBody = new
            {
                username = GlobalData.Username,
                GlobalData.CurrentConversation.id
            };

            ServerStuff.Request(ServerStuff.Conversations.UpdateLastRead, jsonBody, response);
            void response(string responseJson) { }

            GlobalData.CurrentConversation.SetReadUpToDate(); //On success?

            UpdateResult();
        }
        void UpdateResult()
        {
            Intent intent = new Intent();
            intent.PutExtra("lastMessageId", GlobalData.CurrentMessageList.GetLastId());
            SetResult(Result.Ok, intent);
        }

        void DeleteDialog(string messageId, int position)
        {
            new AlertDialog.Builder(this)
                .SetTitle("Delete Messege?")
                .SetPositiveButton("Delete", DeleteMessage)
                .SetNegativeButton("Cancel", delegate { })
                .Create()
                .Show();

            void DeleteMessage(object _sender, DialogClickEventArgs _e)
            {
                Message.DeleteMessageAndProcess(messageId, process);
            }
            void process(ServerStuff.SuccessResponse successObj)
            {
                RunOnUiThread(editUi);
                void editUi()
                {
                    Toast.MakeText(this, successObj.msg, ToastLength.Short).Show();
                }

                if (successObj.success)
                {
                    RunOnUiThread(editUi2);
                    void editUi2()
                    {
                        GlobalData.CurrentMessageList.RemoveAt(position);
                        GlobalData.CurrentMessageList.NotifyDataSetChanged();
                    }
                }
            }
        }

        void GetMessages()
        {
            Message.GetAndProcessMessages(process);

            void process(List<Message> messages)
            {
                GlobalData.CurrentMessageList = new MessageList(this, messages, DeleteDialog);

                SetReadUpToDate();

                ListView messagesView = FindViewById<ListView>(Resource.Id.messageListView);
                RunOnUiThread(EditUi);
                void EditUi()
                {
                    messagesView.Adapter = GlobalData.CurrentMessageList;
                }
            }
        }
        void ShowNoMessages()
        {
            GlobalData.CurrentMessageList = new MessageList(this, new List<Message>(), DeleteDialog);
            ListView messagesView = FindViewById<ListView>(Resource.Id.messageListView);
            RunOnUiThread(EditUi);
            void EditUi()
            {
                messagesView.Adapter = GlobalData.CurrentMessageList;
            }
        }

        private void SendMessage(object sender, EventArgs e)
        {
            EditText newMessageView = FindViewById<EditText>(Resource.Id.newMessageView);

            if (newMessageView.Text == string.Empty) return;

            string dateTime = DateTime.Now.ToString("dd/MM/yyyy 'at' HH:mm:ss");

            Message messageObj =
                new Message()
                {
                    topic = GlobalData.CurrentConversation.topic,
                    sender = GlobalData.Username,
                    body = newMessageView.Text,
                    _id = null,
                    id = GlobalData.CurrentConversation.id,
                    convId = GlobalData.CurrentConversation.involved,
                    dateTime = dateTime,
                    isGroupChat = GlobalData.CurrentConvType == Conversation.Type.Group,
                    read = false
                };

            messageObj.SendAndProcess(process);

            void process(ServerStuff.SuccessResponse successResponse)
            {
                if (successResponse.success)
                {
                    if (GlobalData.CurrentConversation.id == null)
                    {
                        GlobalData.CurrentConversation.id = successResponse.id;
                        if (GlobalData.CurrentConvType == Conversation.Type.Friend)
                            GlobalData.FriendConvList.SetConvId(
                                GlobalData.CurrentConversation.id, GlobalData.CurrentConvIndex);
                    }
                    RunOnUiThread(editUi1);
                }
                else
                    RunOnUiThread(editUi2);
                void editUi1()
                {
                    newMessageView.Text = string.Empty;
                }
                void editUi2()
                {
                    Toast.MakeText(this, successResponse.msg, ToastLength.Short).Show();
                }
            }
        }
    }
}