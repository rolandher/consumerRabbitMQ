using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() 
        { 
            HostName = "localhost",
            Port = 15672,
            UserName = "guest",
            Password = "guest"
        };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Exchange Direct
            channel.ExchangeDeclare("direct_exchange", ExchangeType.Direct);
            channel.QueueDeclare("direct_queue", durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind("direct_queue", "direct_exchange", "routing_key");

            // Event handler for direct messages
            var directConsumer = new EventingBasicConsumer(channel);
            directConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received from direct exchange: {message}");
            };
            channel.BasicConsume("direct_queue", autoAck: true, directConsumer);

            // Exchange Topic
            channel.ExchangeDeclare("topic_exchange", ExchangeType.Topic);
            channel.QueueDeclare("topic_queue", durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind("topic_queue", "topic_exchange", "app.example");

            // Event handler for topic messages
            var topicConsumer = new EventingBasicConsumer(channel);
            topicConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received from topic exchange: {message}");
            };
            channel.BasicConsume("topic_queue", autoAck: true, topicConsumer);

            // Exchange Fanout
            channel.ExchangeDeclare("fanout_exchange", ExchangeType.Fanout);
            channel.QueueDeclare("fanout_queue", durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind("fanout_queue", "fanout_exchange", "");

            // Event handler for fanout messages
            var fanoutConsumer = new EventingBasicConsumer(channel);
            fanoutConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received from fanout exchange: {message}");
            };
            channel.BasicConsume("fanout_queue", autoAck: true, fanoutConsumer);

            Console.WriteLine("Waiting for messages. Press [Enter] to exit.");
            Console.ReadLine();
        }
    }
}
