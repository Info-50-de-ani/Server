using System;
using System.Collections.Generic;
using PaintingClassServer.Services;
using System.Text.Json;
using PaintingClassCommon;

namespace PaintingClassServer.Services
{
    public class RoomUser
    {
        public int clientID;
        public string name;
        public int profToken = 0;
        public bool isOwner = false;
        public bool isShared = false;

        public int wbItemIndex=0;//todo:could be replaced with whiteboardData.Count, maybe
        public List<WBItemMessage> whiteboardData = new();

        public Room room; // room-ul in care este user-ul
        public RoomBehaviour rb; // poate fi null daca user-ul nu este conectat

        public bool isConnected { get => rb != null; }
    }
}
