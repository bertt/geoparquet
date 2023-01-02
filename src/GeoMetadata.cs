using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace GeoParquet;
public static class GeoMetadata
{
    public static Dictionary<string, string> GetGeoMetadata(string geometry_type, double[] bbox, string geometry_column = "geometry", string encoding = "WKB")
    {
        var parquet = new GeoParquet();
        parquet.Version = "0.4.0";
        parquet.Primary_column = geometry_column;

        var o = new JObject();
        o["encoding"] = encoding;
        o["orientation"] = "counterclockwise";
        o["geometry_type"] = geometry_type;
        o["bbox"] = new JArray() { bbox[0], bbox[1], bbox[2], bbox[3] };

        var colKvp = new KeyValuePair<string, object>(geometry_column, o);
        parquet.Columns.Add(colKvp);

        var json = JsonConvert.SerializeObject(parquet);
        var dict = new Dictionary<string, string>
        {
            ["geo"] = json
        };
        return dict;
    }
}
