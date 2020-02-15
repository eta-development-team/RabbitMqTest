using System;
using System.Text;
using System.Timers;
using  RabbitMQ.Client;

namespace RabbitMqSender
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var factory = new ConnectionFactory {HostName = "localhost"};
            using( var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "demo.queue.log",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                
                const string message = "Hello, RabbitMq!";
                var timer = new Timer
                {
                    Enabled = true,
                    Interval = 1000
                };

                timer.Elapsed += (sender, eventArgs) =>
                {
                    channel.BasicPublish
                    (
                        exchange: string.Empty,
                        routingKey: "demo.queue.log",
                        basicProperties: null,
                        body: Encoding.UTF8.GetBytes(message)
                    );
                };
                Console.ReadLine();
            }
        }
    }
}