using Apache.Arrow.Ipc;

namespace GeoParquet.Tests;

public class ArrowTests
{
    [Test]
    public async Task ReadArrowPointFile()
    {
        var file = "testfixtures/gemeenten2016.arrow";
        using var stream = File.OpenRead(file);
        using var reader = new ArrowFileReader(stream);
        // Next line gives message about compression see geoarrow-cs for solution
        // var recordBatch = await reader.ReadNextRecordBatchAsync();
    }
}
