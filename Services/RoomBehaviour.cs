using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp.Server;
using PaintingClassServer;
using WebSocketSharp;
using System.Text.Json;
using static PaintingClassServer.Room;


namespace PaintingClassServer.Services
{
    public class RoomBehaviour : WebSocketBehavior
    {
        Room room; // room-ul din care apartine
        RoomUser ru; // user-ul caruia ii apartine
        
        public RoomBehaviour(Room _room)
        {
            room = _room;
        }

        protected override void OnOpen()
        {
            int clientId = int.Parse(Context.QueryString["clientId"]);
            room.connectedUsers++;

            if (room.users.TryGetValue(clientId,out RoomUser _ru))
            {
                //folosim un RoomUser
                ru = _ru;
                ru.rb = this;
                Console.WriteLine($"#{room.roomId}: User {ru.clientId}({ru.name}) rejoined.");
            }
            else
            {
                //cream un nou RoomUser
                ru = new RoomUser();

                ru.clientId = clientId;
                ru.name = Context.QueryString["name"];
                ru.profToken = int.Parse(Context.QueryString["proftoken"]);
                ru.rb = this;
                room.users.Add(clientId,  ru);
                
                Console.WriteLine($"#{room.roomId}: User {ru.clientId}({ru.name}) joined.");

            }

            Sessions.Broadcast(Packet.Pack(PacketType.UserListMessage, JsonSerializer.Serialize(room.GetConnectedUsers())));
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            System.Console.WriteLine( $"Eroare in #{room.roomId}: {e.Message}");
        }


        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine($"#{room.roomId}: User {ru.clientId}({ru.name}) left.");
            room.connectedUsers--;
            
            // aratam ca conexiunea sa inchis si ca nu este conectat user-ul
            ru.rb = null;
            if (room.connectedUsers == 0)
                room.Close();

            Sessions.Broadcast(Packet.Pack(PacketType.UserListMessage, JsonSerializer.Serialize(room.GetConnectedUsers())));
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
        //protected override void OnClose(WebSocketSharp.CloseEventArgs e)
        //{
        //    Sessions.Broadcast($"DIS {_index} {_name}");
        //    //stergem roomul daca ultimu e host sau daca hostu da dis.
        //    if (_room.Users.Count == 1 || _index == 0)
        //    {
        //        Program.server.RemoveWebSocketService(_room.URL);
        //        _room.Users.Clear();
        //        Room.openRooms.Remove(_room.roomId);
        //        Console.WriteLine("Removed room");
        //    }
        //    else
        //    {
        //        //remove doar la user
        //        _room.Users.Remove(_index);
        //        Console.WriteLine("Removed user");
        //    }
        //}

        //protected override void OnMessage(MessageEventArgs e)
        //{
        //    if (e.Data.Contains("SND"))
        //    {
        //        string[] data = e.Data.Split();
        //        int idx = -1;
        //        int.TryParse(data[1], out idx);
        //        if (_room.Users.ContainsKey(idx))
        //        {
        //            string ans = "RCV ";
        //            for (int i = 2; i < data.Length; i++)
        //                ans += data[i] + " ";
        //            _room.Users[idx].Send(ans);
        //        }
        //    }
        //    else
        //        Sessions.Broadcast($"BRC {_index} {e.Data}");
        //}
    }
}
