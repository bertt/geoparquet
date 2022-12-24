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
var geoParquetMetaData = reader.GeoParquetMetadata;
Assert.IsTrue(geoParquetMetaData.version == "0.4.0");
Assert.IsTrue(geoParquetMetaData.primary_column == "geometry");
Assert.IsTrue(geoParquetMetaData.columns.geometry.bbox.Length == 4);
Assert.IsTrue(geoParquetMetaData.columns.geometry.geometry_type == "MultiPolygon");
Assert.IsTrue(geoParquetMetaData.columns.geometry.orientation == "counterclockwise");
Assert.IsTrue(geoParquetMetaData.columns.geometry.encoding == "WKB");
```

Writing: 

todo 

## Dependencies

- Newtonsoft.JSON 9

- Parquet.Net 4 https://github.com/aloneguid/parquet-dotnet

## Schema generation 

Schema used: 

https://geoparquet.org/releases/v1.0.0-beta.1/schema.json

# Roadmap

- add writing geoParquet file;

- generate geo metadata classes from json schema;

- add conversion methods (to/from NTS);

- add (spatial) filters;

- add read from cloud provider.

## History

2022-12-23: initial 0.1 version implementing reader

