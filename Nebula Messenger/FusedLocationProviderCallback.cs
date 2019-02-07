using Android.Gms.Location;
using Android.Gms.Maps.Model;

namespace Nebula_Messenger
{
    class FusedLocationProviderCallback : LocationCallback
    {
        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
        }

        public override void OnLocationResult(LocationResult result)
        {
            //location = new LatLng(result.LastLocation.Latitude, result.LastLocation.Longitude);
        }
    }
}