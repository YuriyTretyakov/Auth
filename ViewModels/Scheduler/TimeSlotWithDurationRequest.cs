using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ColibriWebApi.ViewModels.Scheduler
{
    public class TimeSlotWithDurationRequest
    {
        [Required]
        public string Date { get; set; }
        [Required]
        public TimeSpan Duration { get; set; }
    }
}
