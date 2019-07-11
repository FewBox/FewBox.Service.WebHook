namespace FewBox.Service.WebHook.Model.Services
{
    public class DockerHubMessage
    {
        public string RepositoryName { get; set; }
        public string Tag { get; set; }
    }
}