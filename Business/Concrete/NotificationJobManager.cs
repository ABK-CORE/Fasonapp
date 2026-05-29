using Business.Abstract;
using DataAccess.Abstract;
using Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class NotificationJobManager : INotificationJobService
    {
        private readonly IPurchaseRequestDal _requestDal;
        private readonly IRequestApprovalRecordDal _recordDal;
        private readonly IDepartmentUserDal _deptUserDal;
        private readonly IUserDal _userDal;
        private readonly IEmailService _emailService;

        public NotificationJobManager(
            IPurchaseRequestDal requestDal,
            IRequestApprovalRecordDal recordDal,
            IDepartmentUserDal deptUserDal,
            IUserDal userDal,
            IEmailService emailService)
        {
            _requestDal = requestDal;
            _recordDal = recordDal;
            _deptUserDal = deptUserDal;
            _userDal = userDal;
            _emailService = emailService;
        }

        public async Task SendPendingApprovalReminders()
        {
            var pendingRequests = _requestDal.GetList(
                pr => pr.Status == RequestStatus.PendingApproval,
                pr => pr.Department
            );

            if (!pendingRequests.Any())
            {
                return;
            }

            var notifications = new List<(string email, string title, string guid)>();

            foreach (var req in pendingRequests)
            {
                var nextStep = _recordDal.GetList(
                        r => r.RequestGuid == req.RequestGuid && r.IsApproved == null
                    )
                    .OrderBy(r => r.OrderIndex)
                    .FirstOrDefault();

                if (nextStep == null) continue;

                Entities.Concrete.EntityFramework.Entities.User approver = null;

                if (nextStep.StepType == StepType.User && nextStep.UserGuid.HasValue)
                {
                    approver = _userDal.Get(u => u.Guid == nextStep.UserGuid.Value);
                }
                else if (nextStep.StepType == StepType.ManagerLevel && nextStep.ManagerLevel.HasValue)
                {
                    var managerDeptUser = _deptUserDal.GetList(
                        du => du.DepartmentId == req.DepartmentId &&
                              du.ManagerLevel == nextStep.ManagerLevel.Value &&
                              du.User.IsActive,
                        du => du.User
                    ).FirstOrDefault();

                    if (managerDeptUser != null)
                    {
                        approver = managerDeptUser.User;
                    }
                }

                if (approver != null && !string.IsNullOrEmpty(approver.Email))
                {
                    notifications.Add((approver.Email, req.Title, req.RequestGuid.ToString()));
                }
            }

            var groupedByApprover = notifications.GroupBy(n => n.email);

            foreach (var group in groupedByApprover)
            {
                var approverEmail = group.Key;
                var requestCount = group.Count();
                var user = _userDal.Get(u => u.Email == approverEmail);
                var detailsUrl = $"https://po.abkcore.com/home";

                await _emailService.SendPendingApprovalReminderAsync(
                    approverEmail,
                    $"{user?.FirstName} {user?.LastName}",
                    requestCount,
                    detailsUrl
                );
            }
        }

        public async Task SendDailySummariesToRequesters()
        {
            var pendingRequests = _requestDal.GetList(
                pr => pr.Status == RequestStatus.PendingApproval
            );

            if (!pendingRequests.Any())
            {
                return;
            }

            var groupedByRequester = pendingRequests.GroupBy(pr => pr.CreatedBy);

            foreach (var group in groupedByRequester)
            {
                var requesterGuid = group.Key;
                var requester = _userDal.Get(u => u.Guid == requesterGuid);

                if (requester == null || string.IsNullOrEmpty(requester.Email)) continue;

                var summaryLines = new List<string>();
                foreach (var req in group)
                {
                    summaryLines.Add($" - '{req.Title}' (Erstellt am: {req.CreatedDate:dd.MM.yyyy})");
                }

                var summaryText = string.Join("\n", summaryLines);
                var detailsUrl = $"https://po.abkcore.com/purchase-requests";

                await _emailService.SendDailySummaryToRequesterAsync(
                    requester.Email,
                    $"{requester.FirstName} {requester.LastName}",
                    group.Count(),
                    summaryText,
                    detailsUrl
                );
            }
        }
    }
}
