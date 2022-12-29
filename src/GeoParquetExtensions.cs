using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parquet;

namespace GeoParquet;
public static class GeoParquetExtensions
{
    public static GeoParquet GetGeoMetadata(this ParquetReader parquetReader)
    {
        var geoMetaData = parquetReader.CustomMetadata.GetValueOrDefault("geo");
        var geoParquet = JsonConvert.DeserializeObject<GeoParquet>(geoMetaData);
        return geoParquet;
    }

    public static void SetGeoMetadata(this ParquetWriter parquetWriter, string geometry_type, double[] bbox, string geometry_column="geometry")
    {
        var parquet = new GeoParquet();
        parquet.Version = "0.4.0";
        parquet.Primary_column = geometry_column;

        var o = new JObject();
        o["encoding"] = "WKB";
        o["orientation"] = "counterclockwise";
        o["geometry_type"] = geometry_type;
        o["bbox"] = new JArray() { bbox[0], bbox[1], bbox[2], bbox[3] };

        var colKvp = new KeyValuePair<string, object>(geometry_column, o);
        parquet.Columns.Add(colKvp);

        var json = JsonConvert.SerializeObject(parquet);
        parquetWriter.CustomMetadata = new Dictionary<string, string>
        {
            ["geo"] = json
        };
    }
}