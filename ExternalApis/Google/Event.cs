using System;

namespace ColibriWebApi.ExternalApis.Google
{
    public class Event
    {
        public string Location { get; set; }
        public DateTime Start { get; set; }
        public string Status { get; set; }
        public DateTime? Created { get; set; }
        public DateTime  End { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public Atendees Attendees { get; set; }
    }

    public class Atendees
    {
        public string DisplayName { get; set; }
        public virtual string Email { get; set; }
    }
}
