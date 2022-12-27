using Newtonsoft.Json;
using Parquet;

namespace GeoParquet;
public class GeoParquetReader
{
    public static GeoParquet GetGeoMetadata(ParquetReader parquetReader)
    {
        var geoMetaData = parquetReader.CustomMetadata.First().Value;
        var geoParquet = JsonConvert.DeserializeObject<GeoParquet>(geoMetaData);
        return geoParquet;
    }
}