using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Nebula_Messenger.Server;
using Firebase.Iid;

namespace Nebula_Messenger
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class LoginActivity : Activity
    {
        bool authenticating = false;
        bool created = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ISharedPreferences preferences = GetPreferences(FileCreationMode.Private);
            string username = preferences.GetString("username", string.Empty);
            string password = preferences.GetString("password", string.Empty);
            string conversationId = Intent.GetStringExtra("id");
            /*
            if (conversationId != null)
                if (username != string.Empty)
                    StartNextActivity(username, 0);
                else
                    Finish();
            else 
            */if (username != string.Empty && password != string.Empty)
                Authenticate(username, password);
            else
                SetUpScreen();
        }
        void SetUpScreen()
        {
            SetContentView(Resource.Layout.login_layout);

            EditText passwordView = FindViewById<EditText>(Resource.Id.passwordView);
            passwordView.EditorAction += AuthenticateInput;

            Button loginButton = FindViewById<Button>(Resource.Id.loginButton);
            loginButton.Click += AuthenticateInput;
            loginButton.Clickable = true;

            TextView successMsgView = FindViewById<TextView>(Resource.Id.successMsgView);
            successMsgView.Text = string.Empty;
            successMsgView.Visibility = Android.Views.ViewStates.Gone;

            Button signUpButton = FindViewById<Button>(Resource.Id.signUpButton);
            signUpButton.Click += delegate
            {
                StartActivityForResult(typeof(RegisterActivity), 2);
            };

            created = true;
        }
        void AuthenticateInput(object sender, EventArgs e)
        {
            EditText usernameView = FindViewById<EditText>(Resource.Id.usernameView);
            EditText passwordView = FindViewById<EditText>(Resource.Id.passwordView);

            string username = usernameView.Text.Trim();
            string password = passwordView.Text.Trim();

            if (username == string.Empty || password == string.Empty || authenticating) return;

            Button loginButton = FindViewById<Button>(Resource.Id.loginButton);
            loginButton.Clickable = false;

            Authenticate(username, password);
        }

        void Authenticate(string username, string password)
        {
            authenticating = true;

            User.Credentials credentials = new User.Credentials()
            {
                username = username,
                password = password,
                newToken = FirebaseInstanceId.Instance.Token
            };

            User.AuthenticateAndProcess(credentials, process);

            void process(User user)
            {
                if (created)
                    RunOnUiThread(editUi);
                void editUi()
                {
                    TextView successMsgView = FindViewById<TextView>(Resource.Id.successMsgView);
                    successMsgView.Text = user.msg;
                    successMsgView.Visibility = Android.Views.ViewStates.Visible;
                }

                if (user.success == true)
                {
                    user.requestedFriends.Remove(string.Empty);
                    user.requestedFriends.Remove(string.Empty);
                    user.requestedFriends.Remove(user.username);

                    GlobalData.Token = user.token;
                    GlobalData.FriendRequests = user.requestedFriends;

                    GetPreferences(FileCreationMode.Private).Edit()
                    .PutString("username", username)
                    .PutString("password", password)
                    .Commit();

                    if (created)
                        RunOnUiThread(_editUi);
                    else
                    {
                        StartNextActivity(username, 1);
                        authenticating = false;
                    }
                    void _editUi()
                    { 
                        EditText usernameView = FindViewById<EditText>(Resource.Id.usernameView);
                        EditText passwordView = FindViewById<EditText>(Resource.Id.passwordView);

                        usernameView.Text = string.Empty;
                        passwordView.Text = string.Empty;

                        TextView successMsgView = FindViewById<TextView>(Resource.Id.successMsgView);
                        successMsgView.Text = string.Empty;
                        successMsgView.Visibility = Android.Views.ViewStates.Gone;

                        StartNextActivity(username, 1);
                        authenticating = false;
                        Button loginButton = FindViewById<Button>(Resource.Id.loginButton);
                        loginButton.Clickable = true;
                    }
                }
                else
                {
                    LogOut();
                    Button loginButton = FindViewById<Button>(Resource.Id.loginButton);
                    loginButton.Clickable = true;
                    authenticating = false;
                }
            }
        }
        void StartNextActivity(string username, int requestCode)
        {
            GlobalData.Username = username;
            ServerStuff.SubscribeToUsername();
            StartActivityForResult(typeof(AllConversationsActivity), requestCode);
        }
        void LogOut()
        {
            ServerStuff.Unsubscribe();
            GlobalData.ClearData();
            GetPreferences(FileCreationMode.Private).Edit()
            .Remove("username")
            .Remove("password")
            .Commit();
            Toast.MakeText(this, "Successfully Logged Out", ToastLength.Short).Show();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case 0:
                    case 1:
                        LogOut();
                        SetUpScreen();
                        break;
                    case 2:
                        break;
                }
            }
            else if (requestCode == 0 || requestCode == 1)
                Finish();
        }
    }
}