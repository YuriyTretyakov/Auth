using System;

namespace Authorization.DL.FeedBack
{
    public class FeedBack
    {
        public int Id { get; set; }
        public string Summary { get; set; }
        public string Details { get; set; }
        public int Rate { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
