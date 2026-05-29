using DataAccess.Abstract;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jobs.BackgroundServices
{
    public class ContractReminderService
    {   // : BackgroundService
        //private readonly IContractsDal _contractDal;
        //private readonly INotificationService _notificationSvc;

        //public ContractReminderService(IContractsDal contractDal, INotificationService notificationSvc)
        //{
        //    _contractDal = contractDal;
        //    _notificationSvc = notificationSvc;
        //}

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        var today = DateTime.UtcNow.Date;
        //        var threshold = today.AddMonths(1);
        //        var dueContracts = _contractDal.GetList(c =>
        //            c.IsActive &&
        //            c.EndDate.Date == threshold &&
        //            !c.ReminderSent
        //        );

        //        foreach (var contract in dueContracts)
        //        {
        //            // Bildirim gönder
        //            await _notificationSvc.SendAsync(
        //                to: contract.SupplierGuid,
        //                subject: $"Sözleşmeniz {contract.EndDate:dd.MM.yyyy} tarihinde sona eriyor",
        //                body: $"{contract.Title} başlıklı sözleşmeniz 1 ay sonra ({contract.EndDate:dd.MM.yyyy}) sona erecek."
        //            );

        //            // Tekrar göndermemek için işaretle
        //            contract.ReminderSent = true;
        //            _contractDal.Update(contract);
        //        }

        //        // Her gün bir kez çalışacak
        //        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        //    }
        //}
    }
}
