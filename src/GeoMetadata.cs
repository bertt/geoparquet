﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace GeoParquet;
public static class GeoMetadata
{
    public static Dictionary<string, string> GetGeoMetadata(GeoColumn geoColumn)
    {
        var parquet = new GeoParquet();
        parquet.Version = "1.0.0-beta.1";
        parquet.Primary_column = "geometry";
        parquet.Columns.Add("geometry", geoColumn);


        JsonSerializerOptions options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        //var serializerSettings = new JsonSerializerSettings();
        //serializerSettings.NullValueHandling = NullValueHandling.Ignore;
        //serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

        var json = JsonSerializer.Serialize(parquet, options);
        var dict = new Dictionary<string, string>
        {
            ["geo"] = json
        };
        return dict;
    }
}
