using System;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Threading.Tasks;

namespace Pleisure
{
	public class BookingEmail
	{
		MailMessage msg;

		public BookingEmail(User user)
		{
			MailAddress src = new MailAddress("pleisurellc@gmail.com", "Pleisure");
			MailAddress dst = new MailAddress(user.Email, user.FullName);

			msg = new MailMessage(src, dst)
			{
				IsBodyHtml = true,
				SubjectEncoding = Encoding.UTF8,
				BodyEncoding = Encoding.UTF8
			};
		}

		public BookingEmail Event(Event evt)
		{
			msg.Subject = "Your booking confirmation for: " + evt.Title;
			msg.Body = "asdf";
			return this;
		}

		public Task Send()
		{
			return Task.Run(() => SmtpClient().Send(msg));
		}

		static SmtpClient SmtpClient()
		{
			SmtpClient client = new SmtpClient(
				"smtp.gmail.com",
				587
				);
			client.EnableSsl = true;
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			client.UseDefaultCredentials = false;
			client.Credentials = new NetworkCredential(
				"pleisurellc@gmail.com",
				"@ntua123"
				);
			return client;
		}
	}
}
