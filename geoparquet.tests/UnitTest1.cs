using Newtonsoft.Json.Linq;

namespace GeoParquet.Tests;

public class Tests
{
    [Test]
    public async Task Test1()
    {
        // 0] read the GeoParquet file
        var geoParquetHolder = await GeoParquetReader.Read("testfixtures/gemeenten2016.parquet");
        
        // 1] get the ParquetReader
        var parquetReader = geoParquetHolder.GeoParquetReader;
        var dataFields = parquetReader.Schema.GetDataFields();
        Assert.That(dataFields.Length == 36);
        var reader = parquetReader.OpenRowGroupReader(0);
        var firstColumn = await reader.ReadColumnAsync(dataFields[3]);
        Assert.That(firstColumn.Data.Length == 391);
        Assert.That((string)firstColumn.Data.GetValue(0) == "Appingedam");

        // 2] get the GeoParquet metadata
        var geoParquet = geoParquetHolder.GeoParquet;
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