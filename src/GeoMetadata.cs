using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GeoParquet;
public static class GeoMetadata
{
    public static Dictionary<string, string> GetGeoMetadata(GeoColumn geoColumn)
    {
        var parquet = new GeoParquet();
        parquet.Version = "1.0.0-beta.1";
        parquet.Primary_column = "geometry";
        parquet.Columns.Add("geometry", geoColumn);

        var serializerSettings = new JsonSerializerSettings();
        serializerSettings.NullValueHandling = NullValueHandling.Ignore;
        serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

        var json = JsonConvert.SerializeObject(parquet, serializerSettings);
        var dict = new Dictionary<string, string>
        {
            ["geo"] = json
        };
        return dict;
    }
}
