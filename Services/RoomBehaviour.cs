using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp.Server;
using PaintingClassServer;
using WebSocketSharp;
using System.Text.Json;
using PaintingClassCommon;


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

        #region  Send stuff
        public void SendMessage (string msg)
        {
            Send(msg);
        }

        public void SendSyncRequest ()
        {
            Send(Packet.Pack(PacketType.SyncRequestMessage,JsonSerializer.Serialize<SyncRequestMessage>(new SyncRequestMessage { clientID = ru.clientID} )));
        }

        public void BroadcastUserListMessage()
        {
            Sessions.Broadcast(Packet.Pack(PacketType.UserListMessage, JsonSerializer.Serialize(room.GenerateUserListMessage())));
        }
        public void ProcessWBItem(WBItemMessage msg)
        {
            if (msg.clientID != ru.clientID)
            {
                Console.WriteLine("You can't process somebody else's WBItemMessage!");
                return;
            }

            //ne asiguram ca suntem sincronizati
            if (ru.wbItemIndex != msg.itemIndex)
            {
                Console.WriteLine($"#{room.roomId}: User {ru.clientID}({ru.name}) desync-ed");
                SendSyncRequest();
                return;
            }
            ru.wbItemIndex++;

            //logica speciala in caz ca type este clearAll
            if (msg.type == WBItemMessage.ContentType.clearAll)
            {
                ru.whiteboardData.Clear();
                ru.wbItemIndex = 0;
            }
            else
                ru.whiteboardData.Add(msg);

            string serializedPackage = Packet.Pack(PacketType.WBItemMessage, JsonSerializer.Serialize(msg));
            foreach (var roomUser in room.users.Values)
            {
                if (roomUser != ru && (ru.isShared || roomUser.isOwner) && roomUser.isConnected)
                {
                    roomUser.rb.SendMessage(serializedPackage);
                }
            }
        }
        #endregion

        protected override void OnOpen()
        {
            int clientId = int.Parse(Context.QueryString["clientId"]);
            room.connectedUsers++;

            if (room.users.TryGetValue(clientId,out RoomUser _ru))
            {
                //folosim un RoomUser
                ru = _ru;
                ru.rb = this;
                Console.WriteLine($"#{room.roomId}: User {ru.clientID}({ru.name}) rejoined.");
            }
            else
            {
                //cream un nou RoomUser
                ru = new RoomUser
                {
                    clientID = clientId,
                    name = Context.QueryString["name"],
                    profToken = int.Parse(Context.QueryString["proftoken"]),
                    room = room,
                    rb = this
                };
                if (ru.profToken == room.ownerToken)
                {   
                    ru.isOwner = true;
                    room.ownerRU = ru;
                }
                room.users.Add(clientId,  ru);
    
                Console.WriteLine($"#{room.roomId}: User {ru.clientID}({ru.name}) joined.");
            }
            //trimite schimbarea la toti clientii
            BroadcastUserListMessage();
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            System.Console.WriteLine( $"Eroare in #{room.roomId}: {e.Message}");
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine($"#{room.roomId}: User {ru.clientID}({ru.name}) left.");
            room.connectedUsers--;
            
            // aratam ca conexiunea sa inchis si ca nu este conectat user-ul
            ru.rb = null;
            if (room.connectedUsers == 0)
                room.Close();

            //trimite schimbarea la toti clientii
            BroadcastUserListMessage();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Packet p = Packet.Unpack(e.Data);

            switch (p.type)
            {
                case PacketType.WBItemMessage:
                    {
                        var wbItem = JsonSerializer.Deserialize<WBItemMessage>(p.msg);
                        ProcessWBItem(wbItem);
                        break;
                    }
                case PacketType.WBCollectionMessage:
                    {
                        var wbColl = JsonSerializer.Deserialize<WBCollectionMessage>(p.msg);
                        if (wbColl.clientID != ru.clientID)
                        {
                            Console.WriteLine("You can't proccess somebody else's WBCollectionMessage!");
                            return;
                        }
                        if (wbColl.partial==false)
                        {
                            Console.WriteLine($"#{room.roomId}: User {ru.clientID}({ru.name}) resync-ed");
                            ru.whiteboardData.Clear();
                            ru.wbItemIndex = 0;
                        }
                        foreach (var item in wbColl.items)
					    {
                            ProcessWBItem(item);
					    }
                        break;
                    }
           
                case PacketType.SyncRequestMessage:
                    {
                        var srmsg = JsonSerializer.Deserialize<SyncRequestMessage>(p.msg);
                        WBCollectionMessage coll = new()
                        {
                            clientID = srmsg.clientID,
                            partial = false,
                            items = room.users[srmsg.clientID].whiteboardData.ToArray()
                        };
                        SendMessage(Packet.Pack(PacketType.WBCollectionMessage,JsonSerializer.Serialize(coll) ));
                        break;
                    }
                case PacketType.ShareRequestMessage:
                    {
                        ShareRequestMessage sm = JsonSerializer.Deserialize<ShareRequestMessage>(p.msg);
                        RoomUser roomUser = room.users[sm.clientId];
                        //ne asiguram ca doar profu poate da share
                        if (ru != room.ownerRU || roomUser.isShared == sm.isShared) break;
                        roomUser.isShared = sm.isShared;
                        Console.WriteLine($"#{room.roomId}: User {roomUser.clientID}({roomUser.name})" + (sm.isShared ? "started sharing":"stopped sharing"));

                        //trimite schimbarea la toti clientii
                        BroadcastUserListMessage();
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
