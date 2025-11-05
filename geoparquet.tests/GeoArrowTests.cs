using NetTopologySuite.Geometries;
using ParquetSharp;
using ParquetSharp.Schema;

namespace GeoParquet.Tests;

public class GeoArrowTests
{
    [Test]
    public void ReadGeoParquetArrowPolygonFile()
    {
        var file = "testfixtures/gemeenten2016_arrow.parquet";
        var file1 = new ParquetFileReader(file);
        var geoParquet = file1.GetGeoMetadata();
        if (geoParquet != null)
        {
            Assert.That(geoParquet.Version == "1.0.0-beta.1");

            // geoarrow.multipolygon
            var rowGroupReader = file1.RowGroup(0);

            // todo: why is column 'xy' not specified?
            var geomColumnId = rowGroupReader.GetColumnId("xy");
            Assert.That(geoParquet.Columns.First().Value.Encoding == "geoarrow.multipolygon");

            if (geomColumnId != null)
            {
                var geometryArrow = rowGroupReader.Column((int)geomColumnId).LogicalReader<Double?[][][][]>().First();
                Assert.That(geometryArrow.Length == 1);
                Assert.That(geometryArrow[0].Length == 1);
                Assert.That(geometryArrow[0][0].Length == 165); //165 vertices
                Assert.That(geometryArrow[0][0][0].Length == 2); //2 points
                Assert.That(geometryArrow[0][0][0][0] == 6.8319922331647964); // longitude first vertice
                Assert.That(geometryArrow[0][0][0][1] == 53.327288101088072); // latitude first vertice
            }
        }
    }

    [Test]
    public void WriteGeoParquetGeoArrowFile()
    {
        var schema = GeoArrowSchemas.CreateGeoArrowPointSchema();

        var geoColumn = new GeoColumn();
        geoColumn.Encoding = "point";
        geoColumn.Geometry_types.Add("Point");
        var geometadata = GeoMetadata.GetGeoMetadata(geoColumn);

        var writerProperties = new WriterPropertiesBuilder().Build();
        var parquetFileWriter = new ParquetFileWriter(@"cities_arrow1.parquet", schema, writerProperties, keyValueMetadata: geometadata);
        var rowGroup = parquetFileWriter.AppendRowGroup();

        var nameWriter = rowGroup.NextColumn().LogicalWriter<String>();
        nameWriter.WriteBatch(new string[] { "London", "Derby" });

        var geom0 = new Point(5, 51);
        var geom1 = new Point(5.5, 51);
        
        // For struct-based Point geometry, write x and y as Nested<double> values
        var xWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>?>();
        xWriter.WriteBatch(new Nested<double>?[] { new Nested<double>(geom0.X), new Nested<double>(geom1.X) });
        
        var yWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>?>();
        yWriter.WriteBatch(new Nested<double>?[] { new Nested<double>(geom0.Y), new Nested<double>(geom1.Y) });
        
        parquetFileWriter.Close();
    }

    [Test]
    public void ReadGeoParquetGeoArrowPointFile()
    {
        var fileName = @"test_geoarrow_point_read.parquet";
        
        // First write a native Point file
        var schema = GeoArrowSchemas.CreateGeoArrowPointSchema();

        var geoColumn = new GeoColumn();
        geoColumn.Encoding = "point";
        geoColumn.Geometry_types.Add("Point");
        var geometadata = GeoMetadata.GetGeoMetadata(geoColumn);

        var writerProperties = new WriterPropertiesBuilder().Build();
        using (var parquetFileWriter = new ParquetFileWriter(fileName, schema, writerProperties, keyValueMetadata: geometadata))
        {
            using (var rowGroup = parquetFileWriter.AppendRowGroup())
            {
                using (var nameWriter = rowGroup.NextColumn().LogicalWriter<String>())
                {
                    nameWriter.WriteBatch(new string[] { "London", "Derby" });
                }

                // Write x coordinates as Nested<double>
                using (var xWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>?>())
                {
                    xWriter.WriteBatch(new Nested<double>?[] { new Nested<double>(5), new Nested<double>(5.5) });
                }
                    
                // Write y coordinates as Nested<double>
                using (var yWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>?>())
                {
                    yWriter.WriteBatch(new Nested<double>?[] { new Nested<double>(51), new Nested<double>(51) });
                }
            }
        }

        // Now read it back
        var file1 = new ParquetFileReader(fileName);
        var geoParquet = file1.GetGeoMetadata();
        Assert.That(geoParquet, Is.Not.Null);
        Assert.That(geoParquet!.Version == "1.1.0");
        Assert.That(geoParquet.Columns.First().Value.Encoding == "point");

        var rowGroupReader = file1.RowGroup(0);
            
        // Read city names
        var cityReader = rowGroupReader.Column(0).LogicalReader<string>();
        var cities = cityReader.ReadAll(2);
        Assert.That(cities[0] == "London");
        Assert.That(cities[1] == "Derby");

        // Read x coordinates
        var xReader = rowGroupReader.Column(1).LogicalReader<Nested<double>?>();
        var xs = xReader.ReadAll(2);
        Assert.That(xs.Length == 2);
        Assert.That(xs[0]!.Value.Value == 5);
        Assert.That(xs[1]!.Value.Value == 5.5);
            
        // Read y coordinates
        var yReader = rowGroupReader.Column(2).LogicalReader<Nested<double>?>();
        var ys = yReader.ReadAll(2);
        Assert.That(ys.Length == 2);
        Assert.That(ys[0]!.Value.Value == 51);
        Assert.That(ys[1]!.Value.Value == 51);
    }

    [Test]
    public void WriteGeoParquetGroupNodeFile()
    {
        var nameNode = new PrimitiveNode(
        "Name", Repetition.Optional, LogicalType.String(), PhysicalType.ByteArray);

        // but which logicaltype for double[]?
        var xyNode = new PrimitiveNode(
        "xy", Repetition.Optional, LogicalType.String(), PhysicalType.ByteArray);

        // Create a group node containing the two nested fields
        var groupNode = new GroupNode(
                "geometry", Repetition.Optional, new Node[] { nameNode });

        // Create the root schema group that contains all top-level columns
        var schema7 = new GroupNode(
                "schema8", Repetition.Required, new Node[] { nameNode });

        var propertiesBuilder = new WriterPropertiesBuilder();
        propertiesBuilder.Compression(Compression.Snappy);
        var writerProperties = propertiesBuilder.Build();
        var fileWriter = new ParquetFileWriter(@"objects.parquet", schema7, writerProperties);

        var messages = new Nested<string?>?[]
        {
            new Nested<string?>("London"),
            new Nested<string?>("Derby")
        };

        var rowGroupWriter = fileWriter.AppendRowGroup();

        var messagesWriter = rowGroupWriter.NextColumn().LogicalWriter<string>();
        messagesWriter.WriteBatch(new string[] { "London", "Derby" });

        fileWriter.Close();
    }

    [Test]
    public void WriteGeoParquetGeoArrowLineStringFile()
    {
        var schema = GeoArrowSchemas.CreateGeoArrowLineStringSchema();

        var geoColumn = new GeoColumn();
        geoColumn.Encoding = "linestring";
        geoColumn.Geometry_types.Add("LineString");
        var geometadata = GeoMetadata.GetGeoMetadata(geoColumn);

        var writerProperties = new WriterPropertiesBuilder().Build();
        var fileName = @"lines_geoarrow.parquet";
        using (var parquetFileWriter = new ParquetFileWriter(fileName, schema, writerProperties, keyValueMetadata: geometadata))
        {
            using (var rowGroup = parquetFileWriter.AppendRowGroup())
            {
                // Write names
                using (var nameWriter = rowGroup.NextColumn().LogicalWriter<String>())
                {
                    nameWriter.WriteBatch(new string[] { "Line1", "Line2" });
                }

                // Create LineString geometries
                // LineString 1: (0,0) -> (1,1) -> (2,0)
                var line1 = new LineString(new Coordinate[] {
                    new Coordinate(0, 0),
                    new Coordinate(1, 1),
                    new Coordinate(2, 0)
                });

                // LineString 2: (5,5) -> (6,6)
                var line2 = new LineString(new Coordinate[] {
                    new Coordinate(5, 5),
                    new Coordinate(6, 6)
                });

                // For LineString, we need to write the list of coordinates
                // Each row contains one LineString: Nested<double>[]
                // WriteBatch takes array of rows: Nested<double>[][]
                
                // Extract x coordinates
                var xCoords1 = line1.Coordinates.Select(c => new Nested<double>(c.X)).ToArray();
                var xCoords2 = line2.Coordinates.Select(c => new Nested<double>(c.X)).ToArray();
                
                using (var xWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>[]>())
                {
                    xWriter.WriteBatch(new Nested<double>[][] { xCoords1, xCoords2 });
                }

                // Extract y coordinates
                var yCoords1 = line1.Coordinates.Select(c => new Nested<double>(c.Y)).ToArray();
                var yCoords2 = line2.Coordinates.Select(c => new Nested<double>(c.Y)).ToArray();
                
                using (var yWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>[]>())
                {
                    yWriter.WriteBatch(new Nested<double>[][] { yCoords1, yCoords2 });
                }
            }
        }
    }

    [Test]
    public void WriteGeoParquetGeoArrowPolygonFile()
    {
        var schema = GeoArrowSchemas.CreateGeoArrowPolygonSchema();

        var geoColumn = new GeoColumn();
        geoColumn.Encoding = "polygon";
        geoColumn.Geometry_types.Add("Polygon");
        var geometadata = GeoMetadata.GetGeoMetadata(geoColumn);

        var writerProperties = new WriterPropertiesBuilder().Build();
        var fileName = @"polygons_geoarrow.parquet";
        using (var parquetFileWriter = new ParquetFileWriter(fileName, schema, writerProperties, keyValueMetadata: geometadata))
        {
            using (var rowGroup = parquetFileWriter.AppendRowGroup())
            {
                // Write names
                using (var nameWriter = rowGroup.NextColumn().LogicalWriter<String>())
                {
                    nameWriter.WriteBatch(new string[] { "Polygon1", "Polygon2" });
                }

                // Create Polygon geometries
                // Polygon 1: Triangle
                var polygon1 = new Polygon(new LinearRing(new Coordinate[] {
                    new Coordinate(0, 0),
                    new Coordinate(4, 0),
                    new Coordinate(2, 3),
                    new Coordinate(0, 0)  // closing coordinate
                }));

                // Polygon 2: Square
                var polygon2 = new Polygon(new LinearRing(new Coordinate[] {
                    new Coordinate(10, 10),
                    new Coordinate(14, 10),
                    new Coordinate(14, 14),
                    new Coordinate(10, 14),
                    new Coordinate(10, 10)  // closing coordinate
                }));

                // For Polygon, we need to write list of rings, each ring is a list of coordinates
                // Each row contains one Polygon: Nested<double>[][] (rings x coordinates)
                // WriteBatch takes array of rows: Nested<double>[][][] (rows x rings x coordinates)
                
                // Extract x coordinates for polygon1 - it has 1 ring (exterior ring)
                var polygon1XRing = polygon1.ExteriorRing.Coordinates.Select(c => new Nested<double>(c.X)).ToArray();
                
                // Extract x coordinates for polygon2 - it has 1 ring (exterior ring)
                var polygon2XRing = polygon2.ExteriorRing.Coordinates.Select(c => new Nested<double>(c.X)).ToArray();
                
                using (var xWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>[][]>())
                {
                    xWriter.WriteBatch(new Nested<double>[][][] { 
                        new Nested<double>[][] { polygon1XRing },  // Polygon 1 with 1 ring
                        new Nested<double>[][] { polygon2XRing }   // Polygon 2 with 1 ring
                    });
                }

                // Extract y coordinates
                var polygon1YRing = polygon1.ExteriorRing.Coordinates.Select(c => new Nested<double>(c.Y)).ToArray();
                var polygon2YRing = polygon2.ExteriorRing.Coordinates.Select(c => new Nested<double>(c.Y)).ToArray();
                
                using (var yWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>[][]>())
                {
                    yWriter.WriteBatch(new Nested<double>[][][] { 
                        new Nested<double>[][] { polygon1YRing },  // Polygon 1 with 1 ring
                        new Nested<double>[][] { polygon2YRing }   // Polygon 2 with 1 ring
                    });
                }
            }
        }
    }

    [Test]
    public void ReadGeoParquetGeoArrowLineStringFile()
    {
        var fileName = @"test_geoarrow_linestring_read.parquet";
        
        // First write a LineString file
        var schema = GeoArrowSchemas.CreateGeoArrowLineStringSchema();

        var geoColumn = new GeoColumn();
        geoColumn.Encoding = "linestring";
        geoColumn.Geometry_types.Add("LineString");
        var geometadata = GeoMetadata.GetGeoMetadata(geoColumn);

        var writerProperties = new WriterPropertiesBuilder().Build();
        using (var parquetFileWriter = new ParquetFileWriter(fileName, schema, writerProperties, keyValueMetadata: geometadata))
        {
            using (var rowGroup = parquetFileWriter.AppendRowGroup())
            {
                // Write names
                using (var nameWriter = rowGroup.NextColumn().LogicalWriter<String>())
                {
                    nameWriter.WriteBatch(new string[] { "Line1" });
                }

                // Write x coordinates for LineString: (0,0) -> (1,1) -> (2,0)
                var xCoords = new Nested<double>[] {
                    new Nested<double>(0),
                    new Nested<double>(1),
                    new Nested<double>(2)
                };
                using (var xWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>[]>())
                {
                    xWriter.WriteBatch(new Nested<double>[][] { xCoords });
                }

                // Write y coordinates
                var yCoords = new Nested<double>[] {
                    new Nested<double>(0),
                    new Nested<double>(1),
                    new Nested<double>(0)
                };
                using (var yWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>[]>())
                {
                    yWriter.WriteBatch(new Nested<double>[][] { yCoords });
                }
            }
        }

        // Now read it back
        using (var file1 = new ParquetFileReader(fileName))
        {
            var geoParquet = file1.GetGeoMetadata();
            Assert.That(geoParquet, Is.Not.Null);
            Assert.That(geoParquet!.Version == "1.1.0");
            Assert.That(geoParquet.Columns.First().Value.Encoding == "linestring");

            var rowGroupReader = file1.RowGroup(0);
            
            // Read name
            var nameReader = rowGroupReader.Column(0).LogicalReader<string>();
            var names = nameReader.ReadAll(1);
            Assert.That(names[0] == "Line1");

            // Read coordinates - each row contains one LineString as Nested<double>[]
            var xReader = rowGroupReader.Column(1).LogicalReader<Nested<double>[]>();
            var xCoords2 = xReader.ReadAll(1);
            Assert.That(xCoords2.Length, Is.EqualTo(1));  // 1 row
            Assert.That(xCoords2[0].Length, Is.EqualTo(3));  // 3 coordinates in the linestring
            Assert.That(xCoords2[0][0].Value, Is.EqualTo(0));
            Assert.That(xCoords2[0][1].Value, Is.EqualTo(1));
            Assert.That(xCoords2[0][2].Value, Is.EqualTo(2));

            var yReader = rowGroupReader.Column(2).LogicalReader<Nested<double>[]>();
            var yCoords2 = yReader.ReadAll(1);
            Assert.That(yCoords2.Length, Is.EqualTo(1));  // 1 row
            Assert.That(yCoords2[0].Length, Is.EqualTo(3));  // 3 coordinates in the linestring
            Assert.That(yCoords2[0][0].Value, Is.EqualTo(0));
            Assert.That(yCoords2[0][1].Value, Is.EqualTo(1));
            Assert.That(yCoords2[0][2].Value, Is.EqualTo(0));
        }
    }

    [Test]
    public void ReadGeoParquetGeoArrowPolygonFile()
    {
        var fileName = @"test_geoarrow_polygon_read.parquet";
        
        // First write a Polygon file
        var schema = GeoArrowSchemas.CreateGeoArrowPolygonSchema();

        var geoColumn = new GeoColumn();
        geoColumn.Encoding = "polygon";
        geoColumn.Geometry_types.Add("Polygon");
        var geometadata = GeoMetadata.GetGeoMetadata(geoColumn);

        var writerProperties = new WriterPropertiesBuilder().Build();
        using (var parquetFileWriter = new ParquetFileWriter(fileName, schema, writerProperties, keyValueMetadata: geometadata))
        {
            using (var rowGroup = parquetFileWriter.AppendRowGroup())
            {
                // Write names
                using (var nameWriter = rowGroup.NextColumn().LogicalWriter<String>())
                {
                    nameWriter.WriteBatch(new string[] { "Triangle" });
                }

                // Write x coordinates for Polygon: Triangle (0,0) -> (4,0) -> (2,3) -> (0,0)
                var xRing = new Nested<double>[] {
                    new Nested<double>(0),
                    new Nested<double>(4),
                    new Nested<double>(2),
                    new Nested<double>(0)
                };
                using (var xWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>[][]>())
                {
                    xWriter.WriteBatch(new Nested<double>[][][] { new Nested<double>[][] { xRing } });
                }

                // Write y coordinates
                var yRing = new Nested<double>[] {
                    new Nested<double>(0),
                    new Nested<double>(0),
                    new Nested<double>(3),
                    new Nested<double>(0)
                };
                using (var yWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>[][]>())
                {
                    yWriter.WriteBatch(new Nested<double>[][][] { new Nested<double>[][] { yRing } });
                }
            }
        }

        // Now read it back
        using (var file1 = new ParquetFileReader(fileName))
        {
            var geoParquet = file1.GetGeoMetadata();
            Assert.That(geoParquet, Is.Not.Null);
            Assert.That(geoParquet!.Version == "1.1.0");
            Assert.That(geoParquet.Columns.First().Value.Encoding == "polygon");

            var rowGroupReader = file1.RowGroup(0);
            
            // Read name
            var nameReader = rowGroupReader.Column(0).LogicalReader<string>();
            var names = nameReader.ReadAll(1);
            Assert.That(names[0] == "Triangle");

            // Read coordinates - each row contains one Polygon as Nested<double>[][]
            var xReader = rowGroupReader.Column(1).LogicalReader<Nested<double>[][]>();
            var xCoords2 = xReader.ReadAll(1);
            Assert.That(xCoords2.Length, Is.EqualTo(1));  // 1 row
            Assert.That(xCoords2[0].Length, Is.EqualTo(1));  // 1 ring
            Assert.That(xCoords2[0][0].Length, Is.EqualTo(4));  // 4 vertices
            Assert.That(xCoords2[0][0][0].Value, Is.EqualTo(0));
            Assert.That(xCoords2[0][0][1].Value, Is.EqualTo(4));
            Assert.That(xCoords2[0][0][2].Value, Is.EqualTo(2));
            Assert.That(xCoords2[0][0][3].Value, Is.EqualTo(0));

            var yReader = rowGroupReader.Column(2).LogicalReader<Nested<double>[][]>();
            var yCoords2 = yReader.ReadAll(1);
            Assert.That(yCoords2.Length, Is.EqualTo(1));  // 1 row
            Assert.That(yCoords2[0].Length, Is.EqualTo(1));  // 1 ring
            Assert.That(yCoords2[0][0].Length, Is.EqualTo(4));  // 4 vertices
            Assert.That(yCoords2[0][0][0].Value, Is.EqualTo(0));
            Assert.That(yCoords2[0][0][1].Value, Is.EqualTo(0));
            Assert.That(yCoords2[0][0][2].Value, Is.EqualTo(3));
            Assert.That(yCoords2[0][0][3].Value, Is.EqualTo(0));
        }
    }

    [Test]
    public void WriteGeoParquetGeoArrowMultiPointFile()
    {
        var schema = GeoArrowSchemas.CreateGeoArrowMultiPointSchema();

        var geoColumn = new GeoColumn();
        geoColumn.Encoding = "multipoint";
        geoColumn.Geometry_types.Add("MultiPoint");
        var geometadata = GeoMetadata.GetGeoMetadata(geoColumn);

        var writerProperties = new WriterPropertiesBuilder().Build();
        var fileName = @"multipoints_geoarrow.parquet";
        using (var parquetFileWriter = new ParquetFileWriter(fileName, schema, writerProperties, keyValueMetadata: geometadata))
        {
            using (var rowGroup = parquetFileWriter.AppendRowGroup())
            {
                // Write names
                using (var nameWriter = rowGroup.NextColumn().LogicalWriter<String>())
                {
                    nameWriter.WriteBatch(new string[] { "MultiPoint1", "MultiPoint2" });
                }

                // Create MultiPoint geometries
                // MultiPoint 1: (0,0), (1,1), (2,0)
                var multiPoint1 = new MultiPoint(new Point[] {
                    new Point(0, 0),
                    new Point(1, 1),
                    new Point(2, 0)
                });

                // MultiPoint 2: (5,5), (6,6)
                var multiPoint2 = new MultiPoint(new Point[] {
                    new Point(5, 5),
                    new Point(6, 6)
                });

                // For MultiPoint, we need to write the list of points
                // Each row contains one MultiPoint: Nested<double>[]
                // WriteBatch takes array of rows: Nested<double>[][]
                
                // Extract x coordinates
                var xCoords1 = multiPoint1.Geometries.Cast<Point>().Select(p => new Nested<double>(p.X)).ToArray();
                var xCoords2 = multiPoint2.Geometries.Cast<Point>().Select(p => new Nested<double>(p.X)).ToArray();
                
                using (var xWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>[]>())
                {
                    xWriter.WriteBatch(new Nested<double>[][] { xCoords1, xCoords2 });
                }

                // Extract y coordinates
                var yCoords1 = multiPoint1.Geometries.Cast<Point>().Select(p => new Nested<double>(p.Y)).ToArray();
                var yCoords2 = multiPoint2.Geometries.Cast<Point>().Select(p => new Nested<double>(p.Y)).ToArray();
                
                using (var yWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>[]>())
                {
                    yWriter.WriteBatch(new Nested<double>[][] { yCoords1, yCoords2 });
                }
            }
        }
    }

    [Test]
    public void ReadGeoParquetGeoArrowMultiPointFile()
    {
        var fileName = @"test_geoarrow_multipoint_read.parquet";
        
        // First write a MultiPoint file
        var schema = GeoArrowSchemas.CreateGeoArrowMultiPointSchema();

        var geoColumn = new GeoColumn();
        geoColumn.Encoding = "multipoint";
        geoColumn.Geometry_types.Add("MultiPoint");
        var geometadata = GeoMetadata.GetGeoMetadata(geoColumn);

        var writerProperties = new WriterPropertiesBuilder().Build();
        using (var parquetFileWriter = new ParquetFileWriter(fileName, schema, writerProperties, keyValueMetadata: geometadata))
        {
            using (var rowGroup = parquetFileWriter.AppendRowGroup())
            {
                // Write names
                using (var nameWriter = rowGroup.NextColumn().LogicalWriter<String>())
                {
                    nameWriter.WriteBatch(new string[] { "MultiPoint1" });
                }

                // Write x coordinates for MultiPoint: (0,0), (1,1), (2,0)
                var xCoords = new Nested<double>[] {
                    new Nested<double>(0),
                    new Nested<double>(1),
                    new Nested<double>(2)
                };
                using (var xWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>[]>())
                {
                    xWriter.WriteBatch(new Nested<double>[][] { xCoords });
                }

                // Write y coordinates
                var yCoords = new Nested<double>[] {
                    new Nested<double>(0),
                    new Nested<double>(1),
                    new Nested<double>(0)
                };
                using (var yWriter = rowGroup.NextColumn().LogicalWriter<Nested<double>[]>())
                {
                    yWriter.WriteBatch(new Nested<double>[][] { yCoords });
                }
            }
        }

        // Now read it back
        using (var file1 = new ParquetFileReader(fileName))
        {
            var geoParquet = file1.GetGeoMetadata();
            Assert.That(geoParquet, Is.Not.Null);
            Assert.That(geoParquet!.Version == "1.1.0");
            Assert.That(geoParquet.Columns.First().Value.Encoding == "multipoint");

            var rowGroupReader = file1.RowGroup(0);
            
            // Read name
            var nameReader = rowGroupReader.Column(0).LogicalReader<string>();
            var names = nameReader.ReadAll(1);
            Assert.That(names[0] == "MultiPoint1");

            // Read coordinates - each row contains one MultiPoint as Nested<double>[]
            var xReader = rowGroupReader.Column(1).LogicalReader<Nested<double>[]>();
            var xCoords2 = xReader.ReadAll(1);
            Assert.That(xCoords2.Length, Is.EqualTo(1));  // 1 row
            Assert.That(xCoords2[0].Length, Is.EqualTo(3));  // 3 points in the multipoint
            Assert.That(xCoords2[0][0].Value, Is.EqualTo(0));
            Assert.That(xCoords2[0][1].Value, Is.EqualTo(1));
            Assert.That(xCoords2[0][2].Value, Is.EqualTo(2));

            var yReader = rowGroupReader.Column(2).LogicalReader<Nested<double>[]>();
            var yCoords2 = yReader.ReadAll(1);
            Assert.That(yCoords2.Length, Is.EqualTo(1));  // 1 row
            Assert.That(yCoords2[0].Length, Is.EqualTo(3));  // 3 points in the multipoint
            Assert.That(yCoords2[0][0].Value, Is.EqualTo(0));
            Assert.That(yCoords2[0][1].Value, Is.EqualTo(1));
            Assert.That(yCoords2[0][2].Value, Is.EqualTo(0));
        }
    }
}
