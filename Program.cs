using System;
using System.Collections.Generic;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
namespace WebSocket___Server
{
    public class ChatRoom : WebSocketBehavior
    {
        private static int usersConnected =0;
        public ChatRoom()
        {
            Interlocked.Increment(ref usersConnected);
            Console.WriteLine($"{usersConnected}");
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            Send(e.Data);
        }
    }
    class Home : WebSocketBehavior
    {
        private bool isHost=false;
        private int joinRoomCode=0;
        public Home()
        {
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            //check it is host 
            if (!bool.TryParse(Context.QueryString["host"], out isHost) || !int.TryParse(Context.QueryString["room"],out joinRoomCode))
            {
                //handle error
                Console.WriteLine("No host key passed or invalid value");
            }
            if(isHost)
            {
                CreateRoom(IdGen.New());
            }
            else
            {
                JoinRoom(joinRoomCode);
            }
            
        }
        public void JoinRoom(int id)
        {
            if (Room.stashRoomIds.ContainsKey(joinRoomCode))
            {
                Console.WriteLine($"Joined Room {id}");
                Send(Room.stashRoomIds[joinRoomCode].URL);
            }
            else Send("[Error] Invalid id");
        }
        public void CreateRoom(int id)
        {
            Console.WriteLine($"Created Room {id}");
            var NewRoom = new Room(id); // problema atomic?
            Send(NewRoom.URL);
        }
    }
    public class Room
    {
        // folosim hash table sa stocam Roomrile
        public static Dictionary<int, Room> stashRoomIds = new Dictionary<int, Room>();
        public string URL;
        public int id { set; get; }
        // folosim aceasta variabila pt a seta indexul unui nou participant
        private int p_Index=0;
        // folosim hash sa stocam useri (nu folosim list pt ca atunci cand un user se deconecteaza 
        // indexul se modifica
        public Dictionary<int, Chat> Users = new Dictionary<int, Chat>();
        public Room(int id)
        {
            this.id = id;
            stashRoomIds.Add(id, this);
            // adaugam un nou path cu noul room
            WS.wssv.AddWebSocketService<Room.Chat>($"/room/{id}", () => new Chat(this)) ;
            URL = $"ws://localhost:9000/room/{id}";
        }
        //cate o noua instanta a clasei se formeaza cand un user intra
        public class Chat : WebSocketBehavior
        {
            private Room _room; // roomul de care apartine
            public bool _isPresenting = false;
            public bool _isHost { set; get; } = false;
            public string _name { set; get; } = "No name";
            public int _index = 0;
            public string _prefix;

            public Chat()
            {
            }

            public Chat(Room room)
            {
                _room = room;
                _index = _room.p_Index;
                Interlocked.Increment(ref _room.p_Index);
                if (_index == 0)
                    _isHost = true;
                room.Users.Add(_index,this);  /// e oare problema ca instructiunile astea nus atomic?
                // prefixul cu care se vor trimite mesaje pe net
                _prefix = (_isHost ? "host" : "guest");
            }

            protected override void OnClose(CloseEventArgs e)
            {
                Sessions.Broadcast($"DIS {_index} {_name}");
                //stergem roomul daca ultimu e host sau daca hostu da dis.
                if (_room.Users.Count == 1 || _index==0)
                {
                    Sessions.Broadcast("EXIT");
                    _room.Users.Clear();
                    Room.stashRoomIds.Remove(_room.id);
                    Console.WriteLine("Removed room");
                }
                else
                {
                    _room.Users.Remove(_index);
                    Console.WriteLine("Removed user");
                }
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                // primul lucru pe care il trimite clientul este un 0
                if (e.Data == "0")
                {
                    Sessions.Broadcast($"CON {_index} {_name}");
                    foreach (var x in _room.Users)
                    {
                        if (x.Value._index != _index)
                            Send($"CON {x.Value._index} {x.Value._name}");
                    }
                }
                else if (e.Data.Contains("SND"))
                {
                    string[] data = e.Data.Split();
                    int idx = -1;
                    int.TryParse(data[1], out idx);
                    if (_room.Users.ContainsKey(idx))
                    {
                        string ans = "RCV ";
                        for (int i = 2; i < data.Length; i++)
                            ans += data[i] + " ";
                        _room.Users[idx].Send(ans);
                    }
                }
                else
                    Sessions.Broadcast($"BRC {(_isHost ? "host" : $"guest{_index}")} {e.Data}");
            }

            protected override void OnOpen()
            {
                _name = Context.QueryString["name"];
            }
        }
    }
    public static class WS
    {
        public static WebSocketServer wssv = new WebSocketServer("ws://localhost:9000");
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            WS.wssv.AddWebSocketService<Home>("/home");
            WS.wssv.Start();
            Console.ReadKey(true);
            WS.wssv.Stop();
        }   
    }
}
