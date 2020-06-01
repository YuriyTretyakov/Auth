using Authorization.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization.DL.FeedBack
{
    public class FeedBack
    {
        [Key]
        public int Id { get; set; }
        public string Summary { get; set; }
        public string Details { get; set; }
        public int Rate { get; set; }
        public User User { get; set; }
        public DateTime CreatedOn { get; set; }
        public ICollection<Response> Responses { get; set; }
    }

    public class Response
    {
        [Key]
        public int Id { get; set; }
        public int FeedbackId { get; set; }
        public string Details { get; set; }
        public User User { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
