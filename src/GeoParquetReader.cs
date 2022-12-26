using Newtonsoft.Json;
using Parquet;

namespace GeoParquet;
public class GeoParquetReader
{
    public static async Task<GeoParquetFile> ReadGeoParquet(string file)
    {
        var fileStream = File.OpenRead(file);
        var parquetReader = await ParquetReader.CreateAsync(fileStream);
        var geoMetaData = parquetReader.CustomMetadata.First().Value;
        var geoParquetMetaData = JsonConvert.DeserializeObject<GeoParquet>(geoMetaData);
        var geoParquetFile = new GeoParquetFile(geoParquetMetaData, parquetReader);
        return geoParquetFile;
    }
}