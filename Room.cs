using System;
using System.Collections.Generic;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using PaintingClassServer.Services;
using System.Text.Json;
using PaintingClassCommon;

namespace PaintingClassServer
{
    public class Room
    {
        public class RoomUser
        {
            public int clientId;
            public string name;
            public int profToken = 0;
            public bool isOwner = false;
            //todo: ce se intampla cand un user intra dupa ce cineva a pritit share?
            public bool isShared = false;
            //todo: daca primesti o comanda de stergere a tablei nu trb sa tii minte ce este inaintea comenzii
            public List<WhiteboardMessage> whiteboardData;

            public RoomBehaviour rb;

            public bool isConnected { get => rb != null; }
        }
        
        // folosim hash table sa stocam Roomrile
        public static Dictionary<int, Room> openRooms = new Dictionary<int, Room>();

        //id-ul incaperii
        public int roomId;
        //valoare profToken ce repezinta profId-ul profesorului ce a creat incaperea
        public int ownerToken;
        //can be null if onwer hasn't joined yet for some reason
        public RoomUser ownerRU;
        //lista Users
        public Dictionary<int, RoomUser> users = new Dictionary<int, RoomUser>();
        //useri logati
        public int connectedUsers;

        public UserListMessage GenerateUserListMessage()
        {
            UserListMessage.UserListItem[] list = new UserListMessage.UserListItem[users.Count];
            int i = 0;
            foreach (var user in users)
            {
                list[i] = new UserListMessage.UserListItem
                {
                    id = user.Key,
                    name = user.Value.name,
                    isConnected = user.Value.isConnected,
                    isShared = user.Value.isShared
                };
            }

            return new UserListMessage { list = list };
        }

        public Room(int _ownerToken)
        {
            do
            {
                roomId = new Random(DateTime.Now.Second).Next(1, int.MaxValue);
            }
            while (openRooms.ContainsKey(roomId));
            ownerToken = _ownerToken;

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
