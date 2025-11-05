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
    public static void WritePoints(this RowGroupWriter rowGroupWriter, Point[] points)
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

    /// <summary>
    /// Writes an array of LineString geometries to a GeoArrow linestring column.
    /// This method writes X and Y coordinates as arrays of Nested&lt;double&gt; values.
    /// </summary>
    /// <param name="rowGroupWriter">The row group writer to write to.</param>
    /// <param name="lineStrings">Array of LineString geometries to write.</param>
    public static void WriteLineStrings(this RowGroupWriter rowGroupWriter, LineString[] lineStrings)
    {
        // Write x coordinates - each LineString is an array of coordinates
        using (var xWriter = rowGroupWriter.NextColumn().LogicalWriter<Nested<double>[]>())
        {
            var xCoords = lineStrings.Select(line => 
                line.Coordinates.Select(c => new Nested<double>(c.X)).ToArray()
            ).ToArray();
            xWriter.WriteBatch(xCoords);
        }
        
        // Write y coordinates
        using (var yWriter = rowGroupWriter.NextColumn().LogicalWriter<Nested<double>[]>())
        {
            var yCoords = lineStrings.Select(line => 
                line.Coordinates.Select(c => new Nested<double>(c.Y)).ToArray()
            ).ToArray();
            yWriter.WriteBatch(yCoords);
        }
    }

    /// <summary>
    /// Writes an array of Polygon geometries to a GeoArrow polygon column.
    /// This method writes X and Y coordinates as arrays of rings (arrays of Nested&lt;double&gt; values).
    /// </summary>
    /// <param name="rowGroupWriter">The row group writer to write to.</param>
    /// <param name="polygons">Array of Polygon geometries to write.</param>
    public static void WritePolygons(this RowGroupWriter rowGroupWriter, Polygon[] polygons)
    {
        // Write x coordinates - each Polygon is an array of rings, each ring is an array of coordinates
        using (var xWriter = rowGroupWriter.NextColumn().LogicalWriter<Nested<double>[][]>())
        {
            var xCoords = polygons.Select(polygon => 
            {
                var rings = new List<Nested<double>[]>();
                // Add exterior ring
                rings.Add(polygon.ExteriorRing.Coordinates.Select(c => new Nested<double>(c.X)).ToArray());
                // Add interior rings (holes) if any
                for (int i = 0; i < polygon.NumInteriorRings; i++)
                {
                    rings.Add(polygon.GetInteriorRingN(i).Coordinates.Select(c => new Nested<double>(c.X)).ToArray());
                }
                return rings.ToArray();
            }).ToArray();
            xWriter.WriteBatch(xCoords);
        }
        
        // Write y coordinates
        using (var yWriter = rowGroupWriter.NextColumn().LogicalWriter<Nested<double>[][]>())
        {
            var yCoords = polygons.Select(polygon => 
            {
                var rings = new List<Nested<double>[]>();
                // Add exterior ring
                rings.Add(polygon.ExteriorRing.Coordinates.Select(c => new Nested<double>(c.Y)).ToArray());
                // Add interior rings (holes) if any
                for (int i = 0; i < polygon.NumInteriorRings; i++)
                {
                    rings.Add(polygon.GetInteriorRingN(i).Coordinates.Select(c => new Nested<double>(c.Y)).ToArray());
                }
                return rings.ToArray();
            }).ToArray();
            yWriter.WriteBatch(yCoords);
        }
    }
}