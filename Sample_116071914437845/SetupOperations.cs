using System;
using System.Configuration;
using CodeSuperior.PipelineStyle;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Sample_116071914437845
{
    public static class SetupOperations
    {

        public static MessageCountDetails GetMessageCountDetails(string path)
        {
            return ConnectionString()
                .To(NamespaceManager.CreateFromConnectionString)
                .GetQueue(path)
                .MessageCountDetails;
        }
        public static void CreateQueueIfNecessary(string path, int msgTimeToLive)
        {
            DefaultQueueDescription(path, msgTimeToLive)
                .Do(CreateQueueIfNecessary);
        }
        private static void CreateQueueIfNecessary(QueueDescription queueDescription)
        {
            ConnectionString()
                .To(NamespaceManager.CreateFromConnectionString)
                .To(m => new
                {
                    m,
                    exists = m.QueueExists(queueDescription.Path)
                })
                .DoIf(a => !a.exists, a =>
                {
                    Console.WriteLine("Creating queue " + queueDescription.Path);
                    a.m.CreateQueue(queueDescription);
                }, a =>
                {
                    Console.WriteLine("Queue already exists");
                });
        }
        private const string ConnectionStringKey = "Microsoft.ServiceBus.ConnectionString";

        private static QueueDescription DefaultQueueDescription(string path, int msgTimeToLive)
        {           
            return new QueueDescription(path)
            {
                DefaultMessageTimeToLive = TimeSpan.FromMinutes(msgTimeToLive),
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10),
                RequiresSession = true,
                EnableDeadLetteringOnMessageExpiration = true
            };
        }

        public static string ConnectionString()
        {
            return ConnectionStringKey
                .To(s => ConfigurationManager.AppSettings[s]);
        }
    }
}