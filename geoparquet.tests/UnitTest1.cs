using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using ParquetSharp;
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
    public void ReadUtrechtKunstwerkenFileToDataFrame()
    {
        var file = "testfixtures/utrecht_kunstwerken.parquet";
        var file1 = new ParquetFileReader(file);
        // failed attempt to go to dataframe
        // next line gives error: Unsupported LogicalType Double?[]
        //var dataframe =file1.ToDataFrame();
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

            var geomColumnId = rowGroupReader.GetColumnId(geoParquet.Primary_column);

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
}

