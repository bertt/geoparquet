using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json.Linq;
using Parquet;

namespace GeoParquet.Tests;

public class Tests
{
    [Test]
    public async Task Test1()
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
}