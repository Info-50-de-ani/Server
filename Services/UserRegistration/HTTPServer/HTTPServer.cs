using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.IO;

namespace Server.Services.UserRegistration.HTTPServer
{
	class HTTPServer
	{
		public static HttpServer httpsv;

		public static void Init()
		{
			httpsv = new HttpServer(32221);
			httpsv.OnGet += Httpsv_OnGet;
			httpsv.Start();
		}

		private static void Httpsv_OnGet(object sender, HttpRequestEventArgs e)
		{
			var req = e.Request;
			if (!string.IsNullOrEmpty(req.QueryString["email"]) && !string.IsNullOrEmpty(req.QueryString["requestId"]))
			{
				int requestId;
				if (int.TryParse(req.QueryString["requestId"],out requestId))
				{
					if(Register.pendingUsers.ContainsKey(requestId))
					{
						if(Register.pendingUsers[requestId].email == req.QueryString["email"])
						{
							Register.SavePendingUser(Register.pendingUsers[requestId]);
							GetPageFromFile(e, 200, "registersucces.html");
							Console.WriteLine($"User {Register.pendingUsers[requestId]} succesfuly confirmed his email.");
							Register.pendingUsers.Remove(requestId);
						}
					}
				}
			}
			else
				GetPageFromFile(e, 400, "registerfail.html");
			return;
				
		}

		public static void GetPageFromFile(HttpRequestEventArgs e, int statusCode, string htmlFileName)
		{
			e.Response.StatusCode = statusCode;
			FileStream fileStream = File.OpenRead(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName + $"\\Html\\{htmlFileName}");
			e.Response.ContentLength64 = fileStream.Length;
			e.Response.ContentEncoding = Encoding.UTF8;
			e.Response.ContentType = "text/html";
			var memoryStream = new MemoryStream();
			fileStream.CopyTo(memoryStream);
			e.Response.Close(memoryStream.ToArray(), true);
			memoryStream.Dispose();
		}

	}
}
