using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json.Linq;
using ParquetSharp;

namespace GeoParquet.Tests;

public class Tests
{
    [Test]
    public void ReadArrowPointsTest()
    {
        var file1 = new ParquetFileReader("testfixtures/region_points.parquet");
        var rowGroupReader = file1.RowGroup(0);
        var geoParquet = file1.GetGeoMetadata();
        var geomColumn = (JObject)geoParquet.Columns.First().Value;
        Assert.That(geomColumn?["encoding"].ToString() == "geoarrow.point");
        var arry = new double?[235][];
    }

    [Test]
    public void ReaderParquetSharp04ArrowTest()
    {
        var firstArrowGeom = GetFirstArrowGeom("testfixtures/gemeenten2016_0.4_arrow.parquet");
        var firstWkbGeom = GetFirstWkbGeom("testfixtures/gemeenten2016_0.4.parquet");
        var wkbReader = new WKBReader();
        var multiPolygon = (MultiPolygon)wkbReader.Read(firstWkbGeom);

        Assert.That(firstArrowGeom[0][0].Length == 165);
    }

    private static double?[][][][] GetFirstArrowGeom(string file)
    {
        var file1 = new ParquetFileReader(file);
        var rowGroupReader = file1.RowGroup(0);
        var groupNumRows = (int)rowGroupReader.MetaData.NumRows;
        var geometryColumn = rowGroupReader.Column(35).LogicalReader<Double?[][][][]>().ReadAll(groupNumRows);
        var firstArrowGeom = geometryColumn[0];
        return firstArrowGeom;
    }

    private static byte[] GetFirstWkbGeom(string file)
    {
        var file1 = new ParquetFileReader(file);
        var rowGroupReader = file1.RowGroup(0);
        var groupNumRows = (int)rowGroupReader.MetaData.NumRows;
        var geometryColumn = rowGroupReader.Column(35).LogicalReader<byte[]>().ReadAll(groupNumRows);
        var firstWkbGeom = geometryColumn[0];
        return firstWkbGeom;
    }


    [Test]
    public void ReaderParquetSharp04Test()
    {
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
    }

    [Test]
    public void ReadGeoParquetSharp10File()
    {
        var file = "testfixtures/gemeenten2016_1.0.parquet";
        var file1 = new ParquetFileReader(file);
        var rowGroupReader = file1.RowGroup(0);

        var gemName = rowGroupReader.Column(33).LogicalReader<String>().First();
        Assert.IsTrue(gemName == "Appingedam");

        var geometryWkb = rowGroupReader.Column(17).LogicalReader<byte[]>().First();
        var wkbReader = new WKBReader();
        var multiPolygon = (MultiPolygon)wkbReader.Read(geometryWkb);
        Assert.That(multiPolygon.Coordinates.Count() == 165);
        var firstCoordinate = multiPolygon.Coordinates.First();
        Assert.That(firstCoordinate.CoordinateValue.X == 6.8319922331647964);
        Assert.That(firstCoordinate.CoordinateValue.Y == 53.327288101088072);
    }

    [Test]
    public void TestWriteGeoParquetWkbFile()
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
    }


    [Test]
    // Attempt to write a GeoParquet arrow file with 2 points.
    // The file gets created but in QGIS nothing is visualized :-(
    // Probably because the type of the geometry column must be different
    public void TestWriteGeoParquetArrowFile()
    {
        var columns = new Column[]
        {
            new Column<string>("city"),
            new Column<Double?[]>("geometry")
        };

        var bbox = new double[] {  3.3583782525105832,
                  50.750367484598314,
                  7.2274984508458306,
                  53.555014517907608};

        var geometadata = GeoMetadata.GetGeoMetadata("Point", bbox, encoding: "geoarrow.point");

        var parquetFileWriter = new ParquetFileWriter(@"d:\aaa\writing_sample_arrow.parquet", columns, Compression.Snappy, geometadata);
        var rowGroup = parquetFileWriter.AppendRowGroup();

        var nameWriter = rowGroup.NextColumn().LogicalWriter<String>();
        nameWriter.WriteBatch(new string[] { "London", "Derby" });

        var geom0 = new Point(5.1, 51.1);
        var geom1 = new Point(5.5, 51.1);

        // https://github.com/geoarrow/geoarrow/blob/main/format.md
        var points = new double?[2][];
        points[0] = new double?[2] {geom0.X, geom0.Y};
        points[1] = new double?[2] { geom1.X, geom1.Y };

        var geometryWriter = rowGroup.NextColumn().LogicalWriter<Double?[]> ();
        geometryWriter.WriteBatch(points);
        parquetFileWriter.Close();
    }
}

