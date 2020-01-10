using System;

namespace Ranger.Common
{
    public class LngLat
    {
        public double Lat { get; private set; }
        public double Lng { get; private set; }

        public LngLat(double lng, double lat)
        {
            this.Lng = lng;
            this.Lat = lat;
        }

        public override bool Equals(object obj)
        {
            return obj is LngLat lngLat &&
                   Lat == lngLat.Lat &&
                   Lng == lngLat.Lng;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Lat, Lng);
        }
    }
}