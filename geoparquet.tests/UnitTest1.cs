using Apache.Arrow.Ipc;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using ParquetSharp;
using ParquetSharp.Schema;
using System.Text.Json;

namespace GeoParquet.Tests;

public class Tests
{
    [Test]
    public void ReadExampleMetadataJson()
    {
        // data from https://github.com/opengeospatial/geoparquet/blob/main/examples/example_metadata.json
        var file = "testfixtures/example_metadata.json";
        var json = File.ReadAllText(file);

        var geoParquet = JsonSerializer.Deserialize<GeoParquet>(json);
        Assert.That(geoParquet.Version == "1.2.0-dev");


    }
    [Test]
    // data from https://github.com/opengeospatial/geoparquet/blob/main/examples/example_metadata_point.json
    public void ReadExampleMetadataPoint()
    {
        var file = "testfixtures/example_metadata_point.json";
        var json = File.ReadAllText(file);

        var geoParquet = JsonSerializer.Deserialize<GeoParquet>(json);
        Assert.That(geoParquet.Version == "1.2.0-dev");
    }
    [Test]
    public async Task ReadArrowPointFile()
    {
        var file = "testfixtures/gemeenten2016.arrow";
        var stream = File.OpenRead(file);
        var reader = new ArrowFileReader(stream);
        // Next line gives message about compression see geoarrow-cs for solution
        // var recordBatch = await reader.ReadNextRecordBatchAsync();
    }

    [Test]
    public void ReadUtrechtKunstwerkenFileToDataFrame()
    {
        var file = "testfixtures/utrecht_kunstwerken.parquet";
        var file1 = new ParquetFileReader(file);
        // failed attempt to go to dataframe
        // next line gives error: Unsupported LogicalType Double?[]
        //var dataframe =file1.ToDataFrame();
    }

    [Test]
    public void ReadGeoParquetArrowPolygonFile()
    {
        var file = "testfixtures/gemeenten2016_arrow.parquet";
        var file1 = new ParquetFileReader(file);
        var geoParquet = file1.GetGeoMetadata();
        if (geoParquet != null)
        {
            Assert.That(geoParquet.Version == "1.0.0-beta.1");

            // geoarrow.multipolygon
            var rowGroupReader = file1.RowGroup(0);

            // todo: why is column 'xy' not specified?
            var geomColumnId = GetColumnId(rowGroupReader, "xy");
            Assert.That(geoParquet.Columns.First().Value.Encoding == "geoarrow.multipolygon");

            if (geomColumnId != null)
            {
                var geometryArrow = rowGroupReader.Column((int)geomColumnId).LogicalReader<Double?[][][][]>().First();
                Assert.That(geometryArrow.Length == 1);
                Assert.That(geometryArrow[0].Length == 1);
                Assert.That(geometryArrow[0][0].Length == 165); //165 vertices
                Assert.That(geometryArrow[0][0][0].Length == 2); //2 points
                Assert.That(geometryArrow[0][0][0][0] == 6.8319922331647964); // longitude first vertice
                Assert.That(geometryArrow[0][0][0][1] == 53.327288101088072); // latitude first vertice
            }
        }
    }

    [Test]
    public void ReadGeoParquetWkbFile()
    {
        var file = "testfixtures/gemeenten2016_wkb.parquet";
        var file1 = new ParquetFileReader(file);

        var geoParquet = file1.GetGeoMetadata();
        if(geoParquet != null)
        {
            Assert.That(geoParquet.Version == "1.0.0-beta.1");
            Assert.That(geoParquet.Columns.First().Value.Encoding == "WKB");

            var rowGroupReader = file1.RowGroup(0);
            var gemName = rowGroupReader.Column(3).LogicalReader<String>().First();
            Assert.That(gemName == "Appingedam");

            var geomColumnId = GetColumnId(rowGroupReader, geoParquet.Primary_column);

            if (geomColumnId != null)
            {
                var geometryWkb = rowGroupReader.Column((int)geomColumnId).LogicalReader<byte[]>().First();
                var wkbReader = new WKBReader();
                var multiPolygon = (MultiPolygon)wkbReader.Read(geometryWkb);
                Assert.That(multiPolygon.Coordinates.Count() == 165);
                var firstCoordinate = multiPolygon.Coordinates.First();
                Assert.That(firstCoordinate.CoordinateValue.X == 6.8319922331647964);
                Assert.That(firstCoordinate.CoordinateValue.Y == 53.327288101088072);
            }
        }
    }

    private static int? GetColumnId(RowGroupReader rowGroupReader, string columnName)
    {
        var names = new List<string>();
        var numColumns = rowGroupReader.MetaData.NumColumns;

        for (var i = 0; i < numColumns; i++)
        {
            var name = rowGroupReader.MetaData.Schema.Column(i).Name;
            if (name == columnName)
            {
                return i;
            }
            names.Add(name);
        }

        return null;
    }

    private static GroupNode CreateGeoArrowPointSchema()
    {
        // Create schema for native Point encoding as per GeoParquet spec
        // The geometry column should be a struct with "x" and "y" fields
        var cityNode = new PrimitiveNode(
            "city", Repetition.Optional, LogicalType.String(), PhysicalType.ByteArray);

        // For Point geometry, use separate x and y fields as per GeoParquet native encoding
        var xNode = new PrimitiveNode(
            "x", Repetition.Required, LogicalType.None(), PhysicalType.Double);
        var yNode = new PrimitiveNode(
            "y", Repetition.Required, LogicalType.None(), PhysicalType.Double);

        // The geometry as a struct containing x and y
        var geometryNode = new GroupNode(
            "geometry", Repetition.Optional, new Node[] { xNode, yNode });

        // Root schema
        return new GroupNode(
            "schema", Repetition.Required, new Node[] { cityNode, geometryNode });
    }

    [Test]
    public void WriteGeoParquetWkbFile()
    {
        var columns = new Column[]
        {
            new Column<string>("city"),
            new Column<byte[]>("geometry")
        };

        var geoColumn = new GeoColumn();
        geoColumn.Encoding = "WKB";
        geoColumn.Geometry_types.Add("Point");
        var geometadata = GeoMetadata.GetGeoMetadata(geoColumn);

        var parquetFileWriter = new ParquetFileWriter(@"cities_wkb.parquet", columns, keyValueMetadata: geometadata);
        var rowGroup = parquetFileWriter.AppendRowGroup();

        var nameWriter = rowGroup.NextColumn().LogicalWriter<String>();
        nameWriter.WriteBatch(new string[] { "London", "Derby" });

        var geom0 = new Point(5, 51);
        var geom1 = new Point(5.5, 51);

        var wkbWriter = new WKBWriter();
        var wkbBytes = new byte[][] { wkbWriter.Write(geom0), wkbWriter.Write(geom1) };

        var writer = rowGroup.NextColumn().LogicalWriter<byte[]>();
        writer.WriteBatch(wkbBytes);
        parquetFileWriter.Close();
    }


    [Test]
    public void WriteGeoParquetGeoArrowFile()
    {
        var schema = CreateGeoArrowPointSchema();

        var geoColumn = new GeoColumn();
        geoColumn.Encoding = "point";
        geoColumn.Geometry_types.Add("Point");
        var geometadata = GeoMetadata.GetGeoMetadata(geoColumn);

        var writerProperties = new WriterPropertiesBuilder().Build();
        var parquetFileWriter = new ParquetFileWriter(@"cities_arrow1.parquet", schema, writerProperties, keyValueMetadata: geometadata);
        var rowGroup = parquetFileWriter.AppendRowGroup();

        var nameWriter = rowGroup.NextColumn().LogicalWriter<String>();
        nameWriter.WriteBatch(new string[] { "London", "Derby" });

        var geom0 = new Point(5, 51);
        var geom1 = new Point(5.5, 51);
        
        // For struct-based Point geometry, write x and y as Nested<double> values
        var xWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>?>();
        xWriter.WriteBatch(new Nested<double>?[] { new Nested<double>(geom0.X), new Nested<double>(geom1.X) });
        
        var yWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>?>();
        yWriter.WriteBatch(new Nested<double>?[] { new Nested<double>(geom0.Y), new Nested<double>(geom1.Y) });
        
        parquetFileWriter.Close();
    }

    [Test]
    public void ReadGeoParquetGeoArrowPointFile()
    {
        var fileName = @"test_geoarrow_point_read.parquet";
        
        // First write a native Point file
        var schema = CreateGeoArrowPointSchema();

        var geoColumn = new GeoColumn();
        geoColumn.Encoding = "point";
        geoColumn.Geometry_types.Add("Point");
        var geometadata = GeoMetadata.GetGeoMetadata(geoColumn);

        var writerProperties = new WriterPropertiesBuilder().Build();
        using (var parquetFileWriter = new ParquetFileWriter(fileName, schema, writerProperties, keyValueMetadata: geometadata))
        {
            using (var rowGroup = parquetFileWriter.AppendRowGroup())
            {
                using (var nameWriter = rowGroup.NextColumn().LogicalWriter<String>())
                {
                    nameWriter.WriteBatch(new string[] { "London", "Derby" });
                }

                // Write x coordinates as Nested<double>
                using (var xWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>?>())
                {
                    xWriter.WriteBatch(new Nested<double>?[] { new Nested<double>(5), new Nested<double>(5.5) });
                }
                    
                // Write y coordinates as Nested<double>
                using (var yWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>?>())
                {
                    yWriter.WriteBatch(new Nested<double>?[] { new Nested<double>(51), new Nested<double>(51) });
                }
            }
        }

        // Now read it back
        var file1 = new ParquetFileReader(fileName);
        var geoParquet = file1.GetGeoMetadata();
        Assert.That(geoParquet, Is.Not.Null);
        Assert.That(geoParquet!.Version == "1.1.0");
        Assert.That(geoParquet.Columns.First().Value.Encoding == "point");

        var rowGroupReader = file1.RowGroup(0);
            
        // Read city names
        var cityReader = rowGroupReader.Column(0).LogicalReader<string>();
        var cities = cityReader.ReadAll(2);
        Assert.That(cities[0] == "London");
        Assert.That(cities[1] == "Derby");

        // Read x coordinates
        var xReader = rowGroupReader.Column(1).LogicalReader<Nested<double>?>();
        var xs = xReader.ReadAll(2);
        Assert.That(xs.Length == 2);
        Assert.That(xs[0]!.Value.Value == 5);
        Assert.That(xs[1]!.Value.Value == 5.5);
            
        // Read y coordinates
        var yReader = rowGroupReader.Column(2).LogicalReader<Nested<double>?>();
        var ys = yReader.ReadAll(2);
        Assert.That(ys.Length == 2);
        Assert.That(ys[0]!.Value.Value == 51);
        Assert.That(ys[1]!.Value.Value == 51);
    }

    [Test]
    public void WriteGeoParquetGroupNodeFile()
    {
        var nameNode = new PrimitiveNode(
        "Name", Repetition.Optional, LogicalType.String(), PhysicalType.ByteArray);

        // but which logicaltype for double[]?
        var xyNode = new PrimitiveNode(
        "xy", Repetition.Optional, LogicalType.String(), PhysicalType.ByteArray);

        // Create a group node containing the two nested fields
        var groupNode = new GroupNode(
                "geometry", Repetition.Optional, new Node[] { nameNode });

        // Create the root schema group that contains all top-level columns
        var schema7 = new GroupNode(
                "schema8", Repetition.Required, new Node[] { nameNode });

        var propertiesBuilder = new WriterPropertiesBuilder();
        propertiesBuilder.Compression(Compression.Snappy);
        var writerProperties = propertiesBuilder.Build();
        var fileWriter = new ParquetFileWriter(@"objects.parquet", schema7, writerProperties);

        var messages = new Nested<string?>?[]
        {
            new Nested<string?>("London"),
            new Nested<string?>("Derby")
        };

        var rowGroupWriter = fileWriter.AppendRowGroup();

        var messagesWriter = rowGroupWriter.NextColumn().LogicalWriter<string>();
        messagesWriter.WriteBatch(new string[] { "London", "Derby" });

        fileWriter.Close();
    }
}

