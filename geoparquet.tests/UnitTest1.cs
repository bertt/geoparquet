using Apache.Arrow.Ipc;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json.Linq;
using Parquet;
using Parquet.Data;

namespace GeoParquet.Tests;

public class Tests
{
    [Test]
    public async Task ReaderParquet04Test()
    {
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
        var firstCoordinate = multiPolygon.Coordinates.First();
        Assert.That(firstCoordinate.CoordinateValue.X == 6.8319922331647964);
        Assert.That(firstCoordinate.CoordinateValue.Y == 53.327288101088072);
    }

    [Test]
    public async Task ReadGeoParquet04WithArrowEncodingFile()
    {
        var file = "testfixtures/gemeenten2016_0.4_arrow.parquet";
        var fileStream = File.OpenRead(file);
        var parquetReader = await ParquetReader.CreateAsync(fileStream);
        Assert.IsTrue(parquetReader.ThriftMetadata.Created_by == "GDAL 3.6.1, using parquet-cpp-arrow version 10.0.1");
        var reader = parquetReader.OpenRowGroupReader(0);
        var dataFields = parquetReader.Schema.GetDataFields();
        var gmNaamColumn = await reader.ReadColumnAsync(dataFields[3]);
        Assert.IsTrue(gmNaamColumn.Count == 391);
        var geometryColumn = await reader.ReadColumnAsync(dataFields[35]);
        var data = geometryColumn.Data;
        Assert.IsTrue(geometryColumn.Count == 205834);

        var firstX = (double)geometryColumn.Data.GetValue(0);
        Assert.That(firstX == 6.8319922331647964);
        var firstY = (double)geometryColumn.Data.GetValue(1);
        Assert.That(firstY == 53.327288101088072);

        var arrowReader = new ArrowFileReader(fileStream);
        // var recordBatch = await arrowReader.ReadNextRecordBatchAsync();
    }


    [Test]
    public async Task ReadGeoParquet10File()
    {
        var file = "testfixtures/gemeenten2016_1.0.parquet";
        var fileStream = File.OpenRead(file);

        var parquetReader = await ParquetReader.CreateAsync(fileStream);
        var reader = parquetReader.OpenRowGroupReader(0);
        var dataFields = parquetReader.Schema.GetDataFields();
        // next line fails??
        // var nameColumn = await reader.ReadColumnAsync(dataFields[33]);
    }


    [Test]
    public async Task TestWriteGeoParquetFile()
    {
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

                parquetWriter.SetGeoMetadata("Point", new double[] {  3.3583782525105832,
                  50.750367484598314,
                  7.2274984508458306,
                  53.555014517907608});
            }
        }
    }
}

