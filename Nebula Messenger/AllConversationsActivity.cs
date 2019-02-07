using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using System.Collections.Generic;
using Android.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Nebula_Messenger.Server;
using Nebula_Messenger.Source.Pools;
using Message = Nebula_Messenger.Server.Message;

namespace Nebula_Messenger
{
    [Activity(Label = "Conversations", WindowSoftInputMode = SoftInput.AdjustPan,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)] //temporary
    public class AllConversationsActivity : AppCompatActivity
    {
        ViewPagerAdapter adapter;
        ViewPager viewPager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.allConversations_layout);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.ShowOverflowMenu();
            SetSupportActionBar(toolbar);

            FloatingActionButton addButton =
                FindViewById<FloatingActionButton>(Resource.Id.addButton);
            addButton.Enabled = false;
            addButton.Click += delegate
            {
                adapter.GetAt(viewPager.CurrentItem).Add();
            };

            Conversation.GetConvsObj jsonBody = new Conversation.GetConvsObj()
            {
                username = GlobalData.Username,
                token = GlobalData.Token
            };

            Conversation.GetAndProcessConversations(jsonBody, process);

            void process(Conversation.FriendsAndConvs user)
            {
                if(!user.success)
                {
                    RunOnUiThread(editUi);
                    void editUi()
                    {
                        Toast.MakeText(this, user.msg, ToastLength.Short).Show();
                    }
                    SetResult(Result.Ok);
                    Finish();
                    return;
                }

                GlobalData.Init(this, user);

                Message.ConnectSocket();

                CreateTabFragments();
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Message.DisconnectSocket();
        }

        void CreateTabFragments()
        {
            GlobalData.FriendConvList.SetContext(this);
            GlobalData.GroupConvList.SetContext(this);

            List<TitledFragment> fragments = new List<TitledFragment>
                {
                    new ConversationListFragment(Conversation.Type.Friend),
                    new ConversationListFragment(Conversation.Type.Group),
                    new PoolsFragment(this)
                };

            viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
            viewPager.ScrollChange += AnimateFAB;
            viewPager.OffscreenPageLimit = 2;

            TabLayout tabLayout = FindViewById<TabLayout>(Resource.Id.sliding_tabs);
            adapter = new ViewPagerAdapter(SupportFragmentManager, fragments);
            
            RunOnUiThread(editUi);
            void editUi()
            {
                viewPager.Adapter = adapter;
                tabLayout.SetupWithViewPager(viewPager);
                tabLayout.RefreshDrawableState();
                FloatingActionButton addButton =
                    FindViewById<FloatingActionButton>(Resource.Id.addButton);
                addButton.Enabled = true;
            }
        }

        private void AnimateFAB(object sender, View.ScrollChangeEventArgs e)
        {
            float scrollFraction = (float)e.ScrollX / viewPager.Width;

            FloatingActionButton addButton =
                FindViewById<FloatingActionButton>(Resource.Id.addButton);
            addButton.Rotation = -scrollFraction * 90;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main_menu, menu);
            return true;
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_logout)
            {
                SetResult(Result.Ok);
                Finish();
                return true;
            }
            else
                return base.OnOptionsItemSelected(item);
        }
    }
}