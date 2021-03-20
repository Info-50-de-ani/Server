using System;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
namespace WebSocket___Sharp_Practice
{
    public static class WS
    {
        public const int port = 32281;
        public static List<string> ActivePresenters = new List<string>(4);
        private static Dictionary<int, string> localUserStash = new Dictionary<int, string>();
        private static Thread T_InRoom = null;
        private static bool? _isHost = null;
        private static string _URL;
        private static string _name;
        public static WebSocketSharp.WebSocket ws; 
        //connect to server as host
        public static void InitHost(string URL, string name)
        {
            _URL = URL;
            _name = name;
            _isHost = true;
            localUserStash.Add(0, WS._name);
            ws = new WebSocketSharp.WebSocket($"{URL}?host={_isHost}");
            WS.WSEventSubscribtions();
            ws.Connect();
        }
        //connect to server as client
        public static void InitClient(string URL,string name, int roomCode)
        {
            _URL = URL;
            _name = name;
            Debug.Assert(roomCode != null);
            _isHost = false;
            ws = new WebSocketSharp.WebSocket($"{URL}?host={_isHost}&room={roomCode}");
            WS.WSEventSubscribtions();
            ws.Connect();
        }
        public static void Close()
        {
            while (T_InRoom == null || !T_InRoom.IsAlive) { }
            T_InRoom.Join();
        }
        public static void WSEventSubscribtions()
        {
            Debug.Assert(_isHost != null);
            if ((bool)_isHost)
                ws.OnMessage += (sender, e) =>
                {
                    if (e.Data.Contains("ws://"))
                    {
                        T_InRoom = new Thread(() => Host.InRoom(e.Data + $"?name={_name}"));
                        T_InRoom.Start();
                        ws.Close();
                    }
                    else
                        Console.WriteLine(e.Data);
                };
            else
                ws.OnMessage += (sender, e) =>
                {
                    if (e.Data.Contains("ws://"))
                    {
                        T_InRoom = new Thread(() => Client.InRoom(e.Data + $"?name={_name}&room={Client.roomCode}"));
                        T_InRoom.Start();
                        ws.Close();
                    }
                    else
                        Console.WriteLine(e.Data);
                };
            ws.OnOpen += (sender, e) => { Console.WriteLine("Conection established"); ws.Send("0"); };
            ws.OnError += (sender, e) => Console.WriteLine(e.Message);
        }
        public static class Host
        {
            public static int roomCode;
            static public WebSocket ws;
            public static void InRoom(string URL)
            {
                Debug.Assert(int.TryParse(URL.Substring(URL.IndexOf("room") + 5, 7), out roomCode));
                ws = new WebSocket(URL);
                EventSubscribtions();
                ws.Connect();
                string msg = "da";
                while (msg != "exit")
                {
                    msg = Console.ReadLine();
                    ws.Send(msg);
                }
            }
            public static void EventSubscribtions()
            {
                ws.OnOpen += (sender, e) => { Console.WriteLine($"Room {roomCode}"); };
                ws.OnMessage += (sender, e) =>
                {
                    Console.WriteLine(e.Data);
                };
                ws.OnError += (sender, e) => Console.WriteLine(e.Message);
            }
        }
        public static class Client
        {
            public static int roomCode;
            public static WebSocket ws;
            public static void InRoom(string URL)
            {
                ws = new WebSocket(URL);
                Debug.Assert(int.TryParse(URL.Substring(URL.IndexOf("room") + 5, 7), out roomCode));
                Client.ClientEnventSubscribtions();
                ws.Connect();
                string msg = "da";
                while (msg != "exit")
                {
                    msg = Console.ReadLine();
                    ws.Send(msg);
                }
            }
            public static void ClientEnventSubscribtions()
            {
                ws.OnOpen += (sender, e) => {
                    Console.WriteLine($"Room {roomCode}");
                    ws.Send("0"); };
                ws.OnMessage += (sender, e) =>
                {
                    if (e.Data == "EXIT")
                        ws.Close();
                    else if(e.Data.Substring(0,3)=="RCV")
                    {
                        Console.WriteLine(e.Data);
                    }
                    else if (e.Data.Substring(0,3)==("CON"))
                    {
                        string[] dat= e.Data.Split();
                        localUserStash.Add(int.Parse(dat[1]),dat[2]);
                    }
                    else
                    Console.WriteLine(e.Data);
                };
                ws.OnError += (sender, e) => Console.WriteLine(e.Message);
            }
        }
    }
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Started Client");
            //ReadName
            #region Input
            bool isHost = false;
            int roomNumberJoin=0;
            Console.WriteLine("Host?(y or n)");
            string AnsIsHost = Console.ReadLine();
            if (AnsIsHost == "y")
                isHost = true;
            else
            {
                if (!int.TryParse(Console.ReadLine(), out roomNumberJoin))
                    Debug.Assert(false);
            }
            #endregion
            if (isHost)
                WS.InitHost($"ws://localhost:{WS.port}/home", "Giorgica");
            else
                WS.InitClient($"ws://localhost:{WS.port}/home", "MateiCorvin", roomNumberJoin);
            WS.Close();
        }

    }
}
