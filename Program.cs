using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using MassTransit;

namespace GettingStarted
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(x =>
                    {
                        x.UsingInMemory((context, config) => config.ConfigureEndpoints(context));

                        x.AddRider(rider =>
                        {
                            rider.AddConsumer<KafkaMessageConsumer>(typeof(KafkaMessageConsumerDefinition));

                            rider.UsingKafka((context, k) =>
                            {
                                k.Host("host", h =>
                                {
                                    h.UseSasl(sasl =>
                                    {
                                        sasl.Username = "username";
                                        sasl.Password = "password";
                                        sasl.Mechanism = SaslMechanism.Plain;
                                    });

                                    h.UseSsl(s => s.EndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.Https);

                                });

                                k.SecurityProtocol = SecurityProtocol.SaslSsl;

                                k.TopicEndpoint<KafkaMessage>("my-topic", "my-kafka-consumer-2", e =>
                                {
                                    e.ConfigureConsumer<KafkaMessageConsumer>(context);
                                    e.AutoOffsetReset = AutoOffsetReset.Earliest;
                                    e.ConcurrentDeliveryLimit = 100; // Important to set for single-key topics, otherwise consumer will return 1 message per batch
                                    e.CheckpointMessageCount = 100;
                                    e.CheckpointInterval = TimeSpan.FromMinutes(1);
                                });

                            });
                        });
                    });
                });
    }
}
