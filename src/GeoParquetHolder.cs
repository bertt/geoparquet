using Parquet;

namespace GeoParquet;
public class GeoParquetHolder
{
    public GeoParquet GeoParquet { get; }
    public ParquetReader GeoParquetReader { get; }

    public GeoParquetHolder(GeoParquet geoParquetMetadata, ParquetReader geoParquetReader)
    {
        GeoParquet = geoParquetMetadata;
        GeoParquetReader = geoParquetReader;
    }
}

