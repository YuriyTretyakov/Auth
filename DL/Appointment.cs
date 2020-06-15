using ColibriWebApi.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace ColibriWebApi.DL
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }
        public User User { get; set; }
        public Product Product { get; set; }
        public double Price { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime Start { get; set; }                                            
        public DateTime End { get; set; }
        public AppointmentState State { get; set; }
        public string CancelationReason { get; set; }
        public string Comment { get; set; }
        public string Title { get; set; }
    }
}
