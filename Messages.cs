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
        ShareMessage = 3
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

    [Serializable]
    public class UserListMessage
    {
        public int[] idList { get; set; }
        public string[] nameList { get; set; }
    }

    [Serializable]
    public class ShareRequestMessage
    {
        public int clientId;
        public bool shared;
    }
}
