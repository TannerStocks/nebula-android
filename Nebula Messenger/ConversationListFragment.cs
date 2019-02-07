using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Nebula_Messenger.Server;
using Nebula_Messenger.Source.Friends;
using Nebula_Messenger.Source.Groups;
using Nebula_Messenger.Source.Messages;
using Newtonsoft.Json;
using RestSharp;
using Message = Nebula_Messenger.Server.Message;

namespace Nebula_Messenger
{
    public class ConversationListFragment : TitledFragment
    {
        readonly Conversation.Type convType;

        public ConversationListFragment() : base() { }

        public ConversationListFragment(Conversation.Type _convType):base()
        {
            convType = _convType;
            
            switch (convType)
            {
                case Conversation.Type.Friend:
                    Title = "Friends";
                    break;
                case Conversation.Type.Group:
                    Title = "Groups";
                    break;
            }
        }
        
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            ListView conversationsView = new ListView(Activity);
            conversationsView.ItemClick += OpenConversation;
            conversationsView.ItemLongClick += OpenConversationMenu;

            if (convType == Conversation.Type.Friend)
                conversationsView.Adapter = GlobalData.FriendConvList;
            else if (convType == Conversation.Type.Group)
                conversationsView.Adapter = GlobalData.GroupConvList;

            return conversationsView;
        }

        public override void Add()
        {
            switch (convType)
            {
                case Conversation.Type.Friend:
                    AddFriendFragment addFriendFragment = new AddFriendFragment();
                    addFriendFragment.Show(Activity.SupportFragmentManager, addFriendFragment.Tag);
                    break;
                case Conversation.Type.Group:
                    EditGroupFragment editGroupFragment = new EditGroupFragment(true);
                    editGroupFragment.Show(Activity.SupportFragmentManager, editGroupFragment.Tag);
                    break;
            }
        }

        private void OpenConversationMenu(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            builder
                .SetTitle("Delete Conversation?")
                .SetPositiveButton("Delete", DeleteConversation)
                .SetNegativeButton("Cancel", delegate { });

            if (convType == Conversation.Type.Friend)
                builder.SetMessage("You will still be friends");

            builder
                .Create()
                .Show();

            void DeleteConversation(object _sender, DialogClickEventArgs _e)
            {
                object jsonBody = new { id = GlobalData.GetConversationId(convType, e.Position) };

                ServerStuff.Request(ServerStuff.Conversations.DelteConversation, jsonBody, response);

                void response(IRestResponse obj)
                {
                    if (string.IsNullOrEmpty(obj.Content)) return;

                    ServerStuff.SuccessResponse successObj =
                        JsonConvert.DeserializeObject<ServerStuff.SuccessResponse>(obj.Content);

                    Activity.RunOnUiThread(editUi);
                    void editUi()
                    {
                        Toast.MakeText(Activity, successObj.msg, ToastLength.Short).Show();
                    }

                    if (successObj.success)
                    {
                        if (convType == Conversation.Type.Group)
                            Activity.RunOnUiThread(_editUi);
                        void _editUi()
                        {
                            GlobalData.GroupConvList.RemoveAt(e.Position);
                            GlobalData.GroupConvList.NotifyDataSetChanged();
                        }
                    }
                }
            }
        }

        private void OpenConversation(object sender, AdapterView.ItemClickEventArgs e)
        {
            GlobalData.AssignCurrentConversation(convType, e.Position);
            StartActivityForResult(new Intent(Activity, typeof(ConversationActivity)), 0);
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == (int)Result.Ok)
            {
                Message.EmitDoneTyping();

                object jsonBody = new
                {
                    username = GlobalData.Username,
                    GlobalData.CurrentConversation.id,
                    msgid = data.GetStringExtra("lastMessageId")
                };

                ServerStuff.Request(ServerStuff.Conversations.UpdateLastRead, jsonBody, response);
                void response(string responseJson) { }

                GlobalData.CurrentConversation.SetReadUpToDate();

                switch (convType)
                {
                    case Conversation.Type.Friend:
                        GlobalData.FriendConvList.NotifyDataSetChanged();
                        break;
                    case Conversation.Type.Group:
                        GlobalData.GroupConvList.NotifyDataSetChanged();
                        break;
                }
            }

            GlobalData.ClearCurrentConversation();
        }
    }
}