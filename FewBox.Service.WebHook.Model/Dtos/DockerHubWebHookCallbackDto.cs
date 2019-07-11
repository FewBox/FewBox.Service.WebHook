namespace FewBox.Service.WebHook.Model.Dtos
{
    public class DockerHubWebHookCallbackDto
    {
        public string State { get; set; }
        public string Description { get; set; }
        public string Context { get; set; }
        public string Target_Url { get; set; }
    }
}