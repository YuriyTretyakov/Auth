using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColibriWebApi.DL
{
    public enum AppointmentState
    {
        Pending,
        Approved,
        CancelledByClient,
        CancelledByManager
    }
}
