using Parquet;

namespace GeoParquet;
public class GeoParquetHolder
{
    public GeoParquet GeoParquet { get; }
    public ParquetReader ParquetReader { get; }

    public GeoParquetHolder(GeoParquet geoParquetMetadata, ParquetReader parquetReader)
    {
        GeoParquet = geoParquetMetadata;
        ParquetReader = parquetReader;
    }
}

