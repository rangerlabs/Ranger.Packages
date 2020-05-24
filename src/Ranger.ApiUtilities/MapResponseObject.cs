using AutoWrapper;

namespace Ranger.ApiUtilities
{
    public class MapResponseObject
    {
        [AutoWrapperPropertyMap(Prop.ResponseException)]
        public object Error { get; set; }
        [AutoWrapperPropertyMap(Prop.ResponseException_ExceptionMessage)]
        public object Message { get; set; }
    }
}