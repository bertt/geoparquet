# GeoParquet

.NET 6 Reader/Writer library for GeoParquet files.

https://geoparquet.org/

https://getindata.com/blog/introducing-geoparquet-data-format/

Blog about GeoParquet: https://bertt.wordpress.com/2022/12/20/geoparquet-geospatial-vector-data-using-apache-parquet/

NuGet: https://www.nuget.org/packages/bertt.geoparquet/

## Sample code

Reading:

```
var reader = await GeoParquetReader.ReadGeoParquet("testfixtures/gemeenten2016.parquet");
var dataFields = reader.GeoParquetReader.Schema.GetDataFields();
Assert.IsTrue(dataFields.Length == 36);
var geoParquetMetaData = reader.GeoParquetMetadata;
Assert.IsTrue(geoParquetMetaData.Version == "0.4.0");
Assert.IsTrue(geoParquetMetaData.Primary_column == "geometry");
Assert.IsTrue(geoParquetMetaData.Columns.Count == 1);
var geomColumn = (JObject)geoParquetMetaData.Columns.First().Value;
Assert.IsTrue(geomColumn["encoding"].ToString() == "WKB");
Assert.IsTrue(geomColumn["orientation"].ToString() == "counterclockwise");
Assert.IsTrue(geomColumn["geometry_type"].ToString() == "MultiPolygon");
var bbox = (JArray)geomColumn["bbox"];
Assert.IsTrue(bbox.Count == 4);
```

Writing: 

todo 

## Dependencies

- Newtonsoft.JSON 9

- Parquet.Net 4 https://github.com/aloneguid/parquet-dotnet

## Schema generation 

At the moment we use schema 0.4 from:

https://geoparquet.org/releases/v0.4.0/schema.json

When thee are testfiles available with schema 1.0.0-beta.1 we can switch to that version.

https://geoparquet.org/releases/v1.0.0-beta.1/schema.json

GeoParquet metadata classes are generated from JSON schema using NJsonSchema.CodeGeneration.CSharp, see console project 
'geoparquet.codegen' for details.


# Roadmap

- add writing geoParquet file;

- add conversion methods (to/from NTS);

- add (spatial) filters;

- add read from cloud provider.

## History

2022-12-23: initial 0.1 version implementing reader

