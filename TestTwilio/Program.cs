using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

class Program
{
    static void Main(string[] args)
    {
        var accountSid = "YOUR_ACCOUNT_SID";
        var authToken = "YOUR_AUTH_TOKEN";
        TwilioClient.Init(accountSid, authToken);

        try 
        {
            var message = MessageResource.Create(
                body: "Test from console app",
                from: new PhoneNumber("whatsapp:+14155238886"),
                to: new PhoneNumber("whatsapp:+917777916487")
            );
            Console.WriteLine("Success: " + message.Sid);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
