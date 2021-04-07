using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;
namespace Server
{
	public static class ServerCertificateManager
	{
		public static void AddCertificate(WebSocketServer server)
		{
			// setam pathul spre certificat si parola
			// certificatul este temporar
			var path = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Certificate\\ChangeCert.pfx");
			server.SslConfiguration.ServerCertificate = new X509Certificate2(path, "apples");
		}
	}
}
