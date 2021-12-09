namespace SPO.ColdStorage.Web.Models
{
    public class StorageInfo
    {
        public string SharedAccessToken { get; set; } = string.Empty;
        public string AccountURI { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
    }
}
