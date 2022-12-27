# GeoParquet

.NET 6 Reader/Writer library for GeoParquet files.

https://geoparquet.org/

https://getindata.com/blog/introducing-geoparquet-data-format/

Blog about GeoParquet: https://bertt.wordpress.com/2022/12/20/geoparquet-geospatial-vector-data-using-apache-parquet/

NuGet: https://www.nuget.org/packages/bertt.geoparquet/

## Sample code

Reading:

```
// 0] read the GeoParquet file
var (parquetReader, geoParquet) = await GeoParquetReader.Read("testfixtures/gemeenten2016.parquet");
var dataFields = parquetReader.Schema.GetDataFields();
Assert.That(dataFields.Length == 36);
var reader = parquetReader.OpenRowGroupReader(0);

// 1] Use the ParquetReader to read table
var nameColumn = await reader.ReadColumnAsync(dataFields[3]);
Assert.That(nameColumn.Data.Length == 391);
Assert.That((string)nameColumn.Data.GetValue(0) == "Appingedam");

// 2] Use the GeoParquet metadata
Assert.That(geoParquet.Version == "0.4.0");
Assert.That(geoParquet.Primary_column == "geometry");
Assert.That(geoParquet.Columns.Count == 1);
var geomColumn = (JObject)geoParquet.Columns.First().Value;
Assert.That(geomColumn?["encoding"].ToString() == "WKB");
Assert.That(geomColumn?["orientation"].ToString() == "counterclockwise");
Assert.That(geomColumn?["geometry_type"].ToString() == "MultiPolygon");
var bbox = (JArray)geomColumn?["bbox"];
Assert.That(bbox?.Count == 4);

// 3] Use the geometry column (WKB) to create NetTopologySuite geometry
var geometryColumn = await reader.ReadColumnAsync(dataFields[35]);
var geometryWkb = (byte[])geometryColumn.Data.GetValue(0);
var wkbReader = new WKBReader();
var multiPolygon = (MultiPolygon)wkbReader.Read(geometryWkb);
Assert.That(multiPolygon.Coordinates.Count() == 165);
```

Writing: 

todo 

## Dependencies

- Newtonsoft.JSON 13

- Parquet.Net 4 https://github.com/aloneguid/parquet-dotnet

## Schema generation 

At the moment we use schema 0.4 from:

https://geoparquet.org/releases/v0.4.0/schema.json

When there are testfiles available with schema 1.0.0-beta.1 we can switch to that version.

https://geoparquet.org/releases/v1.0.0-beta.1/schema.json

GeoParquet metadata classes are generated from JSON schema using NJsonSchema.CodeGeneration.CSharp (https://github.com/RicoSuter/NJsonSchema), see console project 
'geoparquet.codegen' for details.


# Roadmap

- add writing geoParquet file;

- add conversion methods (to/from NTS);

- add (spatial) filters;

- add read from cloud provider.

## History

2022-12-23: initial 0.1 version implementing reader

