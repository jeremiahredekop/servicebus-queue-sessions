using System;
using System.Collections.Generic;
using System.Threading;
using CodeSuperior.PipelineStyle;
using Newtonsoft.Json;

namespace Sample_116071914437845
{

    class Program
    {
        
        static void Main(string[] args)
        {

            var path = "";

            
            var receiveId = 0;

            Func<Receiver> receiverFunc = () =>
            {
                receiveId++;
                return new Receiver(path, receiveId);
            };
            

            Dictionary<string, Receiver> receivers = new Dictionary<string, Receiver>();

            DisplayInstructions();

            while (true)
            {
                Thread.Sleep(2000); // ensure console color set back to white
                Console.ResetColor();

                var action = Console.ReadLine();

                if (action.Contains("init"))
                {
                    action.Split(' ')
                        .To(a => new
                        {
                            queueName = a[1],
                            msgTimeToLive = a[2].To(int.Parse)
                        })
                        .Do(a =>
                        {
                            path = a.queueName;
                            SetupOperations.CreateQueueIfNecessary(a.queueName, a.msgTimeToLive);
                        });
                }
                else if (action.Contains("post"))
                {
                    action.Split(' ')
                        .To(a => new
                        {
                            sessionName = a[0],
                            numberOfMessages = a[2].To(int.Parse),
                            publisher = new Publisher(path)
                        })
                        .Do(a => a.publisher.DumpMessages(a.numberOfMessages, a.sessionName));
                }
                else if (action.Contains("receive"))
                {
                    action.Split(' ')
                        .To(a => new
                        {
                            timeoutInMinutes = a[1].To(int.Parse),
                            newReceiver = receiverFunc()
                        })
                        .Do(a =>
                        {
                            receiverFunc().AcceptSession(a.timeoutInMinutes, receivers);
                        });
                }
                else if (action.Contains("process"))
                {
                    action.Split(' ')
                        .To(a => new
                        {
                            sessionName = a[1],
                            numberOfMessages = a[2].To(int.Parse)
                        })
                        .Do(a =>
                        {
                            receivers[a.sessionName].ProcessMessages(a.numberOfMessages);
                        });
                }
                else if (action.Contains("stats"))
                {
                    SetupOperations.GetMessageCountDetails(path)
                        .To(JsonConvert.SerializeObject)
                        .Do(Console.WriteLine);
                }
                else
                {
                    Console.WriteLine("Unknown command");
                }
            }
        }

        private static void DisplayInstructions()
        {
            Thread.Sleep(2000); // ensure console color set back to white
            Console.ResetColor();

            new[]
            {
                "Available Commands",
                "init <queueName> <msgTimeToLive>",
                "<sessionName> post <numberMessages>",
                "receive <numberOfMinutesForSessionLock>",
                "process <sessionName> <numberOfMessages>",
                "stats"
            }.Do(a => Array.ForEach(a, Console.WriteLine));

            Console.WriteLine();
            Console.WriteLine();
        }

    }
}
