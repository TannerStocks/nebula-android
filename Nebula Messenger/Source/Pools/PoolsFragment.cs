using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.OS;
using Android.Views;
using Android.Widget;
using static Android.Widget.AdapterView;
using Nebula_Messenger.Server;
using RestSharp;
using Newtonsoft.Json;
using Android.Support.Design.Widget;
using Android.Gms.Maps.Model;
using Android.Locations;

namespace Nebula_Messenger.Source.Pools
{
    public class PoolsFragment : MapViewFragment, IOnItemClickListener, IOnItemLongClickListener
    {
        LinearLayout layout;

        public PoolsFragment(Activity activity) : base(activity)
        {
            Title = "Pools";

            mapView = new MapView(activity);
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            mapView.GetMapAsync(this);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (layout != null)
                layout.RemoveAllViews();

            layout = new LinearLayout(Activity)
            {
                LayoutParameters = new ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
                Orientation = Orientation.Vertical
            };

            mapView.LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent, (int)(container.Height * 0.8));
            layout.AddView(mapView);

            ListView pools = new ListView(Activity)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
                Adapter = GlobalData.PoolConvList,
                OnItemClickListener = this,
                OnItemLongClickListener = this
            };
            layout.AddView(pools);

            return layout;
        }

        public override void Add()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);

            TextInputEditText textView = new TextInputEditText(Activity);
            textView.InputType = Android.Text.InputTypes.TextFlagCapWords;
            
            TextInputLayout textLayout = new TextInputLayout(Activity);
            textLayout.Hint = "Pool Name";
            textLayout.AddView(textView);
            textLayout.SetPadding(50, 50, 50, 50);

            builder.SetView(textLayout)
                .SetTitle("Create Pool at Current Location?")
                .SetNegativeButton("Cancel", delegate { })
                .SetPositiveButton("Create", Create);
            AlertDialog dialog = builder.Create();
            dialog.Window.SetSoftInputMode(SoftInput.StateVisible);
            dialog.Show();

            void Create(object _sender, DialogClickEventArgs _e)
            {
                if (string.IsNullOrEmpty(textView.Text)) return;

                Pool newPool = new Pool()
                {
                    connectionLimit = 50,
                    coordinates = new double[] { GlobalData.Location.Latitude, GlobalData.Location.Longitude },
                    name = textView.Text,
                    username = GlobalData.Username,
                    poolid = string.Concat(GlobalData.Location.Latitude.ToString(),
                                            GlobalData.Location.Longitude.ToString())
                };
                Pool.CreatePool(newPool, Response);

                void Response(IRestResponse obj)
                {
                    if (string.IsNullOrEmpty(obj.Content)) return;

                    ServerStuff.SuccessResponse successResponse =
                        JsonConvert.DeserializeObject<ServerStuff.SuccessResponse>(obj.Content);

                    if (successResponse.success)
                    {
                        allPools.Add(newPool);
                        Location location = new Location("")
                        {
                            Latitude = GlobalData.Location.Latitude,
                            Longitude = GlobalData.Location.Longitude
                        };
                        Activity.RunOnUiThread(editUi);
                        void editUi()
                        {
                            OnLocationChanged(location);
                        }
                    }
                    else
                    {
                        Toast.MakeText(Activity, successResponse.msg, ToastLength.Short).Show();
                    }
                }
            }
        }

        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            GlobalData.AssignCurrentConversation(Conversation.Type.Pool, position);
            StartActivityForResult(new Intent(Activity, typeof(PoolActivity)), 0);
        }

        public bool OnItemLongClick(AdapterView parent, View view, int position, long id)
        {
            return false;
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            GlobalData.ClearCurrentConversation();
        }
    }
}