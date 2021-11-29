using Azure.Identity;

namespace SPO.ColdStorage.Migration.Engine
{
    public class SharePointDiscovery
    {
        public SharePointDiscovery(ClientSecretCredential clientSecretCredential)
        {
            this.ClientSecretCredential = clientSecretCredential;
        }
        public ClientSecretCredential ClientSecretCredential { get; set; }

        public Task StartAsync()
        {
            throw new NotImplementedException();
        }
    }
}