# GeoParquet

.NET 6 Reader/Writer library form GeoParquet files.

https://geoparquet.org/

https://getindata.com/blog/introducing-geoparquet-data-format/

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

https://github.com/opengeospatial/geoparquet/blob/main/format-specs/schema.json

## History

2022-12-23: initial 0.1 version implementing reader

