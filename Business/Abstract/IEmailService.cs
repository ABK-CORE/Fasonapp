using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
        Task SendVerificationCodeAsync(string toEmail, string code);
        Task SendPasswordResetCodeAsync(string toEmail, string code);
        Task SendOfferReceivedNotificationAsync(string toEmail, string tenderTitle);
        Task SendTenderWinnerNotificationAsync(string toEmail, string tenderTitle, string detailsUrl);
        Task SendApprovalPendingNotificationAsync(string toEmail, string tenderTitle, string detailsUrl);
        Task SendApprovalRejectedNotificationAsync(string toEmail, string tenderTitle, string detailsUrl);
        Task SendPurchaseRequestCreatedNotificationAsync(string toEmail, string requestTitle, string nextApproverName, string detailsUrl);
        Task SendPurchaseRequestPendingApprovalNotificationAsync(string toEmail, string requestTitle, string detailsUrl);
        Task SendPurchaseRequestProcurementApprovedNotificationAsync(string toEmail, string requestTitle, string detailsUrl);
        Task SendPurchaseRequestProcessedNotificationAsync(string toEmail, string requestTitle, string detailsUrl);
        Task SendPurchaseRequestRejectedNotificationAsync(string toEmail, string requestTitle, string detailsUrl);
        Task SendPendingApprovalReminderAsync(string toEmail, string approverName, int requestCount, string detailsUrl);
        Task SendDailySummaryToRequesterAsync(string toEmail, string requesterName, int requestCount, string summaryText, string detailsUrl);
    }
}
