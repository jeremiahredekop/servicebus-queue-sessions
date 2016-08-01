using System;
using System.Linq;
using System.Threading.Tasks;
using CodeSuperior.PipelineStyle;
using Microsoft.ServiceBus.Messaging;

namespace Sample_116071914437845
{
    public class Publisher
    {
        private readonly QueueClient _client;
        private int _runCount;

        public Publisher(string path)
        {
            _client = SetupOperations.ConnectionString()
                .To(s=> QueueClient.CreateFromConnectionString(s, path));
            _runCount = 0;
        }

        public void DumpMessages(int totalNumberOfMessages, string sessionName)
        {
            _runCount++;

            Console.WriteLine(string.Format("Run #{0}: Sending {1} messages to session {2}", _runCount,
                totalNumberOfMessages, sessionName));

            Enumerable.Range(_runCount, totalNumberOfMessages)
                .Select(n => new MyTestMessage() {RunNumber = _runCount, MsgNumber = n})
                .Select(m => new BrokeredMessage(m){ SessionId = sessionName})
                .ToList()
                .Do(l => _client.SendBatch(l));

            Console.WriteLine("Messages sent");
        }
    }
}