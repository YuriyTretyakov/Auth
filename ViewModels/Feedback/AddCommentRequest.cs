using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Authorization.ViewModels.Feedback
{
    public class AddCommentRequest
    {
        [Required]
        public int FeedbackId { get; set; }
        [Required]
        public string Text { get; set; }
    }
}
