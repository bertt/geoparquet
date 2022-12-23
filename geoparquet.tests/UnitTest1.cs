using Newtonsoft.Json;
using Parquet;
using System.Runtime.InteropServices;

namespace geoparquet.tests;

public class Tests
{

    [Test]
    public async Task Test1()
    {
        var reader = await GeoParquetReader.ReadGeoParquet("testfixtures/gemeenten2016.parquet");
        var dataFields = reader.GeoParquetReader.Schema.GetDataFields();
        Assert.IsTrue(dataFields.Length == 36);
        var geoParquetMetaData = reader.GeoParquetMetadata;
        Assert.IsTrue(geoParquetMetaData.version == "0.4.0");
        Assert.IsTrue(geoParquetMetaData.primary_column == "geometry");
        Assert.IsTrue(geoParquetMetaData.columns.geometry.bbox.Length == 4);
        Assert.IsTrue(geoParquetMetaData.columns.geometry.geometry_type == "MultiPolygon");
        Assert.IsTrue(geoParquetMetaData.columns.geometry.orientation == "counterclockwise");
        Assert.IsTrue(geoParquetMetaData.columns.geometry.encoding == "WKB");
    }
}