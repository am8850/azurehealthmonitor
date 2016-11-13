using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers.Azure
{
    public class ServiceBusHelper
    {
        public static void CreateTopic(string topicPath)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.TopicExists(topicPath))
            {
                namespaceManager.CreateTopic(topicPath);
            }

        }

        public static async Task CreateTopicAsync(string topicPath)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.TopicExists(topicPath))
            {
                await namespaceManager.CreateTopicAsync(topicPath);
            }

        }

        public static void CreateSubscription(string topicName, string name, Filter filter = null)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.SubscriptionExists(topicName, name))
            {
                if (filter == null)
                    namespaceManager.CreateSubscription(topicName, name);
                else
                    namespaceManager.CreateSubscription(topicName, name, filter);
            }
        }

        public static async Task CreateSubscriptionAsync(string topicName, string name, Filter filter = null)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.SubscriptionExists(topicName, name))
            {
                await namespaceManager.CreateSubscriptionAsync(topicName, name);
            }
        }

        public static void SendMessage(string topicName, BrokeredMessage message)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            TopicClient Client = TopicClient.CreateFromConnectionString(connectionString, topicName);

            Client.Send(message);
        }

        public static async Task SendMessageAsync(string topicName, BrokeredMessage message)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            TopicClient Client = TopicClient.CreateFromConnectionString(connectionString, topicName);

            await Client.SendAsync(message);
        }

        public static void ReceiveMessage(string topicPath, string name, Action<BrokeredMessage> action)
        {
            string connectionString =
    CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            SubscriptionClient Client =
                SubscriptionClient.CreateFromConnectionString
                        (connectionString, topicPath, name);

            // Configure the callback options.
            OnMessageOptions options = new OnMessageOptions();
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

            Client.OnMessage((message) =>
            {
                try
                {
                    action(message);

                    // Remove message from subscription.
                    message.Complete();
                }
                catch (Exception)
                {
                    // Indicates a problem, unlock message in subscription.
                    message.Abandon();
                }
            }, options);
        }

        public static void ReceiveMessageAsync(string topicPath, string name, Action<BrokeredMessage> action)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            SubscriptionClient Client = SubscriptionClient.CreateFromConnectionString(connectionString, topicPath, name);

            // Configure the callback options.
            OnMessageOptions options = new OnMessageOptions();
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

            Client.OnMessageAsync(async m =>
            {
                bool shouldAbandon = false;
                try
                {
                    // asynchronouse processing of messages
                    //await ProcessMessageAsync(m);
                    action(m);

                    // complete if successful processing
                    await m.CompleteAsync();
                }
                catch (Exception ex)
                {
                    shouldAbandon = true;
                    Console.WriteLine(ex);
                }

                if (shouldAbandon)
                {
                    await m.AbandonAsync();
                }
            },
            options);

        }

    }
}
