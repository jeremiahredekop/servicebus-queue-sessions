using System;
using System.Collections.Generic;
using System.Linq;
using CodeSuperior.PipelineStyle;
using Microsoft.ServiceBus.Messaging;

namespace Sample_116071914437845
{
    public class Receiver
    {
        private readonly int _identifier;
        private readonly QueueClient _client;
        private MessageSession _session;

        public Receiver(string path, int identifier)
        {
            _identifier = identifier;
            _client = SetupOperations.ConnectionString()
                .To(s=> QueueClient.CreateFromConnectionString(s, path));
        }

        public void AcceptSession(int timeInMinutes, Dictionary<string, Receiver> receivers)
        {
            _session = _client.AcceptMessageSession(TimeSpan.FromMinutes(timeInMinutes));
            receivers[_session.SessionId] = this;
            Console.WriteLine(String.Format("ID {0}#: Session Acquired: {1}", _identifier, _session.SessionId));
        }

        public void ProcessMessages(int numberOfMessages)
        {
            Console.WriteLine(String.Format("Session {0}: Processing {1} messages:", _session.SessionId, numberOfMessages));
            _session.ReceiveBatch(numberOfMessages)
                .Select(m => m.LockToken)
                .ToList()
                .DoIf(l=> l.Any(),l =>
                {
                    _session.CompleteBatch(l);
                        Console.WriteLine(String.Format("Session {0}: {1} messages Processed", _session.SessionId, l.Count));
                }, l =>
                {
                    Console.WriteLine(String.Format("No messages available to process for session {0}",
                        _session.SessionId));
                });
            
        }
    }
}