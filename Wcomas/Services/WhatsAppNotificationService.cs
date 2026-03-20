using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Wcomas.Services;

public class WhatsAppNotificationService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public WhatsAppNotificationService(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public async Task SendInquiryNotificationAsync(string name, string email, int unseen, int accepted, int process, int dispatch, int done)
    {
        var sid = _config["Twilio:AccountSid"];
        var token = _config["Twilio:AuthToken"];
        var from = _config["Twilio:FromWhatsAppNumber"];
        var to = _config["Twilio:ToWhatsAppNumber"];
        var contentSid = _config["Twilio:ContentSid"];

        if (string.IsNullOrEmpty(sid) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(to) || string.IsNullOrEmpty(contentSid))
            return;

        var url = $"https://api.twilio.com/2010-04-01/Accounts/{sid}/Messages.json";
        
        var variables = new Dictionary<string, string> {
            { "1", name },
            { "2", email },
            { "3", unseen.ToString() },
            { "4", accepted.ToString() },
            { "5", process.ToString() },
            { "6", dispatch.ToString() },
            { "7", done.ToString() }
        };

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("To", to),
            new KeyValuePair<string, string>("From", from ?? ""),
            new KeyValuePair<string, string>("ContentSid", contentSid),
            new KeyValuePair<string, string>("ContentVariables", JsonSerializer.Serialize(variables))
        });

        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{sid}:{token}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

        try {
            await _httpClient.PostAsync(url, content);
        } catch { /* log or ignore */ }
    }
}
