using System;
using System.Collections.Generic;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using WebSocket___Server.Storage;
namespace WebSocket___Server
{
    class Home : WebSocketBehavior
    {
        private int joinRoomCode = 0;
        public Home()
        {
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
        private int p_Index = 0;
        // folosim hash sa stocam useri (nu folosim list pt ca atunci cand un user se deconecteaza 
        // indexul se modifica
        public Dictionary<int, Chat> Users = new Dictionary<int, Chat>();
        public Room(int id)
        {
            this.id = id;
            stashRoomIds.Add(id, this);
            // adaugam un nou path cu noul room
            WS.wssv.AddWebSocketService<Room.Chat>($"/room/{id}", () => new Chat(this));
            URL = $"ws://localhost:{WS.port}/room/{id}";
        }
        //cate o noua instanta a clasei se formeaza cand un user intra
        public class Chat : WebSocketBehavior
        {
            private Room _room; // roomul de care apartine
            public bool _isPresenting = false;//todo
            public bool _isHost { set; get; } = false;
            public string _name { set; get; } = "No name";
            public int _index = 0;

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
                room.Users.Add(_index, this); 
            }

            protected override void OnClose(CloseEventArgs e)
            {
                Sessions.Broadcast($"DIS {_index} {_name}");
                //stergem roomul daca ultimu e host sau daca hostu da dis.
                if (_room.Users.Count == 1 || _index == 0)
                {
                    WS.wssv.RemoveWebSocketService(_room.URL);
                    _room.Users.Clear();
                    Room.stashRoomIds.Remove(_room.id);
                    Console.WriteLine("Removed room");
                }
                else
                {
                //remove doar la user
                    _room.Users.Remove(_index);
                    Console.WriteLine("Removed user");
                }
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                if (e.Data.Contains("SND"))
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
                    Sessions.Broadcast($"BRC {_index} {e.Data}");
            }

            protected override void OnOpen()
            {
                _name = Context.QueryString["name"];
                Sessions.Broadcast($"CON {_index} {_name}");
                foreach (var x in _room.Users)
                {
                    if (x.Value._index != _index)
                        Send($"CON {x.Value._index} {x.Value._name}");
                }
            }
        }
    }
    static class TokensAndPasswords
    {
        public class User
        {
            string email;
            string password;
            int Token;
        }

        public static Dictionary<int, int> Tokens = new Dictionary<int, int>();
        public static List<User> Users = new List<User>();
        static TokensAndPasswords()
        {
            Tokens.Add(12345, 1);
            Tokens.Add(111, 1);

          //  Storage.AppdataIO.Load<User>("Users.xml"); todo
        }
    }
    public class ProfLogin : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            // todo
        }
    }

    public class CreateRoom : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            int token;
            if(int.TryParse(Context.QueryString["profToken"],out token))
            {
                if (TokensAndPasswords.Tokens.ContainsKey(token))
                {
                    GenRoom();
                }
                else
                {
                    this.Sessions.CloseSession(this.ID);
                }
            }
            else
                this.Sessions.CloseSession(this.ID);
        }
        public void GenRoom()
        {
            int id = IdGen.New();
            Console.WriteLine($"Created Room {id}");
            var NewRoom = new Room(id); 
            Send(id.ToString());
        }
    }

    public static class WS
    {
        public const int port = 32281;
        public static WebSocketServer wssv = new WebSocketServer(port);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            WS.wssv.AddWebSocketService<Home>("/CreateRoom");
            WS.wssv.Start();
            Console.WriteLine("Started Server");
            Console.ReadKey(true);
            WS.wssv.Stop();
        }   
    }
}
