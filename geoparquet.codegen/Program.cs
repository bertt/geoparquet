using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

// schema sources:
// - https://geoparquet.org/releases/v1.0.0/schema.json
var json = File.ReadAllText("./1.1.0/schema.json");

// there are two manual actions :-(
//
// 1] remove crs from schema because too complex
//       "crs": {
// "oneOf": [
//                { "$ref": "https://proj.org/schemas/v0.5/projjson.schema.json" },
//                { "type": "null" }
//             ]
//            },
// 2] Change in resulting file GeoParquet.cs - Rename object Anonymous to GeoColumn

var schema = await JsonSchema.FromJsonAsync(json);
var settings = new CSharpGeneratorSettings() { JsonLibrary = CSharpJsonLibrary.SystemTextJson };
settings.Namespace = "GeoParquet";
var generator = new CSharpGenerator(schema, settings);

var file = generator.GenerateFile();
File.WriteAllText("../../../../src/GeoParquet.cs", file);