using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;

namespace GettingStarted
{
    class KafkaMessageConsumer :
        IConsumer<Batch<KafkaMessage>>
    {
        public async Task Consume(ConsumeContext<Batch<KafkaMessage>> context)
        {
            Console.WriteLine("Messages in batch: " + context.Message.Length);

            for (int i = 0; i < context.Message.Length; i++)
            {
                ConsumeContext<KafkaMessage> audit = context.Message[i];
                Console.WriteLine("Guid: " + audit.Message.Id + " ThreadId: " + Thread.CurrentThread.ManagedThreadId);
            }
        }
    }

    class KafkaMessageConsumerDefinition :
        ConsumerDefinition<KafkaMessageConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<KafkaMessageConsumer> consumerConfigurator)
        {
            consumerConfigurator.Options<BatchOptions>(options =>
                {
                    options
                    .SetTimeLimit(100)
                    .SetMessageLimit(100)
                    .SetConcurrencyLimit(10);
                }
            );
        }
    }
}
