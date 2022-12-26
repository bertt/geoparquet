using Newtonsoft.Json.Linq;

namespace GeoParquet.Tests;

public class Tests
{
    [Test]
    public async Task Test1()
    {
        var reader = await GeoParquetReader.ReadGeoParquet("testfixtures/gemeenten2016.parquet");
        var dataFields = reader.GeoParquetReader.Schema.GetDataFields();
        Assert.IsTrue(dataFields.Length == 36);
        var geoParquetMetaData = reader.GeoParquetMetadata;
        Assert.IsTrue(geoParquetMetaData.Version == "0.4.0");
        Assert.IsTrue(geoParquetMetaData.Primary_column == "geometry");
        Assert.IsTrue(geoParquetMetaData.Columns.Count == 1);
        var geomColumn = (JObject)geoParquetMetaData.Columns.First().Value;
        Assert.IsTrue(geomColumn["encoding"].ToString() == "WKB");
        Assert.IsTrue(geomColumn["orientation"].ToString() == "counterclockwise");
        Assert.IsTrue(geomColumn["geometry_type"].ToString() == "MultiPolygon");
        var bbox = (JArray)geomColumn["bbox"];
        Assert.IsTrue(bbox.Count == 4);
    }
}