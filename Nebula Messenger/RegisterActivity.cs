using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Nebula_Messenger.Server;
using Newtonsoft.Json;
using RestSharp;

namespace Nebula_Messenger
{
    [Activity(Label = "RegisterActivity")]
    public class RegisterActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.register_layout);
            
            Button registerButton = FindViewById<Button>(Resource.Id.registerButton);
            registerButton.Click += Register;
        }

        private void Register(object sender, EventArgs e)
        {
            EditText nameView = FindViewById<EditText>(Resource.Id.nameView);
            EditText emailView = FindViewById<EditText>(Resource.Id.emailView);
            EditText usernameView = FindViewById<EditText>(Resource.Id.newUsernameView);
            EditText password1View = FindViewById<EditText>(Resource.Id.newPasswordView);
            EditText password2View = FindViewById<EditText>(Resource.Id.confirmPasswordView);

            string name = nameView.Text.Trim();
            string email = emailView.Text.Trim();
            string username = usernameView.Text.Trim();
            string password1 = password1View.Text.Trim();
            string password2 = password2View.Text.Trim();

            if (!string.Equals(password1, password2))
            {
                password1View.Text = string.Empty;
                password2View.Text = string.Empty;
                return;
            }

            object jsonBody = new
            {
                name,
                email,
                username,
                password = password1,
                registrationTokens = ""
            };

            ServerStuff.Request(ServerStuff.Users.Register, jsonBody, response);

            void response(IRestResponse obj)
            {
                if (string.IsNullOrEmpty(obj.Content)) return;

                ServerStuff.SuccessResponse registerJson =
                    JsonConvert.DeserializeObject<ServerStuff.SuccessResponse>(obj.Content);

                RunOnUiThread(editUi);
                void editUi()
                {
                    TextView successMsgView = FindViewById<TextView>(Resource.Id.successMsgView);
                    successMsgView.Text = registerJson.msg;
                    successMsgView.Visibility = Android.Views.ViewStates.Visible;
                }

                if (registerJson.success == true)
                {
                    SetResult(Result.Ok);
                    Finish();
                }
            }
        }
    }
}