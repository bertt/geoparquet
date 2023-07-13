using Apache.Arrow.Ipc;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using ParquetSharp;

namespace GeoParquet.Tests;

public class Tests
{
    [Test]
    public async Task ReadArrowFile()
    {
        var file = "testfixtures/gemeenten2016.arrow";
        var stream = File.OpenRead(file);
        var reader = new ArrowFileReader(stream);
        // Next line gives: FixedSizeList is unsupported...
        var recordBatch = await reader.ReadNextRecordBatchAsync();
    }


    [Test]
    public void ReadGeoParquetArrowFile()
    {
        var file = "testfixtures/gemeenten2016_arrow.parquet";
        var file1 = new ParquetFileReader(file);
        var geoParquet = file1.GetGeoMetadata();
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

    [Test]
    public void ReadGeoParquetWkbFile()
    {
        var file = "testfixtures/gemeenten2016_wkb.parquet";
        var file1 = new ParquetFileReader(file);

        var geoParquet = file1.GetGeoMetadata();
        Assert.That(geoParquet.Version == "1.0.0-beta.1");
        Assert.That(geoParquet.Columns.First().Value.Encoding == "WKB");

        var rowGroupReader = file1.RowGroup(0);
        var gemName = rowGroupReader.Column(3).LogicalReader<String>().First();
        Assert.IsTrue(gemName == "Appingedam");

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

    [Test]
    public void WriteGeoParquetWkbFile()
    {
        var columns = new Column[]
        {
            new Column<string>("city"),
            new Column<byte[]>("geometry")
        };

        var bbox = new double[] {  3.3583782525105832,
                  50.750367484598314,
                  7.2274984508458306,
                  53.555014517907608};

        var geoColumn = new GeoColumn();
        geoColumn.Bbox = bbox;
        geoColumn.Encoding = "WKB";
        geoColumn.Geometry_types.Add("Point");
        var geometadata = GeoMetadata.GetGeoMetadata(geoColumn);

        var parquetFileWriter = new ParquetFileWriter(@"writing_sample11.parquet", columns, keyValueMetadata: geometadata);
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
    }
}

