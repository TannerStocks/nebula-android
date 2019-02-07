using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Net;
using Android.Support.V4.App;
using Firebase.Messaging;

namespace Nebula_Messenger
{
    [Service(Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    [IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE"})]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            message.Data.TryGetValue("id", out string conversationId);

            if (string.IsNullOrEmpty(conversationId)) return;

            if (GlobalData.CurrentConversation == null || conversationId != GlobalData.CurrentConversation.id)
            {
                PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0,
                    new Intent(this, typeof(LoginActivity)),
                    PendingIntentFlags.CancelCurrent);

                long[] pattern = { 0, 400, 100, 100, 100, 400 };

                Uri alarmSound = RingtoneManager.GetDefaultUri(RingtoneType.Notification);

                Notification notification = new NotificationCompat.Builder(this)
                    .SetContentTitle(message.GetNotification().Title)
                    .SetContentText(message.GetNotification().Body)
                    .SetContentIntent(pendingIntent)
                    .SetAutoCancel(true)
                    .SetSmallIcon(Resource.Mipmap.noti_icon)
                    .SetLights(Color.Purple.ToArgb(), 500, 500)
                    .SetVibrate(pattern)
                    .SetSound(alarmSound)

                    .Build();

                NotificationManagerCompat manager = NotificationManagerCompat.From(this);
                manager.Notify(conversationId, 0, notification);
            }
        }
    }
}