using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Widget;
using Nebula_Messenger.Server;
using Nebula_Messenger.Source.Messages;
using Message = Nebula_Messenger.Server.Message;

namespace Nebula_Messenger.Source.Pools
{
    [Activity(Label = "ConversationActivity")]
    public class PoolActivity : Activity
    {
        const string messageStr = "message";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window.SetSoftInputMode(Android.Views.SoftInput.StateHidden);
            SetContentView(Resource.Layout.conversation_layout);

            ListView messagesView = FindViewById<ListView>(Resource.Id.messageListView);
            messagesView.TranscriptMode = TranscriptMode.Normal;
            messagesView.StackFromBottom = true;
            
            ImageButton sendButton = FindViewById<ImageButton>(Resource.Id.sendMessageButton);
            sendButton.Click += SendMessage;

            GetMessages();
        }

        void GetMessages()
        {
            Message.GetAndProcessMessages(process);

            void process(List<Message> messages)
            {
                GlobalData.CurrentMessageList = new MessageList(this, messages, delegate { });

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
            GlobalData.CurrentMessageList = new MessageList(this, new List<Message>(), delegate { });

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

            if (string.IsNullOrEmpty(newMessageView.Text)) return;

            string dateTime = DateTime.Now.ToString("dd/MM/yyyy 'at' HH:mm:ss");

            Message message = new Message
            {
                sender = GlobalData.Username,
                body = newMessageView.Text,
                id = GlobalData.CurrentConversation.id,
                dateTime = dateTime,
                isPool = true,
                topic = "POOL"
            };

            message.SendAndProcess(process);
            
            void process(ServerStuff.SuccessResponse successResponse)
            {
                if (successResponse.success)
                {
                    if (GlobalData.CurrentConversation.id == null)
                        GlobalData.CurrentConversation.id = successResponse.id;
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