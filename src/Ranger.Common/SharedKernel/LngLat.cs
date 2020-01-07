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
    }
}