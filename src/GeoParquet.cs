//----------------------
// <auto-generated>
//     Generated using the NJsonSchema v11.0.1.0 (Newtonsoft.Json v13.0.0.0) (http://NJsonSchema.org)
// </auto-generated>
//----------------------


namespace GeoParquet
{
    #pragma warning disable // Disable all warnings

    /// <summary>
    /// Parquet metadata included in the geo field.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "11.0.1.0 (Newtonsoft.Json v13.0.0.0)")]
    public partial class GeoParquet
    {

        [System.Text.Json.Serialization.JsonPropertyName("version")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Version { get; set; }


        [System.Text.Json.Serialization.JsonPropertyName("primary_column")]
        [System.ComponentModel.DataAnnotations.Required]
        public string Primary_column { get; set; }


        [System.Text.Json.Serialization.JsonPropertyName("columns")]
        [System.ComponentModel.DataAnnotations.Required]
        public System.Collections.Generic.IDictionary<string, Anonymous> Columns { get; set; } = new System.Collections.Generic.Dictionary<string, Anonymous>();



        private System.Collections.Generic.IDictionary<string, object> _additionalProperties;

        [System.Text.Json.Serialization.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>()); }
            set { _additionalProperties = value; }
        }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "11.0.1.0 (Newtonsoft.Json v13.0.0.0)")]
    public partial class Anonymous
    {

        [System.Text.Json.Serialization.JsonPropertyName("encoding")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^(WKB|point|linestring|polygon|multipoint|multilinestring|multipolygon)$")]
        public string Encoding { get; set; }


        [System.Text.Json.Serialization.JsonPropertyName("geometry_types")]
        [System.ComponentModel.DataAnnotations.Required]
        public System.Collections.Generic.ICollection<string> Geometry_types { get; set; } = new System.Collections.ObjectModel.Collection<string>();


        [System.Text.Json.Serialization.JsonPropertyName("edges")]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public Edges Edges { get; set; }


        [System.Text.Json.Serialization.JsonPropertyName("orientation")]
        public string Orientation { get; set; }


        [System.Text.Json.Serialization.JsonPropertyName("bbox")]
        public object Bbox { get; set; }


        [System.Text.Json.Serialization.JsonPropertyName("epoch")]
        public double Epoch { get; set; }


        [System.Text.Json.Serialization.JsonPropertyName("covering")]
        public Covering Covering { get; set; }



        private System.Collections.Generic.IDictionary<string, object> _additionalProperties;

        [System.Text.Json.Serialization.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>()); }
            set { _additionalProperties = value; }
        }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "11.0.1.0 (Newtonsoft.Json v13.0.0.0)")]
    public enum Edges
    {

        [System.Runtime.Serialization.EnumMember(Value = @"planar")]
        Planar = 0,


        [System.Runtime.Serialization.EnumMember(Value = @"spherical")]
        Spherical = 1,


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "11.0.1.0 (Newtonsoft.Json v13.0.0.0)")]
    public partial class Covering
    {

        [System.Text.Json.Serialization.JsonPropertyName("bbox")]
        [System.ComponentModel.DataAnnotations.Required]
        public Bbox Bbox { get; set; } = new Bbox();



        private System.Collections.Generic.IDictionary<string, object> _additionalProperties;

        [System.Text.Json.Serialization.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>()); }
            set { _additionalProperties = value; }
        }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "11.0.1.0 (Newtonsoft.Json v13.0.0.0)")]
    public partial class Bbox
    {

        [System.Text.Json.Serialization.JsonPropertyName("xmin")]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(2)]
        [System.ComponentModel.DataAnnotations.MaxLength(2)]
        public System.Tuple<string, object> Xmin { get; set; } = new System.Tuple<string, object>();


        [System.Text.Json.Serialization.JsonPropertyName("xmax")]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(2)]
        [System.ComponentModel.DataAnnotations.MaxLength(2)]
        public System.Tuple<string, object> Xmax { get; set; } = new System.Tuple<string, object>();


        [System.Text.Json.Serialization.JsonPropertyName("ymin")]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(2)]
        [System.ComponentModel.DataAnnotations.MaxLength(2)]
        public System.Tuple<string, object> Ymin { get; set; } = new System.Tuple<string, object>();


        [System.Text.Json.Serialization.JsonPropertyName("ymax")]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(2)]
        [System.ComponentModel.DataAnnotations.MaxLength(2)]
        public System.Tuple<string, object> Ymax { get; set; } = new System.Tuple<string, object>();



        private System.Collections.Generic.IDictionary<string, object> _additionalProperties;

        [System.Text.Json.Serialization.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>()); }
            set { _additionalProperties = value; }
        }

    }
}