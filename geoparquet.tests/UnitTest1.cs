using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using ParquetSharp;

namespace GeoParquet.Tests;

public class Tests
{
    [Test]
    public void ReadGeoParquetFile()
    {
        var file = "testfixtures/gemeenten2016_1.0.parquet";
        var file1 = new ParquetFileReader(file);

        var geoParquet = file1.GetGeoMetadata();
        Assert.That(geoParquet.Version == "1.0.0-beta.1");

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

