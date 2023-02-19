using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

// schema sources:
// - https://geoparquet.org/releases/v0.4.0/schema.json
// - https://geoparquet.org/releases/v1.0.0-beta.1/schema.json
// we will use 0.4 now because gdal does not convert (yet) to 1.0
var json = File.ReadAllText("./v0.4.0/schema.json");
// var json = File.ReadAllText("./1.0.0-beta.1/schema.json");


// there is one manual action: remove crs from schema because too complex
/*       "crs": {
"oneOf": [
                { "$ref": "https://proj.org/schemas/v0.5/projjson.schema.json" },
                { "type": "null" }
              ]
            },
*/

var schema = await JsonSchema.FromJsonAsync(json);
var settings = new CSharpGeneratorSettings() { JsonLibrary = CSharpJsonLibrary.SystemTextJson };
settings.Namespace = "GeoParquet";
var generator = new CSharpGenerator(schema, settings);

var file = generator.GenerateFile();
File.WriteAllText("../../../../src/GeoParquet.cs", file);