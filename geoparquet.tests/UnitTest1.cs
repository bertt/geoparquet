using Newtonsoft.Json.Linq;

namespace GeoParquet.Tests;

public class Tests
{
    [Test]
    public async Task Test1()
    {
        var reader = await GeoParquetReader.ReadGeoParquet("testfixtures/gemeenten2016.parquet");
        var dataFields = reader.GeoParquetReader.Schema.GetDataFields();
        Assert.That(dataFields.Length == 36);
        var geoParquet = reader.GeoParquet;
        Assert.That(geoParquet.Version == "0.4.0");
        Assert.That(geoParquet.Primary_column == "geometry");
        Assert.That(geoParquet.Columns.Count == 1);
        var geomColumn = (JObject)geoParquet.Columns.First().Value;
        Assert.That(geomColumn?["encoding"].ToString() == "WKB");
        Assert.That(geomColumn?["orientation"].ToString() == "counterclockwise");
        Assert.That(geomColumn?["geometry_type"].ToString() == "MultiPolygon");
        var bbox = (JArray)geomColumn?["bbox"];
        Assert.That(bbox?.Count == 4);
    }
}