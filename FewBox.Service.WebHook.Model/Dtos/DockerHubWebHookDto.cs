using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FewBox.Service.WebHook.Model.Dtos
{
    public class DockerHubWebHookDto
    {
        public string Callback_Url { get; set; }
        public PushDataDto Push_Data { get; set; }
        public RepositoryDto Repository { get; set; }
    }

    public class PushDataDto
    {
        public IList<string> Images { get; set; }
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Pushed_At { get; set; }
        public string Pusher { get; set; }
        public string Tag { get; set; }
    }
    public class RepositoryDto
    {
        public int Comment_Count { get; set; }
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Date_Created { get; set; }
        public string Description { get; set; }
        public string Dockerfile { get; set; }
        public string Full_Description { get; set; }
        public bool Is_Official { get; set; }
        public bool Is_Private { get; set; }
        public bool Is_Trusted { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Owner { get; set; }
        public string Repo_Name { get; set; }
        public string Repo_Url { get; set; }
        public int Star_Count { get; set; }
        public string Status { get; set; }
    }

    public class DateTimeConverter : DateTimeConverterBase
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((DateTime)value - Epoch).TotalMilliseconds + "000");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) { return null; }
            return Epoch.AddMilliseconds((Int64)reader.Value);
        }
    }
}