# Testing GeoArrow LineString and Polygon Files in QGIS

This document explains how to verify that the GeoParquet files with LineString and Polygon geometries work correctly in QGIS.

## Generated Files

The following GeoParquet files with GeoArrow encoding have been created:

1. **lines_geoarrow.parquet** - Contains 2 LineString geometries:
   - Line1: (0,0) -> (1,1) -> (2,0)
   - Line2: (5,5) -> (6,6)

2. **polygons_geoarrow.parquet** - Contains 2 Polygon geometries:
   - Polygon1: Triangle with vertices at (0,0), (4,0), (2,3), (0,0)
   - Polygon2: Square with vertices at (10,10), (14,10), (14,14), (10,14), (10,10)

## GeoParquet Metadata

Both files contain proper GeoParquet metadata:

### LineString Metadata
```json
{
  "version": "1.1.0",
  "primary_column": "geometry",
  "columns": {
    "geometry": {
      "encoding": "linestring",
      "geometry_types": ["LineString"],
      "edges": "Planar"
    }
  }
}
```

### Polygon Metadata
```json
{
  "version": "1.1.0",
  "primary_column": "geometry",
  "columns": {
    "geometry": {
      "encoding": "polygon",
      "geometry_types": ["Polygon"],
      "edges": "Planar"
    }
  }
}
```

## Testing in QGIS

### Prerequisites
- QGIS 3.30 or later (which supports GeoParquet format)

### Steps to Verify

1. **Open QGIS**

2. **Add the LineString Layer**
   - Go to `Layer` → `Add Layer` → `Add Vector Layer`
   - Or use the Data Source Manager (Ctrl+L / Cmd+L)
   - Select the `lines_geoarrow.parquet` file
   - Click `Add`

3. **Verify LineString Layer**
   - The layer should appear in the Layers panel
   - You should see 2 line features displayed on the map
   - Open the attribute table to verify the "name" column contains "Line1" and "Line2"
   - Check the geometry type in layer properties - should show "LineString"

4. **Add the Polygon Layer**
   - Repeat steps 2-3 for `polygons_geoarrow.parquet`

5. **Verify Polygon Layer**
   - The layer should appear in the Layers panel
   - You should see 2 polygon features displayed on the map
   - One should be a triangle, the other a square
   - Open the attribute table to verify the "name" column contains "Polygon1" and "Polygon2"
   - Check the geometry type in layer properties - should show "Polygon"

6. **Visual Inspection**
   - The LineString layer should display:
     - A bent line from (0,0) through (1,1) to (2,0)
     - A short straight line from (5,5) to (6,6)
   - The Polygon layer should display:
     - A triangle 
     - A square (larger than the triangle)

7. **Additional Checks**
   - Use the Identify tool to click on each geometry and verify coordinates
   - Check that no errors are displayed in the QGIS message log
   - Verify that the geometries can be styled (e.g., change line width, fill color)
   - Test that spatial operations work (e.g., buffer, intersection)

## Expected Results

✅ Both files should load without errors
✅ Geometries should be displayed correctly on the map
✅ Attribute data should be readable
✅ Layer properties should show the correct geometry type
✅ All QGIS spatial operations should work normally

## File Locations

The test files are located in:
- `geoparquet.tests/bin/Debug/net8.0/lines_geoarrow.parquet`
- `geoparquet.tests/bin/Debug/net8.0/polygons_geoarrow.parquet`

## Schema Information

Both files use the GeoParquet native encoding with a struct-based geometry representation:

### LineString Schema
- Column 0: `name` (string) - Feature name
- Column 1: `x` (list of doubles) - X coordinates of the linestring
- Column 2: `y` (list of doubles) - Y coordinates of the linestring

### Polygon Schema
- Column 0: `name` (string) - Feature name  
- Column 1: `x` (list of lists of doubles) - X coordinates for each ring
- Column 2: `y` (list of lists of doubles) - Y coordinates for each ring

The geometry column is a struct containing the x and y fields, following the GeoParquet specification for native encoding.
