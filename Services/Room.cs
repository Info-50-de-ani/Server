using System;
using System.Collections.Generic;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using PaintingClassServer.Services;
using PaintingClassCommon;

namespace PaintingClassServer.Services
{
    public partial class Room
    {
        
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
                    isShared = user.Value.isShared,
                    wbItemIndex = user.Value.wbItemIndex
                };
                i++;
            }

            return new UserListMessage { list = list };
        }

        public Room(int _ownerToken)
        {
            do
            {
                roomId = new Random(DateTime.Now.Millisecond).Next(1, int.MaxValue);
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
