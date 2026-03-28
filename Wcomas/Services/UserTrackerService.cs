using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Wcomas.Data;
using Wcomas.Models;

namespace Wcomas.Services
{
    public class UserSessionInfo
    {
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? CircuitId { get; set; }
    }

    public class ActiveUser
    {
        public string CircuitId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string CurrentUrl { get; set; } = "Home";
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
        public string Country { get; set; } = "Unknown";
        public string City { get; set; } = "Unknown";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsLocated { get; set; }
    }

    public class UserTrackerService
    {
        private readonly ConcurrentDictionary<string, ActiveUser> _activeUsers = new();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDbContextFactory<WcomasDbContext> _dbFactory;
        
        public UserTrackerService(IHttpClientFactory httpClientFactory, IDbContextFactory<WcomasDbContext> dbFactory)
        {
            _httpClientFactory = httpClientFactory;
            _dbFactory = dbFactory;
        }

        public event Action? OnChange;

        public List<ActiveUser> GetActiveUsers() => _activeUsers.Values.ToList();

        public void RegisterUser(string circuitId, string ip, string userAgent)
        {
            var user = new ActiveUser
            {
                CircuitId = circuitId,
                IpAddress = ip,
                UserAgent = userAgent,
                LastActive = DateTime.UtcNow,
                CurrentUrl = "Home"
            };
            
            _activeUsers[circuitId] = user;
            NotifyStateChanged();
            
            // Fire and forget geolocation and logging
            _ = LocateUserAsync(circuitId, ip);
        }

        public void UpdateUrl(string circuitId, string url)
        {
            if (_activeUsers.TryGetValue(circuitId, out var user))
            {
                user.CurrentUrl = string.IsNullOrEmpty(url) ? "Home" : url;
                user.LastActive = DateTime.UtcNow;
                NotifyStateChanged();
            }
        }

        public void RemoveUser(string circuitId)
        {
            _activeUsers.TryRemove(circuitId, out _);
            NotifyStateChanged();
        }

        private async Task LocateUserAsync(string circuitId, string ip)
        {
            if (ip == "::1" || ip == "127.0.0.1" || ip == "Unknown") return;

            try
            {
                var client = _httpClientFactory.CreateClient();
                // Use ip-api.com (free for development/small projects)
                var response = await client.GetFromJsonAsync<JsonElement>($"http://ip-api.com/json/{ip}");

                if (response.TryGetProperty("status", out var status) && status.GetString() == "success")
                {
                    if (_activeUsers.TryGetValue(circuitId, out var user))
                    {
                        user.Country = response.GetProperty("country").GetString() ?? "Unknown";
                        user.City = response.GetProperty("city").GetString() ?? "Unknown";
                        user.Latitude = response.GetProperty("lat").GetDouble();
                        user.Longitude = response.GetProperty("lon").GetDouble();
                        user.IsLocated = true;
                        
                        // Save to Persistent Log
                        using var db = await _dbFactory.CreateDbContextAsync();
                        db.VisitorLogs.Add(new VisitorLog
                        {
                            IpAddress = user.IpAddress,
                            UserAgent = user.UserAgent,
                            CurrentUrl = user.CurrentUrl,
                            Country = user.Country,
                            City = user.City,
                            Latitude = user.Latitude,
                            Longitude = user.Longitude,
                            IsLocated = true,
                            CreatedAt = DateTime.UtcNow
                        });
                        await db.SaveChangesAsync();
                        
                        NotifyStateChanged();
                    }
                }
            }
            catch { /* Ignore geolocation errors */ }
        }

        public async Task<List<VisitorLog>> GetHistoricalLogsAsync(int limit = 100)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            return await db.VisitorLogs.OrderByDescending(l => l.CreatedAt).Take(limit).ToListAsync();
        }

        public async Task<List<(DateTime Date, int Count)>> GetTrafficStatsAsync(int days = 7)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            
            var logs = await db.VisitorLogs
                .Where(l => l.CreatedAt >= startDate)
                .ToListAsync();

            return logs
                .GroupBy(l => l.CreatedAt.Date)
                .Select(g => (Date: g.Key, Count: g.Count()))
                .OrderBy(x => x.Date)
                .ToList();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }

    public class UserCircuitHandler : CircuitHandler
    {
        private readonly UserTrackerService _tracker;
        private readonly UserSessionInfo _sessionInfo;
        private string? _circuitId;

        public UserCircuitHandler(UserTrackerService tracker, UserSessionInfo sessionInfo)
        {
            _tracker = tracker;
            _sessionInfo = sessionInfo;
        }

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            _circuitId = circuit.Id;
            _sessionInfo.CircuitId = circuit.Id;
            _tracker.RegisterUser(circuit.Id, _sessionInfo.IpAddress ?? "Unknown", _sessionInfo.UserAgent ?? "Unknown");
            return base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            if (_circuitId != null)
            {
                _tracker.RemoveUser(_circuitId);
            }
            return base.OnCircuitClosedAsync(circuit, cancellationToken);
        }
    }
}
