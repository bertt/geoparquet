using Parquet;

namespace GeoParquet;
public class GeoParquetFile
{
    public GeoParquet GeoParquetMetadata { get; }
    public ParquetReader GeoParquetReader { get; }

    public GeoParquetFile(GeoParquet geoParquetMetadata, ParquetReader geoParquetReader)
    {
        GeoParquetMetadata = geoParquetMetadata;
        GeoParquetReader = geoParquetReader;
    }
}

