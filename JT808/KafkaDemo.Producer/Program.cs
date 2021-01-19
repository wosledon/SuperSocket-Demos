using System;
using Confluent.Kafka;

namespace KafkaDemo.Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ProducerConfig()
            {
                BootstrapServers = "127.0.0.1:9092"
            };

            Action<DeliveryReport<Null, string>> handler = r =>
            {
                Console.WriteLine(!r.Error.IsError
                    ? $"Delivered message to {r.TopicPartitionOffset}"
                    : $"Delivery Error: {r.Error.Reason}");
            };

            using (var p = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    for (var i = 1; i <= 10; i++)
                    {
                        p.Produce("test", new Message<Null, string>()
                        {
                            Value = $"My message: {i}"
                        }, handler);
                    }

                    p.Flush(TimeSpan.FromSeconds(10));
                }
                catch (ProduceException<Null, string> e)
                {
                    Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                }
            }

            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }
}
