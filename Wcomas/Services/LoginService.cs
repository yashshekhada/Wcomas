using System.Collections.Concurrent;

namespace Wcomas.Services
{
    public class LoginService
    {
        private readonly ConcurrentDictionary<string, LoginTracker> _trackers = new();
        private const int MaxAttempts = 3;
        private readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(30);

        public bool IsLockedOut(string username)
        {
            if (_trackers.TryGetValue(username, out var tracker))
            {
                if (tracker.LockoutEnd.HasValue && tracker.LockoutEnd.Value > DateTime.UtcNow)
                {
                    return true;
                }
                
                // Lockout expired, reset
                if (tracker.LockoutEnd.HasValue)
                {
                    ResetAttempts(username);
                }
            }
            return false;
        }

        public void RecordFailedAttempt(string username)
        {
            var tracker = _trackers.AddOrUpdate(username, 
                _ => new LoginTracker { Attempts = 1 },
                (_, existing) => {
                    existing.Attempts++;
                    if (existing.Attempts >= MaxAttempts)
                    {
                        existing.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                    }
                    return existing;
                });
        }

        public void ResetAttempts(string username)
        {
            _trackers.TryRemove(username, out _);
        }

        public DateTime? GetLockoutEnd(string username)
        {
            if (_trackers.TryGetValue(username, out var tracker))
            {
                return tracker.LockoutEnd;
            }
            return null;
        }

        private class LoginTracker
        {
            public int Attempts { get; set; }
            public DateTime? LockoutEnd { get; set; }
        }
    }
}
