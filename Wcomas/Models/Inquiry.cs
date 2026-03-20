using System.ComponentModel.DataAnnotations;

namespace Wcomas.Models;

public enum InquiryStatus
{
    Unseen = 0,
    Seen = 1,
    Accepted = 2,
    UnderProcess = 3,
    Dispatched = 4,
    Completed = 5
}

public class Inquiry
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsHandled { get; set; } = false;

    public InquiryStatus Status { get; set; } = InquiryStatus.Unseen;
}
