using System;
using System.Collections.Generic;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using PaintingClassServer.Services;

namespace PaintingClassServer
{
    public class Room
    {
        public class RoomUser
        {
            public int clientId;
            public string name;
            public int profToken = 0;

            public RoomBehaviour rb;

            public bool isConnected { get => rb != null; }
            public bool isProf { get => profToken != 0; }
        }
        
        // folosim hash table sa stocam Roomrile
        public static Dictionary<int, Room> openRooms = new Dictionary<int, Room>();
        
        public int roomId { set; get; }
        //lista Users
        public Dictionary<int, RoomUser> users = new Dictionary<int, RoomUser>();
        //useri logati
        public int connectedUsers
        {
            get
            {
                int nr = 0;
                foreach (var kvp in users)
                {
                    if (kvp.Value.isConnected)
                        nr++;
                }
                return nr;
            }
        }
        
        public Room()
        {
            do
            {
                roomId = new Random(DateTime.Now.Second).Next(1, int.MaxValue);
            }
            while (openRooms.ContainsKey(roomId));

            openRooms.Add(roomId, this);
            Console.WriteLine($"Created Room #{roomId}");

            // inregistram noua camera
            Program.server.AddWebSocketService<RoomBehaviour>($"/room/{roomId}", () => new RoomBehaviour(this));
        }

        public void Close()
        {
            Console.WriteLine($"Closed Room #{roomId}");
            Program.server.RemoveWebSocketService($"/room/{roomId}");
            openRooms.Remove(roomId);
        }
    }

}
