using System;
using WebSocketSharp.Server;
using PaintingClassServer.Services;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using WebSocketSharp;
using Server;
using System.Net;
using Server.Services.UserRegistration.HTTPServer;
using System.Text.Json;
using Server.Services.UserRegistration;

namespace PaintingClassServer
{
    public static class Constants
    {
        public static readonly string registerSenderEmail;
        public static readonly string registerSenderPassword;
        public const int socketPort = 32281;
        public const int httpPort = 32221;
        public static readonly string publicIPAdress;

        static Constants()
		{
            publicIPAdress = GetIPAddress();
            var cred = ReadEmailServiceCredentials();
            registerSenderEmail = cred.email;
            registerSenderPassword = cred.password;
		}
		static string GetIPAddress()
		{
            string address = "";
			WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
			using (WebResponse response = request.GetResponse())
			using (StreamReader stream = new StreamReader(response.GetResponseStream()))
			{
				address = stream.ReadToEnd();
			}

			int first = address.IndexOf("Address: ") + 9;
			int last = address.LastIndexOf("</body>");
			address = address.Substring(first, last - first);

			return address;
		}

        class EmailServiceCredentials
        {
            public string email { get; set; }
            public string password { get; set; }
        }

        private static EmailServiceCredentials ReadEmailServiceCredentials()
        {
            string txt = File.ReadAllText("./SmptCredentials.json");
            var cred = JsonSerializer.Deserialize<EmailServiceCredentials>(txt);
            return cred;
        }
	}

    public class Program
    {
        public static WebSocketServer server;

        //temporar
        public static HashSet<int> profTokens = new HashSet<int>();

        public static void Main(string[] args)
        {
            // intializam conexiunea cu baza de date care
            // contine informatiile despre profesori
            Server.Services.UserRegistration.Register.InitDB();
            
            // pornim servurl http pt inregistrare 
            HTTPServer.Init();
            Console.WriteLine($"IP Address: {Constants.publicIPAdress}");

            // folosim constructor cu port 
            // deoarece au intervenit probleme cu ip-ul cand am testat pe net
            server = new WebSocketServer(Constants.socketPort, true);
            
            ServerCertificateManager.AddCertificate(server);

            Console.WriteLine("Started Server");

            //hardcodat
            profTokens.Add(1234);
            EmailService.SendConfirmationEmail(new RegisterUserData { email = "gabriel10tm@gmail.com" }, 1);
            //adaugam servicii
            server.AddWebSocketService<Server.Services.UserRegistration.Login>("/login");
            server.AddWebSocketService<Server.Services.UserRegistration.Register>("/register");

            server.Start();

            char c=(char)0;
            Console.WriteLine("Press Y to exit");
            while (c!='y'&&c!='Y')
            {
                c = Console.ReadKey(true).KeyChar;
            }
            server.Stop();
        }   
    }
}
