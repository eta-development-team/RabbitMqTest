using System;
using System.Text;
using  RabbitMQ.Client;

namespace RabbitMqSender
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var factory = new ConnectionFactory {HostName = "localhost"};
            using var connection = factory.CreateConnection();
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "demo.queue.log",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                
                var message = "Hello, RabbitMq!";
                channel.BasicPublish
                (
                    exchange: string.Empty,
                    routingKey: "demo.queue.log",
                    basicProperties: null,
                    body: Encoding.UTF8.GetBytes(message)
                );
            }
        }
    }
}