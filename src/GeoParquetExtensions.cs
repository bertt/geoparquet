using NetTopologySuite.Geometries;
using ParquetSharp;
using System.Text.Json;

namespace GeoParquet;
public static class GeoParquetExtensions
{
    public static string? GetGeoMetadataAsString(this ParquetFileReader parquetFileReader)
    {
        var metadata = parquetFileReader.FileMetaData.KeyValueMetadata;
        var geoMetaData = metadata.GetValueOrDefault("geo");
        return geoMetaData;
    }

    public static GeoParquet? GetGeoMetadata(this ParquetFileReader parquetFileReader)
    {
        var geoMetaData = parquetFileReader.GetGeoMetadataAsString();
        if(geoMetaData != null)
        {
            return JsonSerializer.Deserialize<GeoParquet>(geoMetaData);
        }
        return null;
    }

    public static int? GetColumnId(this RowGroupReader rowGroupReader, string columnName)
    {
        var numColumns = rowGroupReader.MetaData.NumColumns;

        for (var i = 0; i < numColumns; i++)
        {
            var name = rowGroupReader.MetaData.Schema.Column(i).Name;
            if (name == columnName)
            {
                return i;
            }
        }

        return null;
    }

    /// <summary>
    /// Writes an array of Point geometries to a GeoArrow point column.
    /// This method writes both X and Y coordinates as Nested&lt;double&gt; values.
    /// </summary>
    /// <param name="rowGroupWriter">The row group writer to write to.</param>
    /// <param name="points">Array of Point geometries to write.</param>
    public static void WriteGeoArrowPoints(this RowGroupWriter rowGroupWriter, Point[] points)
    {
        // Write x coordinates as Nested<double>
        using (var xWriter = rowGroupWriter.NextColumn().LogicalWriter<Nested<double>?>())
        {
            var xCoords = points.Select(p => (Nested<double>?)new Nested<double>(p.X)).ToArray();
            xWriter.WriteBatch(xCoords);
        }
        
        // Write y coordinates as Nested<double>
        using (var yWriter = rowGroupWriter.NextColumn().LogicalWriter<Nested<double>?>())
        {
            var yCoords = points.Select(p => (Nested<double>?)new Nested<double>(p.Y)).ToArray();
            yWriter.WriteBatch(yCoords);
        }
    }
}