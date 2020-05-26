﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Authorization.ViewModels.Feedback
{
    public class RequestFeedback
    {
        [Required]
        public string Summary{ get; set; }
        [Required]
        public string Details { get; set; }
        [Required]
        public int Rate { get; set; }
        
    }
}
