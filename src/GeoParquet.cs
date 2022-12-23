namespace geoparquet;

public class GeoParquetMetadata
{
    public string version { get; set; }
    public string primary_column { get; set; }
    public Columns columns { get; set; }
}

public class Columns
{
    public Geometry geometry { get; set; }
}

public class Geometry
{
    public string encoding { get; set; }
    public float[] bbox { get; set; }
    public string orientation { get; set; }
    public string geometry_type { get; set; }
}
