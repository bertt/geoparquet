namespace GeoParquet
{
    public partial class Covering
    {

        [System.Text.Json.Serialization.JsonPropertyName("bbox")]
        [System.ComponentModel.DataAnnotations.Required]
        public Bbox Bbox { get; set; } = new Bbox();

        private IDictionary<string, object> _additionalProperties;

        [System.Text.Json.Serialization.JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ?? (_additionalProperties = new Dictionary<string, object>()); }
            set { _additionalProperties = value; }
        }
    }
}