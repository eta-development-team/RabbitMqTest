using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqWebReceiver
{
    public class RabbitMqBusReceiver : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private IHubContext<RabbitMqHub> _hubContext;
        public RabbitMqBusReceiver(IHubContext<RabbitMqHub> hubContext)
        {
            _hubContext = hubContext;
            InitRabbitMq();
        }

        private void InitRabbitMq()
        {
            var factory = new ConnectionFactory {HostName = "localhost"};

            // create connection  
            _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);
            _channel.QueueDeclare("demo.queue.log", false, false, false, null);
            _channel.QueueBind("demo.queue.log", "demo.exchange", "demo.queue.*", null);
            _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += (sender, args) => { };
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message  
                var content = System.Text.Encoding.UTF8.GetString(ea.Body);

                // handle the received message  
                File.WriteAllText(@"c:\temp\RabbitMq.txt", content);
                 _hubContext.Clients.All
                     .SendAsync("ReceiveMessage", content, cancellationToken: stoppingToken)
                     .GetAwaiter()
                     .GetResult();
                
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += (sender, args) => { };
            consumer.Registered += (sender, args) => { };
            consumer.Unregistered += (sender, args) => { };
            consumer.ConsumerCancelled += (sender, args) => { };

            _channel.BasicConsume("demo.queue.log", false, consumer);
            return Task.CompletedTask;
            ;
        }
    }

    public class RabbitMqHub : Hub
    {
        public async Task Send(string message, string userName)
        {
            await Clients.All.SendAsync("Receive", message, userName);
        }
    }
    
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddHostedService<RabbitMqBusReceiver>();
            services.AddControllersWithViews();
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<RabbitMqHub>("/rabbitMqHub");
                
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}