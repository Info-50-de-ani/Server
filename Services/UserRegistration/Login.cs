using System;
using System.Text.Json;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Security.Cryptography;
using System.Text;
using System.Data.SqlClient;
using PaintingClassServer;
using Dapper;
using System.Data;

namespace Server.Services.UserRegistration
{ 
	[Serializable]
	public class LoginUserData
	{
		public string email { set; get; }
		public string password { set; get; }
	}



	class Login : WebSocketBehavior
	{
		private static Random tokenGen = new Random();

		protected override void OnError(WebSocketSharp.ErrorEventArgs e)
		{
			throw new Exception(e.Message);
		}

		private static SHA512 sha3 = new SHA512Managed();

		public class DBData
		{
			public string Email { get; set; }
			public string Name { get; set; }
			public byte[] HashedPass { get; set; }
			public byte[] Salt { get; set; }
			public int Token { get; set; }
		}

		protected override void OnMessage(MessageEventArgs e)
		{
		 	LoginUserData loginUserData = JsonSerializer.Deserialize<LoginUserData>(e.Data);
				
			if(loginUserData == null)
			{
				Send(((int)ServerResponse.Fail).ToString());
				return;
			}
			DynamicParameters dynamicParameters = new DynamicParameters();
			dynamicParameters.Add("@email", loginUserData.email, DbType.String);
			var res = Register.dbConnection.Query<DBData>("select * from Credentials where Email=@email",dynamicParameters).AsList();
			if (res.Count == 1)
			{
				byte[] storedPass = res[0].HashedPass;

				#region salt + pass
				byte[] passwordBytes = Encoding.ASCII.GetBytes(loginUserData.password);

				byte[] bsalt = res[0].Salt;

				byte[] saltedPass = new byte[passwordBytes.Length + bsalt.Length];
				bsalt.CopyTo(saltedPass, 0);
				passwordBytes.CopyTo(saltedPass, bsalt.Length);
				#endregion

				string hashedPass = Encoding.ASCII.GetString(sha3.ComputeHash(saltedPass));

				if(hashedPass == Encoding.ASCII.GetString(storedPass))
				{
					//genToken
					int token;
					do
						token = tokenGen.Next(1,int.MaxValue);
					while (Program.profTokens.Contains(token));
					Program.profTokens.Add(token);
					Send($"{((int)ServerResponse.Succes)} {token} {res[0].Name}");
					return;						
				}
				else
				{
					Send(((int)ServerResponse.NotRegistered).ToString());
					return;
				}
			}
			else
			{
				Send(((int)ServerResponse.NotRegistered).ToString());
				return;
			}
			
		}
	}
}
