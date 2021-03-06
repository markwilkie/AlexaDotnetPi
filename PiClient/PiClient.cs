﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Safern.Hub.Reader;
using Safern.Hub.Sender;
using Pi.IO;
using Pi.IO.GeneralPurpose;

namespace PiClient
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isRunning = true;

            CancellationTokenSource cts = new CancellationTokenSource();

            MessageReader messageReader = new MessageReader(cts.Token);

            System.Console.CancelKeyPress += (s, e) =>
            {
                isRunning = false;
                cts.Cancel();
                Console.WriteLine("Exiting.");
            };

            messageReader.OnMessageReceived += (s, e) =>
            {
                var obj = JsonConvert.DeserializeObject<dynamic>(e.Data);
                Console.WriteLine();
                Console.WriteLine($"{DateTime.Now} > Message Received: ");
                Console.WriteLine($"     Message: {obj.message}");
                Console.WriteLine($"     Queue: {obj.queue}");
                Console.WriteLine();
            };

            messageReader.RunAsync("evenQueue");

            MessageSender messageSender = new MessageSender();

            int messageNumber = 1;

         
            //Instanciate device
            Device pi = new Device();
            bool ledState = false;

            //Loop 
            while (isRunning)
            {
                //Read pir and led state
                bool pirState = pi.ReadPirPin();

                //Super simple state machine
                if (pirState && !ledState)
                {
                    ledState = true;
                    pi.SetLEDPin(ledState);
                }
                else if (!pirState && ledState)
                {
                    ledState = false;
                    pi.SetLEDPin(ledState);
                }                

                Console.WriteLine("Sent Message");
                messageSender.SendMessageAsync($"LED state: {ledState}", "evenQueue");
                Thread.Sleep(1000);
            }
        }
    }
}