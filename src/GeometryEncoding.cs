namespace GeoParquet;

/// <summary>
/// Defines the possible encoding types for geometry columns in GeoParquet files.
/// </summary>
public static class GeometryEncoding
{
    /// <summary>
    /// Well-Known Binary (WKB) encoding.
    /// </summary>
    public const string WKB = "WKB";
    
    /// <summary>
    /// GeoArrow Point encoding.
    /// </summary>
    public const string Point = "point";
    
    /// <summary>
    /// GeoArrow LineString encoding.
    /// </summary>
    public const string LineString = "linestring";
    
    /// <summary>
    /// GeoArrow Polygon encoding.
    /// </summary>
    public const string Polygon = "polygon";
    
    /// <summary>
    /// GeoArrow MultiPoint encoding.
    /// </summary>
    public const string MultiPoint = "multipoint";
    
    /// <summary>
    /// GeoArrow MultiLineString encoding.
    /// </summary>
    public const string MultiLineString = "multilinestring";
    
    /// <summary>
    /// GeoArrow MultiPolygon encoding.
    /// </summary>
    public const string MultiPolygon = "geoarrow.multipolygon";
}
