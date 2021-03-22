using System;
using WebSocketSharp.Server;
using PaintingClassServer.Services;
using System.Collections.Generic;

namespace PaintingClassServer
{
    public static class Constants
    {
        //cui nui plac url-urile hardcodate?
        public const string url = "ws://127.0.0.1:32281";
        public const int port = 32281;
    }

    public class Program
    {

        public static WebSocketServer server;

        //temporar
        public static HashSet<int> profTokens = new HashSet<int>();

        public static void Main(string[] args)
        {
            //creeam serverul
            server = new WebSocketServer(Constants.url);
            server.Start();
            Console.WriteLine("Started Server");

            //hardcodat
            profTokens.Add(1234);

            //adaugam servicii
            server.AddWebSocketService<CreateRoom>("/createRoom");

            Console.ReadKey(true);
            server.Stop();

        }   
    }
}
