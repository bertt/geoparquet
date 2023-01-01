using Newtonsoft.Json;
using ParquetSharp;

namespace GeoParquet;
public static class GeoParquetExtensions
{
    public static GeoParquet GetGeoMetadata(this ParquetFileReader parquetFileReader)
    {
        var metadata = parquetFileReader.FileMetaData.KeyValueMetadata;
        var geoMetaData = metadata.GetValueOrDefault("geo");
        var geoParquet = JsonConvert.DeserializeObject<GeoParquet>(geoMetaData);
        return geoParquet;
    }
}