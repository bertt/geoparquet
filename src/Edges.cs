using System.Runtime.Serialization;

namespace GeoParquet
{
    public enum Edges
    {

        [EnumMember(Value = @"planar")]
        Planar = 0,


        [EnumMember(Value = @"spherical")]
        Spherical = 1,
    }
}