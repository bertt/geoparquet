using ParquetSharp;
using ParquetSharp.Schema;

namespace GeoParquet;

public static class GeoArrowSchemas
{
    public static GroupNode CreateGeoArrowPointSchema()
    {
        // Create schema for native Point encoding as per GeoParquet spec
        // The geometry column should be a struct with "x" and "y" fields
        var cityNode = new PrimitiveNode(
            "city", Repetition.Optional, LogicalType.String(), PhysicalType.ByteArray);

        // For Point geometry, use separate x and y fields as per GeoParquet native encoding
        var xNode = new PrimitiveNode(
            "x", Repetition.Required, LogicalType.None(), PhysicalType.Double);
        var yNode = new PrimitiveNode(
            "y", Repetition.Required, LogicalType.None(), PhysicalType.Double);

        // The geometry as a struct containing x and y
        var geometryNode = new GroupNode(
            "geometry", Repetition.Optional, new Node[] { xNode, yNode });

        // Root schema
        return new GroupNode(
            "schema", Repetition.Required, new Node[] { cityNode, geometryNode });
    }

    public static GroupNode CreateGeoArrowLineStringSchema()
    {
        // Create schema for LineString encoding as per GeoParquet spec
        // LineString is a list of coordinates (x, y pairs)
        var nameNode = new PrimitiveNode(
            "name", Repetition.Optional, LogicalType.String(), PhysicalType.ByteArray);

        // Create the coordinate struct with x and y
        var xNode = new PrimitiveNode(
            "x", Repetition.Required, LogicalType.None(), PhysicalType.Double);
        var yNode = new PrimitiveNode(
            "y", Repetition.Required, LogicalType.None(), PhysicalType.Double);
        
        var xyNode = new GroupNode(
            "xy", Repetition.Required, new Node[] { xNode, yNode });

        // LineString is a list of xy coordinates
        var listNode = new GroupNode(
            "list", Repetition.Repeated, new Node[] { xyNode });
        
        var geometryNode = new GroupNode(
            "geometry", Repetition.Optional, new Node[] { listNode }, LogicalType.List());

        // Root schema
        return new GroupNode(
            "schema", Repetition.Required, new Node[] { nameNode, geometryNode });
    }

    public static GroupNode CreateGeoArrowMultiPointSchema()
    {
        // Create schema for MultiPoint encoding as per GeoParquet spec
        // MultiPoint is a list of points (x, y pairs)
        var nameNode = new PrimitiveNode(
            "name", Repetition.Optional, LogicalType.String(), PhysicalType.ByteArray);

        // Create the coordinate struct with x and y
        var xNode = new PrimitiveNode(
            "x", Repetition.Required, LogicalType.None(), PhysicalType.Double);
        var yNode = new PrimitiveNode(
            "y", Repetition.Required, LogicalType.None(), PhysicalType.Double);
        
        var xyNode = new GroupNode(
            "xy", Repetition.Required, new Node[] { xNode, yNode });

        // MultiPoint is a list of xy coordinates
        var listNode = new GroupNode(
            "list", Repetition.Repeated, new Node[] { xyNode });
        
        var geometryNode = new GroupNode(
            "geometry", Repetition.Optional, new Node[] { listNode }, LogicalType.List());

        // Root schema
        return new GroupNode(
            "schema", Repetition.Required, new Node[] { nameNode, geometryNode });
    }

    public static GroupNode CreateGeoArrowPolygonSchema()
    {
        // Create schema for Polygon encoding as per GeoParquet spec
        // Polygon is a list of rings, where each ring is a list of coordinates
        var nameNode = new PrimitiveNode(
            "name", Repetition.Optional, LogicalType.String(), PhysicalType.ByteArray);

        // Create the coordinate struct with x and y
        var xNode = new PrimitiveNode(
            "x", Repetition.Required, LogicalType.None(), PhysicalType.Double);
        var yNode = new PrimitiveNode(
            "y", Repetition.Required, LogicalType.None(), PhysicalType.Double);
        
        var xyNode = new GroupNode(
            "xy", Repetition.Required, new Node[] { xNode, yNode });

        // A ring is a list of xy coordinates
        var ringListNode = new GroupNode(
            "list", Repetition.Repeated, new Node[] { xyNode });
        
        var ringNode = new GroupNode(
            "element", Repetition.Required, new Node[] { ringListNode }, LogicalType.List());

        // Polygon is a list of rings
        var polygonListNode = new GroupNode(
            "list", Repetition.Repeated, new Node[] { ringNode });
        
        var geometryNode = new GroupNode(
            "geometry", Repetition.Optional, new Node[] { polygonListNode }, LogicalType.List());

        // Root schema
        return new GroupNode(
            "schema", Repetition.Required, new Node[] { nameNode, geometryNode });
    }
}
