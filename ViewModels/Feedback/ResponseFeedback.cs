using System;

namespace Authorization.ViewModels.Feedback
{
    public class ResponseFeedback
    {
        public int Id { get; set; }
        public string Summary { get; set; }
        public string Details { get; set; }
        public int Rate { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public DateTime CreatedOn { get; set; }
        public Response[] Responses { get; set; }
    }

    public class Response
    {
        public string Name { get; set; }
        public string Picture { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Details { get; set; }
    }

}
