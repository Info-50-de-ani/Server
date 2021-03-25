using System;
using WebSocketSharp.Server;
using PaintingClassServer;

namespace PaintingClassServer.Services
{
    public class CreateRoom : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            int token;
            if(int.TryParse(Context.QueryString["profToken"],out token) && Program.profTokens.Contains(token))
            {
                var room = new Room(token);
                Send(room.roomId.ToString());
            }
            else Sessions.CloseSession(ID);
        }
    }
}
