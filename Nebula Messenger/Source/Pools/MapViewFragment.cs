using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Gms.Tasks;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Widget;
using Java.Lang;
using Nebula_Messenger.Server;
using System.Collections.Generic;
using static Android.Gms.Maps.GoogleMap;

namespace Nebula_Messenger.Source.Pools
{
    public class MapViewFragment : TitledFragment,
        IOnMapReadyCallback, IOnMapLoadedCallback,
        IOnSuccessListener, Android.Locations.ILocationListener
    {
        LocationManager locationManager;

        protected MapView mapView;
        protected GoogleMap googleMap;
        protected List<Pool> allPools;

        bool locationPermissionGranted;

        public MapViewFragment(Activity activity)
        {
            GetLocationPermission(activity);

            Pool.GetAndProcessPools(getPools);
            void getPools(List<Pool> pools)
            {
                allPools = pools;
            }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            mapView.OnCreate(savedInstanceState);

            locationManager = (LocationManager)
                Activity.GetSystemService(Android.Content.Context.LocationService);

            if (locationManager.IsProviderEnabled(LocationManager.GpsProvider) && locationPermissionGranted)
            {
                try
                {
                    locationManager.RequestLocationUpdates(LocationManager.GpsProvider, 2000, 1, this);
                    Task locationResult = LocationServices.GetFusedLocationProviderClient(Activity).LastLocation;
                    locationResult.AddOnSuccessListener(this);
                }
                catch (Exception e)
                {
                    Toast.MakeText(Context, e.Message, ToastLength.Long).Show();
                }
            }
            else
                Toast.MakeText(Context, "Try Turning On Location Services", ToastLength.Long).Show();
        }

        public void OnSuccess(Object result)
        {
            if (GlobalData.Location != null || result == null) return;

            Location lastLocation = (Location)result;
            GlobalData.Location = new LatLng(lastLocation.Latitude, lastLocation.Longitude);

            if (googleMap != null)
                InitializeCamera();
        }


        public void OnMapReady(GoogleMap _googleMap)
        {
            googleMap = _googleMap;
            googleMap.SetOnMapLoadedCallback(this);
            googleMap.UiSettings.MapToolbarEnabled = true;

            if (locationPermissionGranted)
                ShowLocationUi();

            if (GlobalData.Location != null)
                InitializeCamera();
        }
        void InitializeCamera()
        {
            CameraUpdate cameraStart = CameraUpdateFactory.NewLatLngZoom(GlobalData.Location, 7);
            googleMap.MoveCamera(cameraStart);
        }
        public void OnMapLoaded()
        {
        }

        private void ShowLocationUi()
        {
            googleMap.MyLocationEnabled = true;
            googleMap.UiSettings.MyLocationButtonEnabled = true;
        }

        void GetLocationPermission(Activity activity)
        {
            locationPermissionGranted =
                ContextCompat.CheckSelfPermission(activity, Manifest.Permission.AccessFineLocation)
                == Permission.Granted;

            if (!locationPermissionGranted)
                ActivityCompat.RequestPermissions(
                    activity,
                    new string[] { Manifest.Permission.AccessFineLocation },
                    0);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            switch (requestCode)
            {
                case 0:
                    if (grantResults.Length > 0)
                        locationPermissionGranted = grantResults[0] == Permission.Granted;
                    if (locationPermissionGranted && googleMap != null)
                        ShowLocationUi();
                    break;
            }
        }


        public void OnLocationChanged(Location location)
        {
            if (googleMap == null) return;

            GlobalData.Location = new LatLng(location.Latitude, location.Longitude);

            googleMap.Clear();

            List<Pool> localPools = FindLocalPools();
            GlobalData.PoolConvList.SetPools(localPools);
            GlobalData.PoolConvList.NotifyDataSetChanged();

            CreateMarkers();

            CameraUpdate cameraUpdate = CameraUpdateFactory.NewLatLngZoom(GlobalData.Location, 20);
            googleMap.AnimateCamera(cameraUpdate);
        }
        void CreateMarkers()
        {
            foreach(Pool pool in allPools)
            {
                LatLng poolLocation = new LatLng(pool.coordinates[0], pool.coordinates[1]);

                MarkerOptions markerOptions = new MarkerOptions();
                markerOptions.SetPosition(poolLocation);
                markerOptions.SetTitle(pool.name);

                googleMap.AddMarker(markerOptions);

                CircleOptions circleOptions = new CircleOptions();
                circleOptions.InvokeCenter(poolLocation);
                circleOptions.InvokeRadius(30);
                circleOptions.InvokeStrokeWidth(2);
                circleOptions.InvokeStrokeColor(
                    ContextCompat.GetColor(Activity, Resource.Color.colorAccentLight));
                circleOptions.InvokeFillColor(
                    ContextCompat.GetColor(Activity, Resource.Color.colorAccentClear));

                googleMap.AddCircle(circleOptions);
            }
        }
        List<Pool> FindLocalPools()
        {
            List<Pool> pools = new List<Pool>();

            double range = 0.00034;
            foreach (Pool pool in allPools)
            {
                double dist = Math.Sqrt(sqr(pool.coordinates[0] - GlobalData.Location.Latitude) +
                    sqr(pool.coordinates[1] - GlobalData.Location.Longitude));
                if (dist <= range)
                    pools.Add(pool);
            }
            return pools;
        }
        double sqr(double a)
        {
            return a * a;
        }
        public void OnProviderDisabled(string provider) { }
        public void OnProviderEnabled(string provider) { }
        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras) { }


        //forward lifecycle methods to mapView
        public override void OnStart()
        {
            base.OnStart();
            mapView.OnStart();
        }
        public override void OnResume()
        {
            base.OnResume();
            mapView.OnResume();
        }
        public override void OnPause()
        {
            base.OnPause();
            mapView.OnPause();
        }
        public override void OnStop()
        {
            base.OnStop();
            mapView.OnStop();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            mapView.OnDestroy();
        }
        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            mapView.OnSaveInstanceState(outState);
        }
        public override void OnLowMemory()
        {
            base.OnLowMemory();
            mapView.OnLowMemory();
        }
    }
}