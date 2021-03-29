using System;
using System.Text.Json;
using System.Web;

namespace PaintingClassCommon
{
    /// <summary>
    /// Aceasta clasa TREBUIE sa fie identica cu cealalta copie
    /// </summary>
    public enum PacketType
    {
        none = 0,
        WhiteboardMessage = 1,
        UserListMessage = 2,
        ShareRequestMessage = 3
    }
    [Serializable]
    public class Packet
    {
        public PacketType type { get; set; }
        public string msg { get; set; }

        public static Packet Unpack(string SerializedPacket)
        {
            return JsonSerializer.Deserialize<Packet>(SerializedPacket);
        }
        //evita creearea unui nou obiect deci e mai rapid
        //msg trebuie sa fie JSON
        public static string Pack(PacketType type, string msg)
        {
            string escapedMsg = HttpUtility.JavaScriptStringEncode(msg);
            return $"{{\"type\":{(int)type},\"msg\":\"{escapedMsg}\"}}";
        }
    }

    //trimis de client si de server
    [Serializable]
    public class WhiteboardMessage
    {
        public enum ContentType
        {
            Drawing, Action
        };

        public int clientId { get; set; }
        public ContentType type { get; set; }
        public string content { get; set; }
    }

    //trimis de server
    [Serializable]
    public class UserListMessage
    {
        [Serializable]
        public class UserListItem
        {
            public int id { get; set; }
            public string name {get;set;}
            public bool isConnected { get; set; }
            public bool isShared { get; set; }
        }
        public UserListItem[] list { get; set; }
    }

    //trims de client-ul profesorului
    public class ShareRequestMessage
    {
        public int clientId { get; set; }
        public bool isShared { get; set; }
    }
}
