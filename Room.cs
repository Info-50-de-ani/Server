using System;
using System.Collections.Generic;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using PaintingClassServer.Services;
using System.Text.Json;

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
        public int connectedUsers;

        public UserListMessage GetConnectedUsers()
        {
            var idListe = new int[connectedUsers];
            var nameListe = new string[connectedUsers];
            int i = 0;
            foreach (var x in users)
            {
                if (x.Value.isConnected)
                {
                    idListe[i] = x.Value.clientId;
                    nameListe[i] = x.Value.name;
                    i++;
                }
            }

            return new UserListMessage { idList = idListe,  nameList  =  nameListe };
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
