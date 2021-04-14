using System;
using System.IO;
using MailKit.Net.Smtp;
using MimeKit;
using PaintingClassServer;

namespace Server.Services.UserRegistration
{
	public static class EmailService
	{
		static EmailService()
		{
			smtpClient = new SmtpClient();
			smtpClient.Connect("smtp.gmail.com", 465, true);
			smtpClient.Authenticate("booldogsromania@gmail.com", "ciocolataTrebuieSaFieInterzisa");
		}

		private static SmtpClient smtpClient;
		private static string emailHtml = System.IO.File.ReadAllText(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Html\\email.html");

		public static void SendConfirmationEmail(RegisterUserData registerUserData, int requestId)
		{
			var msg = new MimeMessage();
			msg.From.Add(new MailboxAddress(Constants.registerSenderEmail));
			msg.To.Add(new MailboxAddress(registerUserData.email));
			msg.Subject = "Confirmare email";

			string emailBody = emailHtml.Replace("NUME_UTILIZATOR", registerUserData.name);
			
			emailBody = emailBody.Replace("LINK_CONFIRMARE", $"http://{Constants.publicIPAdress}:{Constants.httpPort}?email={registerUserData.email}&requestId={requestId}");

			msg.Body = new TextPart("html")
			{
				Text = emailBody,
			};
			smtpClient.Send(msg);
		}
	}
}
