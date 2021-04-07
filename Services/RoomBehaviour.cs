using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp.Server;
using PaintingClassServer;
using WebSocketSharp;
using System.Text.Json;
using PaintingClassCommon;
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
            //todo: ce se intampla cand reintri cu acelasi clientId dar nume diferit
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
                ru = new RoomUser
                {
                    clientId = clientId,
                    name = Context.QueryString["name"],
                    profToken = int.Parse(Context.QueryString["proftoken"]),
                    rb = this,
                    whiteboardData = new List<WhiteboardMessage>()
                };
                if (ru.profToken == room.ownerToken)
                {   
                    ru.isOwner = true;
                    room.ownerRU = ru;
                }
                room.users.Add(clientId,  ru);

                
                Console.WriteLine($"#{room.roomId}: User {ru.clientId}({ru.name}) joined.");

            }
            Sessions.Broadcast(Packet.Pack(PacketType.UserListMessage, JsonSerializer.Serialize(room.GenerateUserListMessage())));
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

            Sessions.Broadcast(Packet.Pack(PacketType.UserListMessage, JsonSerializer.Serialize(room.GenerateUserListMessage())));
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Packet p = Packet.Unpack(e.Data);

            switch (p.type)
            {
                case PacketType.WhiteboardMessage:
                    WhiteboardMessage wm = JsonSerializer.Deserialize<WhiteboardMessage>(p.msg);
                    //ne asiguram ca clintId-ul este acelasi
                    if (wm.clientId != ru.clientId) break;
                    ru.whiteboardData.Add(wm);
                    break;

                case PacketType.ShareRequestMessage:
                    ShareRequestMessage sm = JsonSerializer.Deserialize<ShareRequestMessage>(p.msg);
                    //ne asiguram ca doar profu poate da share
                    if (ru != room.ownerRU || ru.isShared == sm.isShared) break;
                    ru.isShared = sm.isShared;
                    
                    Console.WriteLine($"#{room.roomId}: User {ru.clientId}({ru.name})" + (sm.isShared ? "started sharing":"stopped sharing"));

                    //trimite schimbarea la toti clientii
                    Sessions.Broadcast(Packet.Pack(PacketType.UserListMessage, JsonSerializer.Serialize(room.GenerateUserListMessage())));

                    break;
                default:
                    break;
            }
        }
    }
}
