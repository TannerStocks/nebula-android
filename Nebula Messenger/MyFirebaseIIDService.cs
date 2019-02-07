using System;
using Android.App;
using Firebase.Iid;

namespace Nebula_Messenger
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseIIDService : FirebaseInstanceIdService
    {
        public override void OnTokenRefresh()
        {
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            
            SendRegistrationToServer(refreshedToken);
        }

        void SendRegistrationToServer(string token)
        {

        }
    }
}