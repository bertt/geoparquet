using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using ParquetSharp;

namespace geoarrow;
public class GeoArrowReader
{
    /// <summary>
    /// Converts GeoArrow points array to NTS FeatureCollection
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public FeatureCollection Read(LogicalColumnReader<double?[]> points)
    {
        var featureCollection = new FeatureCollection();

        GeometryFactory factory = new GeometryFactory();
        foreach (var point in points)
        {
            var coordinate = new Coordinate(point[0].Value, point[1].Value);
            var pnt = factory.CreatePoint(coordinate);
            var feature = new Feature(pnt, new AttributesTable());
            // todo: fill attributes
            featureCollection.Add(feature);
        }
        return featureCollection;
    }

    // Todo read other geometry types
}
