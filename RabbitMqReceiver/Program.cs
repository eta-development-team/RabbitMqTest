using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory{HostName = "localhost"};

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "demo.queue.log",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (sender, eventArgs) =>
                    {
                        var message = Encoding.UTF8.GetString(eventArgs.Body);

                        Console.WriteLine(message);
                    };

                    channel.BasicConsume(queue: "demo.queue.log",
                        autoAck: true,
                        consumer: consumer);


                    Console.WriteLine("Hello World!");
                    Console.ReadLine();
                }
            }
        }
    }
}