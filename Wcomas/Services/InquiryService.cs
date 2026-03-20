using Microsoft.EntityFrameworkCore;
using Wcomas.Data;
using Wcomas.Models;

namespace Wcomas.Services;

public class InquiryService
{
    private readonly IDbContextFactory<WcomasDbContext> _dbFactory;
    private readonly WhatsAppNotificationService _whatsAppService;

    public InquiryService(IDbContextFactory<WcomasDbContext> dbFactory, WhatsAppNotificationService whatsAppService)
    {
        _dbFactory = dbFactory;
        _whatsAppService = whatsAppService;
    }

    public async Task SaveInquiryAsync(Inquiry inquiry)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        inquiry.Status = InquiryStatus.Unseen;
        context.Inquiries.Add(inquiry);
        await context.SaveChangesAsync();

        // Query counts by status for the notification
        var unseenCount    = await context.Inquiries.CountAsync(i => i.Status == InquiryStatus.Unseen);
        var acceptedCount  = await context.Inquiries.CountAsync(i => i.Status == InquiryStatus.Accepted);
        var processCount   = await context.Inquiries.CountAsync(i => i.Status == InquiryStatus.UnderProcess);
        var dispatchCount  = await context.Inquiries.CountAsync(i => i.Status == InquiryStatus.Dispatched);
        var doneCount      = await context.Inquiries.CountAsync(i => i.Status == InquiryStatus.Completed);

        // Fire-and-forget WhatsApp notification
        _ = _whatsAppService.SendInquiryNotificationAsync(
            inquiry.Name, inquiry.Email,
            unseenCount, acceptedCount, processCount, dispatchCount, doneCount);
    }

    public async Task<List<Inquiry>> GetAllInquiriesAsync()
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.Inquiries.OrderByDescending(i => i.CreatedAt).ToListAsync();
    }

    public async Task UpdateStatusAsync(int id, InquiryStatus status)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var inquiry = await context.Inquiries.FindAsync(id);
        if (inquiry != null)
        {
            inquiry.Status = status;
            inquiry.IsHandled = (status == InquiryStatus.Completed);
            await context.SaveChangesAsync();
        }
    }

    public async Task MarkAsHandledAsync(int id)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var inquiry = await context.Inquiries.FindAsync(id);
        if (inquiry != null)
        {
            inquiry.IsHandled = true;
            inquiry.Status = InquiryStatus.Completed;
            await context.SaveChangesAsync();
        }
    }
}
