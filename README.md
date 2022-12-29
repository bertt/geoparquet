# GeoParquet

.NET 6 Reader/Writer library for GeoParquet files.

https://geoparquet.org/

Specification: https://github.com/opengeospatial/geoparquet/blob/main/format-specs/geoparquet.md

Blog: https://bertt.wordpress.com/2022/12/20/geoparquet-geospatial-vector-data-using-apache-parquet/

NuGet: https://www.nuget.org/packages/bertt.geoparquet/

## Architecture

In this package there are two extension methods for handling the geo metadata:

1] ParquetReader extension method 'GetGeoMetadata()' to obtain the Geo metadata

2] ParquetWriter extension method SetGeoMetadata(string geometry_column, string geometry_type, double[] bbox)

See sample code below for reading/writing samples/

## Sample code

In these samples NetTopologySuite (https://github.com/NetTopologySuite/NetTopologySuite) is used for handling geometries, but any library that can handle 
WKB geometries can be used.

### Reading:

Use extension method 'parquetReader.GetGeoMetadata()':


```
// 0] read the GeoParquet file
var file = "testfixtures/gemeenten2016_0.4.parquet";
var fileStream = File.OpenRead(file);
var parquetReader = await ParquetReader.CreateAsync(fileStream);
var dataFields = parquetReader.Schema.GetDataFields();
Assert.That(dataFields.Length == 36);
var reader = parquetReader.OpenRowGroupReader(0);

// 1] Use the ParquetReader to read table
var nameColumn = await reader.ReadColumnAsync(dataFields[3]);
Assert.That(nameColumn.Data.Length == 391);
Assert.That((string)nameColumn.Data.GetValue(0) == "Appingedam");

// 2] Use the GeoParquet metadata
var geoParquet = parquetReader.GetGeoMetadata();
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

### Writing 

Use extension method 'parquetWriter.SetGeoMetadata', store the geometries as WKB.

Result Parquet file can be visualized in QGIS:

![image](https://user-images.githubusercontent.com/538812/210020220-b89da098-0877-45bd-87f2-8285941bf697.png)

```
var cityColumn = new DataColumn(
    new DataField<string>("city"),
    new string[] { "London", "Derby" });

var geom0 = new Point(5, 51);
var geom1 = new Point(5.5, 51);

var wkbWriter = new WKBWriter();

var wkbColumn = new DataColumn(
    new DataField<byte[]>("geometry"),
    new byte[][] { wkbWriter.Write(geom0), wkbWriter.Write(geom1) });

var schema = new Schema(cityColumn.Field, wkbColumn.Field);

using (var stream = File.OpenWrite(@"writing_sample.parquet"))
{
    using (ParquetWriter parquetWriter = await ParquetWriter.CreateAsync(schema, stream))
    {
        using (ParquetRowGroupWriter groupWriter = parquetWriter.CreateRowGroup())
        {
            await groupWriter.WriteColumnAsync(cityColumn);
            await groupWriter.WriteColumnAsync(wkbColumn);
        }

        parquetWriter.SetGeoMetadata("geometry", "Point", new double[] {  3.3583782525105832,
            50.750367484598314,
            7.2274984508458306,
            53.555014517907608});
    }
}
```


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

- Add support for 1.0

- add Apache Arrow encoding for geometries

- add (spatial) filters;

- add read from cloud provider.

## History

2022-12-30: version 0.3 - add extension method to write geo metadata

2022-12-27: version 0.2 - add extension method to read geo metadata

2022-12-23: initial 0.1 version implementing reader

