using Newtonsoft.Json;
using Parquet;

namespace GeoParquet;
public static class ParquetReaderExtensions
{
    public static GeoParquet GetGeoMetadata(this ParquetReader parquetReader)
    {
        var geoMetaData = parquetReader.CustomMetadata.First().Value;
        var geoParquet = JsonConvert.DeserializeObject<GeoParquet>(geoMetaData);
        return geoParquet;
    }
}