using ParquetSharp;
using System.Text.Json;

namespace GeoParquet;
public static class GeoParquetExtensions
{
    public static GeoParquet GetGeoMetadata(this ParquetFileReader parquetFileReader)
    {
        var metadata = parquetFileReader.FileMetaData.KeyValueMetadata;
        var geoMetaData = metadata.GetValueOrDefault("geo");
        var geoParquet = JsonSerializer.Deserialize<GeoParquet>(geoMetaData);
        return geoParquet;
    }
}