using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authorization.ViewModels.Feedback
{
    public class ResponseFeedback
    {
        public string Summary { get; set; }
        public string Details { get; set; }
        public int Rate { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
