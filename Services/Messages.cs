using System;
using System.Text.Json;
using System.Web;

namespace PaintingClassCommon
{
    // Aceast fisier TREBUIE sa fie identic cu cealalta copie!!!

    public enum PacketType
    {
        none = 0,

        UserListMessage = 1,
        ShareRequestMessage = 2,

        WBItemMessage = 10,
        WBCollectionMessage = 11,
        SyncRequestMessage = 12,

        WhiteboardMessage = 999 // obsolete
    }

    /// <summary>
    /// Fiecare Message este trimis intrun Packet
    /// </summary>
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

    /// <summary>
    /// Contine date despre toti utilizatorii, se trimite la toti cand o schimbare se petrece
    /// Trimis de server
    /// </summary>
    [Serializable]
    public class UserListMessage
    {
        [Serializable]
        public class UserListItem
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool isConnected { get; set; }
            public bool isShared { get; set; }
            public int wbItemIndex { get; set; }
        }
        public UserListItem[] list { get; set; }
    }

    /// <summary>
    /// Cere sa dai share la un user
    /// Trims de client-ul profesorului
    /// </summary>
    [Serializable]
    public class ShareRequestMessage
    {
        public int clientId { get; set; }
        public bool isShared { get; set; }
    }

    /// <summary>
    /// Poate transmite un drawing, userControl sau comanda de a goli tabla
    /// Poata adauga acel element sau poate inlocui o versiune veche a lui
    /// </summary>
    [Serializable]
    public class WBItemMessage
    {
        public enum ContentType
        {
            drawing,
            userControl,
            clearAll
        }
        public enum Operation
        {
            add,
            edit,
            delete
        }

        public int clientID { get; set; }
        public int itemIndex { get; set; }
        public int contentIndex { get; set; }
        public ContentType type { get; set; }
        public Operation op { get; set; }
        public string content { get; set; }
    }

    /// <summary>
    /// O colectie de iteme de tabla
    /// Daca partial este false atunci mesajul contine *toata* si ce exista deja trebuie golit
    /// </summary>
    [Serializable]
    public class WBCollectionMessage
    {
        public int clientID { get; set; }
        public bool partial { get; set; } = true;
        public WBItemMessage[] items { get; set; }
    }

    /// <summary>
    /// Cere sa retrimita tabla folosind un WBCollectionMessage
    /// </summary>
    [Serializable]
    public class SyncRequestMessage
    {
        public int clientID { get; set; }
    }

    //trimis de client si de server
    [Serializable]
    [Obsolete("Vechi", true)]
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

}
