using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using GoogleCalendarEvent = Google.Apis.Calendar.v3.Data.Event;
namespace ColibriWebApi.ExternalApis.Google
{
    



    public class GoogleApi
    {

        private readonly string _applicationName;
        private readonly string _calendarId;
        private readonly string _clientEmail;
        private readonly string _privateKey;
        private readonly string _timeZone;

        public class CalendarServiceContainer
        {
            private readonly string _clientemail;
            private readonly string _privatekey;
            private readonly string _appname;

            private CalendarServiceContainer() { }

            public CalendarServiceContainer(string clientemail, string privatekey, string appname)
            {
                _clientemail = clientemail;
                _privatekey = privatekey;
                _appname = appname;
            }

            public CalendarService GetService()
            {
                var credential =
                    new ServiceAccountCredential(
                    new ServiceAccountCredential.Initializer(_clientemail)
                    {
                        Scopes = new string[] { CalendarService.Scope.Calendar,
                       CalendarService.Scope.CalendarEvents }
                    }.FromPrivateKey(_privatekey));

                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _appname,
                });

                return service;
            }
        }

        readonly Lazy<CalendarServiceContainer> _lazyCalendar;
            

        public GoogleApi(IConfiguration configuration)
        {
            _applicationName= configuration.GetValue<string>("GoogleCalendar:ApplicationName");
            _calendarId= configuration.GetValue<string>("GoogleCalendar:CalendarId");
            _clientEmail = configuration.GetValue<string>("GoogleCalendar:ClientEmail");
            _privateKey = configuration.GetValue<string>("GoogleCalendar:PrivateKey");
            _timeZone = configuration.GetValue<string>("GoogleCalendar:TimeZone");

            _lazyCalendar = new Lazy<CalendarServiceContainer>(
                () => new CalendarServiceContainer(_clientEmail, _privateKey, _applicationName), true);
        }



        public async Task<GoogleCalendarEvent[]> GetCalendarEvents(DateTime start, DateTime end, TimeSpan timestart, TimeSpan timeend)
        {
            var service = _lazyCalendar.Value.GetService();
            EventsResource.ListRequest request = service.Events.List(_calendarId);
            request.TimeMax = start.Date.Add(timeend);
            request.TimeMin = end.Date.Add(timestart);
            request.ShowHiddenInvitations = true;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            request.TimeZone = _timeZone;
            Events events = await request.ExecuteAsync();

            return events.Items.ToArray();
        }

        public async Task AddCalendarEvent(DateTime start, DateTime end,string summary,string description)
        {
            var calendarEvent = new GoogleCalendarEvent
            {
                Start = new EventDateTime
                {
                    DateTime = start
                },
                End = new EventDateTime
                {
                    DateTime = end
                },
                Summary = summary,
                Description = description
            };
            //TODO: implement atendees;
            var service = _lazyCalendar.Value.GetService();
            await service.Events.Insert(calendarEvent, _calendarId).ExecuteAsync();
        }
    }
}
