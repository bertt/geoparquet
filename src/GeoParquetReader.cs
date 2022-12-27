using Newtonsoft.Json;
using Parquet;

namespace GeoParquet;
public class GeoParquetReader
{
    public static async Task<Tuple<ParquetReader,GeoParquet>> Read(string file)
    {
        var fileStream = File.OpenRead(file);
        var parquetReader = await ParquetReader.CreateAsync(fileStream);
        var geoMetaData = parquetReader.CustomMetadata.First().Value;
        var geoParquet = JsonConvert.DeserializeObject<GeoParquet>(geoMetaData);
        return new Tuple<ParquetReader, GeoParquet>(parquetReader, geoParquet);
    }
}