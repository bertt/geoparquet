# GeoParquet

.NET 6 Reader/Writer library for GeoParquet files.

https://geoparquet.org/

Specification: https://github.com/opengeospatial/geoparquet/blob/main/format-specs/geoparquet.md

Blog: https://bertt.wordpress.com/2022/12/20/geoparquet-geospatial-vector-data-using-apache-parquet/

NuGet: https://www.nuget.org/packages/bertt.geoparquet/

## Architecture

In this package there are functions for handling Geometadata

1] Reading

For reading GeoParquet files these is a ParquetFileReader extension method GetGeoMetadata() to obtain the Geo metadata

2] Writing 

For writing GeoParquet files these a GeoMetadata.GetGeoMetadata(string geometry_type, double[] bbox, string geometry_colum="geometry") static function to get the geo metadata dictionary. This 
dictionary can be passed to the ParquetFileWriter constructor.

geometry_type can be one of  Point, LineString, Polygon, MultiPoint, MultiLineString, MultiPolygon, GeometryCollection.

See sample code below for reading/writing samples.

## Sample code

In these samples NetTopologySuite (https://github.com/NetTopologySuite/NetTopologySuite) is used for handling geometries, but any library that can handle 
WKB geometries can be used.

### Reading:

Use extension method 'parquetReader.GetGeoMetadata()':

```
// 0] read the GeoParquet file
var file = "testfixtures/gemeenten2016_0.4.parquet";
var parquetFileReader = new ParquetFileReader(file);
var rowGroupReader = parquetFileReader.RowGroup(0);
var numRows = (int)rowGroupReader.MetaData.NumRows;
Assert.That(numRows == 391);
var numColumns = (int)rowGroupReader.MetaData.NumColumns;
Assert.That(numColumns == 36);

// 1] Use the GeoParquet metadata
var geoParquet = parquetFileReader.GetGeoMetadata();
Assert.That(geoParquet.Version == "0.4.0");
Assert.That(geoParquet.Primary_column == "geometry");
Assert.That(geoParquet.Columns.Count == 1);
var geomColumn = (JObject)geoParquet.Columns.First().Value;
Assert.That(geomColumn?["encoding"].ToString() == "WKB");
Assert.That(geomColumn?["orientation"].ToString() == "counterclockwise");
Assert.That(geomColumn?["geometry_type"].ToString() == "MultiPolygon");
var bbox = (JArray)geomColumn?["bbox"];
Assert.That(bbox?.Count == 4);

// 2] Read table values
var nameColumn1 = rowGroupReader.Column(3).LogicalReader<String>().First();
Assert.That(nameColumn1 == "Appingedam");

// 4] Read WKB to create NetTopologySuite geometry
var geometryWkb = rowGroupReader.Column(35).LogicalReader<byte[]>().First();

var wkbReader = new WKBReader();
var multiPolygon = (MultiPolygon)wkbReader.Read(geometryWkb);
Assert.That(multiPolygon.Coordinates.Count() == 165);
var firstCoordinate = multiPolygon.Coordinates.First();
Assert.That(firstCoordinate.CoordinateValue.X == 6.8319922331647964);
Assert.That(firstCoordinate.CoordinateValue.Y == 53.327288101088072);
```

To read Apache Arrow encoded geometry use type 'Double?[][][][]' for MultiPolygons:

```
var file = "testfixtures/gemeenten2016_0.4_arrow.parquet";
var file1 = new ParquetFileReader(file);
var rowGroupReader = file1.RowGroup(0);
var groupNumRows = (int)rowGroupReader.MetaData.NumRows;
var geometryColumn = rowGroupReader.Column(35).LogicalReader<Double?[][][][]>().ReadAll(groupNumRows);
var firstArrowGeom = geometryColumn[0];
Assert.That(firstArrowGeom[0][0].Length == 165);
```

### Writing 

Use GeoMetadata.GetGeoMetadata to construct the ParquetFileWriter, store the geometries as WKB.

Result Parquet file can be visualized in QGIS:

![image](https://user-images.githubusercontent.com/538812/210020220-b89da098-0877-45bd-87f2-8285941bf697.png)

```
var columns = new Column[]
{
    new Column<string>("city"),
    new Column<byte[]>("geometry")
};

var bbox = new double[] {  3.3583782525105832,
            50.750367484598314,
            7.2274984508458306,
            53.555014517907608};

var geometadata = GeoMetadata.GetGeoMetadata("Point", bbox);

var parquetFileWriter = new ParquetFileWriter(@"writing_sample.parquet", columns,Compression.Snappy,geometadata);
var rowGroup = parquetFileWriter.AppendRowGroup();

var nameWriter = rowGroup.NextColumn().LogicalWriter<String>();
nameWriter.WriteBatch(new string[] { "London", "Derby" });
        
var geom0 = new Point(5, 51);
var geom1 = new Point(5.5, 51);

var wkbWriter = new WKBWriter();
var wkbBytes = new byte[][] { wkbWriter.Write(geom0), wkbWriter.Write(geom1) };

var geometryWriter = rowGroup.NextColumn().LogicalWriter<byte[]>();
geometryWriter.WriteBatch(wkbBytes);

parquetFileWriter.Close();
```

## Dependencies

- Newtonsoft.JSON 13 https://github.com/JamesNK/Newtonsoft.Json

- ParquetSharp 10 https://github.com/G-Research/ParquetSharp

## Schema generation 

At the moment we use schema 0.4 from:

https://geoparquet.org/releases/v0.4.0/schema.json

When there are testfiles available with schema 1.0.0-beta.1 we can switch to that version.

https://geoparquet.org/releases/v1.0.0-beta.1/schema.json

GeoParquet metadata classes are generated from JSON schema using NJsonSchema.CodeGeneration.CSharp (https://github.com/RicoSuter/NJsonSchema), see console project 
'geoparquet.codegen' for details.

# Roadmap

- Add support for multiple geometry columns;

- Add support for GeoParquet 1.0;

- add writing Apache Arrow encoding for geometries;

- add support for crs;

- add (spatial) filters.

## History

2023-01-01: version 0.4 - using ParquetSharp 10.0.1-beta1 instead of Parquet.Net 4

2022-12-30: version 0.3.1 - make geometry column name optional in SetGeoMetadata

2022-12-30: version 0.3 - add extension method to write geo metadata

2022-12-27: version 0.2 - add extension method to read geo metadata

2022-12-23: initial 0.1 version implementing reader

