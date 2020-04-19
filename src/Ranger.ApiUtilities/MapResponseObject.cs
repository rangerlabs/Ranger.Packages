using AutoWrapper;

namespace Ranger.ApiUtilities
{
    public class MapResponseObject
    {
        [AutoWrapperPropertyMap(Prop.ResponseException)]
        public object Error { get; set; }
    }
}