using ParquetSharp;
using System.Text.Json;

namespace GeoParquet;
public static class GeoParquetExtensions
{
    public static String? GetGeoMetadataAsString(this ParquetFileReader parquetFileReader)
    {
        var metadata = parquetFileReader.FileMetaData.KeyValueMetadata;
        var geoMetaData = metadata.GetValueOrDefault("geo");
        return geoMetaData;
    }

    public static GeoParquet? GetGeoMetadata(this ParquetFileReader parquetFileReader)
    {
        var geoMetaData = parquetFileReader.GetGeoMetadataAsString();
        if(geoMetaData != null)
        {
            return JsonSerializer.Deserialize<GeoParquet>(geoMetaData);
        }
        return null;
    }
}