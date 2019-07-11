using System;
using System.Collections.Generic;
using System.Text;
using FewBox.Core.Utility.Formatter;
using FewBox.Core.Utility.Net;
using FewBox.Core.Web.Dto;
using FewBox.Core.Web.Filter;
using FewBox.Service.WebHook.Model.Configs;
using FewBox.Service.WebHook.Model.Dtos;
using FewBox.Service.WebHook.Model.Services;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FewBox.Service.WebHook.Controllers
{
    [Route("api/[controller]")]
    public class WebHooksController : ControllerBase
    {
        private RabbitMQConfig RabbitMQConfig { get; set; }
        public WebHooksController(RabbitMQConfig rabbitMQConfig)
        {
            this.RabbitMQConfig = rabbitMQConfig;
        }

        [HttpPost("dockerhub")]
        //[Trace]
        public MetaResponseDto DockerHub([FromBody]DockerHubWebHookDto dockerHubWebHookDto)
        {
            var factory = new ConnectionFactory() { HostName = this.RabbitMQConfig.HostName };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: this.RabbitMQConfig.Queue,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string message = JsonUtility.Serialize<DockerHubMessage>(new DockerHubMessage{
                    RepositoryName = dockerHubWebHookDto.Repository.Repo_Name,
                    Tag = dockerHubWebHookDto.Push_Data.Tag
                });
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: this.RabbitMQConfig.Queue,
                                     basicProperties: null,
                                     body: body);
            }
            /*RestfulUtility.Post<DockerHubWebHookCallbackDto, dynamic>(dockerHubWebHookDto.Callback_Url, new Package<DockerHubWebHookCallbackDto>{
                Body = new DockerHubWebHookCallbackDto{
                    State = "success",
                    Description = $"Latest version {dockerHubWebHookDto.Repository.Namespace}/{dockerHubWebHookDto.Repository.Name}-{dockerHubWebHookDto.Push_Data.Tag} DEPLOYED",
                    Context = "Continuous Delivery by FewBox",
                    Target_Url = "https://mesh.fewbox.com/"
                },
                Headers = new List<Header>{}
            });*/
            return new MetaResponseDto();
        }

        [HttpGet]
        public MetaResponseDto ConsumeDockerHub()
        {
            var factory = new ConnectionFactory() { HostName = this.RabbitMQConfig.HostName };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: this.RabbitMQConfig.Queue,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, deliverEventArgs) =>
                {
                    var body = deliverEventArgs.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message); // Todo: Need to implement in listener.
                };
                channel.BasicConsume(queue: this.RabbitMQConfig.Queue,
                                     autoAck: true,
                                     consumer: consumer);
                Console.ReadLine();
            }
            return new MetaResponseDto();
        }
    }
}