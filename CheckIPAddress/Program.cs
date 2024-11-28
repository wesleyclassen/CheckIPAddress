using System.Net;
using System.Net.Mail;

namespace CheckIPAddress;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var domain = "www.wesleyclassen.com";
        var toEmail = "wesley@gmail.com";
        var delay = TimeSpan.FromMinutes(5); // Adjust as appropriate

        while (true)
            try
            {
                var externalIp = await GetExternalIpAddressAsync();
                var dnsIp = GetDnsIpAddress(domain);

                if (externalIp != dnsIp)
                {
                    SendEmail(toEmail, "IP Address Mismatch", $"Current IP: {externalIp}, DNS IP: {dnsIp}");

                    while (externalIp != dnsIp)
                    {
                        Console.WriteLine("IP address mismatch, checking again in 5 minutes...");
                        await Task.Delay(delay);
                        externalIp = await GetExternalIpAddressAsync();
                        dnsIp = GetDnsIpAddress(domain);
                    }
                }

                Console.WriteLine("IP addresses match.");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                await Task.Delay(delay);
            }
    }

    private static async Task<string> GetExternalIpAddressAsync()
    {
        using (var client = new WebClient())
        {
            return await client.DownloadStringTaskAsync("https://api.ipify.org");
        }
    }

    private static string GetDnsIpAddress(string domain)
    {
        var addresses = Dns.GetHostAddresses(domain);
        return addresses.Length > 0 ? addresses[0].ToString() : string.Empty;
    }

    private static void SendEmail(string toEmail, string subject, string body)
    {
        var fromAddress = new MailAddress("wesley@gmail.com", "Wesley Classen"); // Add your email and name
        var toAddress = new MailAddress(toEmail);
        const string
            fromPassword =
                "thisisobviouslynotmypassword"; // Sign in with an app password -> https://support.google.com/accounts/answer/185833?hl=en

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        };

        using (var message = new MailMessage(fromAddress, toAddress)
               {
                   Subject = subject,
                   Body = body
               })
        {
            smtp.Send(message);
        }
    }
}