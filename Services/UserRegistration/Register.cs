using System;
using System.Collections.Generic;
using System.Text.Json;
using WebSocketSharp.Server;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using MimeKit;
using PaintingClassServer;

namespace Server.Services.UserRegistration
{
	/// <summary>
	/// Primit ca mesaj de la client
	/// cand doreste sa se inregistreze
	/// contine parola nehashuita 
	/// </summary>
	[Serializable]
	public class RegisterUserData
	{
		public string name { set; get; }
		public string email { set; get; }
		public string password { set; get; }
	}

	public class PendingUserData
	{
		public string email { set; get; }
		public string name { set; get; }
		public byte[] hashedPassword { set; get; }
		public byte[] salt { set; get; }
		public Register socket { set; get; }
	}

	enum ServerResponse
	{
		AlreadyRegistered = 0, Fail, Succes, NotRegistered, WaitingForConfirmation
	}

	public class Register : WebSocketBehavior
	{
		private static Random random = new Random(DateTime.Now.Millisecond);
		
		private static string connectionString;
		public static SqlCommand sqlCommand;
		public static SqlConnection sqlConnection;
		
		private static SHA512 sha3 = new SHA512Managed();
		private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
		

		// contine lista de useri care urmeaza sa isi confirme emailul 
		public static Dictionary<int,PendingUserData> pendingUsers = new Dictionary<int, PendingUserData>(); 

		/// <summary>
		/// initializeaza conexiunea cu baza de date 
		/// </summary>
		public static void InitDB()
		{
			// folosim pentru a ne conecta la o baza de date specifica
			connectionString = ConfigurationManager.AppSettings["connectionString"];

			sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			sqlCommand = new SqlCommand();
			sqlCommand.Connection = sqlConnection;
		}

		/// <summary>
		/// Verifica baza de date pentru un user
		/// </summary>
		/// <param name="registerUserData"></param>
		/// <returns></returns>
		private static bool UserRegistered(RegisterUserData registerUserData)
		{
			sqlCommand.CommandText = $"select * from Credentials where Email=\'{registerUserData.email}\'";

			using (SqlDataReader reader = sqlCommand.ExecuteReader())
			{
				if (reader.Read())
					return true;
				else
					return false;
			}
		}

		/// <summary>
		/// Genereaza un PendingUserData, ce va fi stocat pana la confirmarea prin email 
		/// a detinatorului sau pana la expirare.
		/// </summary>
		/// <param name="registerUserData"></param>
		/// <returns>PendingUserData contine parola hashuita si saltul </returns>
		private static PendingUserData GetPendingUserData(RegisterUserData registerUserData, Register socket)
		{
			byte[] salt = new byte[64];
			rngCsp.GetBytes(salt);

			byte[] passwordBytes = Encoding.ASCII.GetBytes(registerUserData.password);

			byte[] saltedPass = new byte[passwordBytes.Length + salt.Length];
			salt.CopyTo(saltedPass, 0);
			passwordBytes.CopyTo(saltedPass, salt.Length);

			byte[] hashedPass = sha3.ComputeHash(saltedPass);

			return new PendingUserData 
			{ 
				hashedPassword = hashedPass,
				name = registerUserData.name,
				email = registerUserData.email,
				salt = salt,
				socket = socket
			};
		}

		/// <summary>
		/// Stocheaza informatia despre un user care si-a confirmat emailul
		/// in baza de date
		/// </summary>
		/// <param name="pendingUserData"></param>
		public static void SavePendingUser(PendingUserData pendingUserData)
		{
			sqlCommand.CommandText = $"insert into Credentials (Email,Name,HashedPass,Salt,Token) values (\'{pendingUserData.email}\',\'{pendingUserData.name}\',@hashedPass ,@salt,\'0\')";

			sqlCommand.Parameters.Add("@hashedPass", SqlDbType.Binary);
			sqlCommand.Parameters["@hashedPass"].Value = pendingUserData.hashedPassword;
			sqlCommand.Parameters.Add("@salt", SqlDbType.Binary);
			sqlCommand.Parameters["@salt"].Value = pendingUserData.salt;

			sqlCommand.ExecuteNonQuery();
			
			pendingUserData.socket?.Send(((int)ServerResponse.Succes).ToString());
		}


		protected override void OnMessage(WebSocketSharp.MessageEventArgs e)
		{
			RegisterUserData registerUserData = JsonSerializer.Deserialize<RegisterUserData>(e.Data);

			Console.WriteLine($"{ registerUserData.name} is trying to register.");

			if (registerUserData == null)
			{
				Send(((int)ServerResponse.Fail).ToString());
				throw new Exception("registerUserData can't be null");
			}

			if (UserRegistered(registerUserData))
			{
				Send(((int)ServerResponse.AlreadyRegistered).ToString());
				return;
			}
			
			// stocam requestul de inregistrare pana primim confirmarea emailului
			int requestId = random.Next(1, int.MaxValue);
			pendingUsers.Add(requestId, GetPendingUserData(registerUserData, this));

			// trimitem userului un email de confirmare ce contine un link de tipul
			// http://IPPublic:32221?email=xulescu@gmail.com&requestId=12345
			// pe care acesta va apasa pentru a confirma
			EmailService.SendConfirmationEmail(registerUserData, requestId);

			Send(((int)ServerResponse.WaitingForConfirmation).ToString());
		}
	}
}
