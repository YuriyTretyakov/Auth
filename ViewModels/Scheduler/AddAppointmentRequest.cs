using System;
using System.ComponentModel.DataAnnotations;

namespace ColibriWebApi.ViewModels.Scheduler
{
    public class AddAppointmentRequest
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public DateTime AppointmentStart { get; set; }
        [Required]
        public DateTime AppointmentEnd { get; set; }
        [Required]
        public double Price { get; set; }
        public string PhoneNumber { get; set; }
    }
}
