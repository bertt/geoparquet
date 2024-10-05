namespace GeoParquet
{
    public partial class GeoColumn
    {

        [System.Text.Json.Serialization.JsonPropertyName("encoding")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^(WKB|point|linestring|polygon|multipoint|multilinestring|multipolygon)$")]
        public string Encoding { get; set; }


        [System.Text.Json.Serialization.JsonPropertyName("geometry_types")]
        [System.ComponentModel.DataAnnotations.Required]
        public ICollection<string> Geometry_types { get; set; } = new System.Collections.ObjectModel.Collection<string>();


        [System.Text.Json.Serialization.JsonPropertyName("edges")]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public Edges Edges { get; set; }


        [System.Text.Json.Serialization.JsonPropertyName("orientation")]
        public string Orientation { get; set; }


        [System.Text.Json.Serialization.JsonPropertyName("bbox")]
        public double[] Bbox { get; set; }


        [System.Text.Json.Serialization.JsonPropertyName("epoch")]
        public double Epoch { get; set; }


        [System.Text.Json.Serialization.JsonPropertyName("covering")]
        public Covering Covering { get; set; }



        private IDictionary<string, object> _additionalProperties;

        [System.Text.Json.Serialization.JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ?? (_additionalProperties = new System.Collections.Generic.Dictionary<string, object>()); }
            set { _additionalProperties = value; }
        }

    }
}