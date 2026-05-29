using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Enum
{
    public enum RequestStatus
    {
        PendingApproval = 1,
        Approved = 2,
        Rejected = 3,
        Completed = 4,
        Canceled = -1,
        RejectionbyBuyer = -2,
    }
}
