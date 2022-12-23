using Parquet;

namespace geoparquet;
public class GeoParquetFile
{
    public GeoParquetMetadata GeoParquetMetadata { get; }
    public ParquetReader GeoParquetReader { get; }

    public GeoParquetFile(GeoParquetMetadata geoParquetMetadata, ParquetReader geoParquetReader)
    {
        GeoParquetMetadata = geoParquetMetadata;
        GeoParquetReader = geoParquetReader;
    }
}

