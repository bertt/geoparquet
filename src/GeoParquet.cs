namespace GeoParquet
{
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
        public IDictionary<string, GeoColumn> Columns { get; set; } = new Dictionary<string, GeoColumn>();

        private IDictionary<string, object> _additionalProperties;

        [System.Text.Json.Serialization.JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ?? (_additionalProperties = new Dictionary<string, object>()); }
            set { _additionalProperties = value; }
        }
    }
}