using ColibriWebApi.ExternalApis.Google;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ColibriWebApi.Helpers;

using Microsoft.Extensions.Configuration;
using ColibriWebApi.ViewModels.Scheduler;
using ColibriWebApi.Identity;

namespace ColibriWebApi.DL
{
    public class TimeSlot
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Duration { get; set; }
        public int Order { get; set; }
    }

    public class SchedulerRepository
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly GoogleApi _googleApi;
        private readonly AuthDbContext _context;
        private TimeSpan _workStart;
        private TimeSpan _workEnd;
        private readonly int _stepMinutes;

        public SchedulerRepository(IMapper mapper, 
                                   IConfiguration configuration,
                                   GoogleApi googleApi,
                                   AuthDbContext context)
        {
            _mapper = mapper;
            _configuration = configuration;
            _googleApi = googleApi;
            _context = context;
            _workStart = _configuration.GetValue<TimeSpan>("Scheduler:WorkStart");
            _workEnd = _configuration.GetValue<TimeSpan>("Scheduler:WorkEnd");
            _stepMinutes= _configuration.GetValue<int>("Scheduler:MinSlotDuration");
        }

        public async Task<List<Event>> GetBusyTimeSlots(DateTime start, DateTime end)
        {
            var events = await _googleApi.GetCalendarEvents(start, end, _workStart, _workEnd);
            var eventsData=_mapper.Map<Google.Apis.Calendar.v3.Data.Event[], List<Event>>(events);
            return eventsData;
        }

        public async Task<List<TimeSlot>> GetFreeTimeSlots(DateTime date)
        {
            var dateslots = GenerateWorkingDaySlots(date);
            dateslots.RemoveAll(free => free.Start < DateTime.Now);
            var busySlots = await GetBusyTimeSlots(date, date);

            foreach(var busySlot in busySlots)
            {
                dateslots.RemoveAll(free => free.Start.InRange(busySlot.Start, busySlot.End));
                dateslots.RemoveAll(free => free.End.InRange(busySlot.Start.AddMinutes(1), busySlot.End));
            }
            return dateslots;
        }

        public async Task<List<TimeSlot>> GetFreeTimeSlotsWithDuration(DateTime date,double duration)
        {
            var freeSlots = await GetFreeTimeSlots(date);
            List<TimeSlot> result = new List<TimeSlot>();

            if (freeSlots.Count == 0)
                return null;

            for(int i=0;i<freeSlots.Count;i++)
            {
                if (IsTimeSlotSequenceMatch(freeSlots, i, duration))
                    result.Add(UpdateSlotWithRequestedDuration(freeSlots[i],duration));
            }
            return result;
        }

        private TimeSlot UpdateSlotWithRequestedDuration(TimeSlot slot, double duration)
        {
            slot.Duration = TimeSpan.FromMinutes(duration);
            slot.End = slot.Start.Add(slot.Duration);
            return slot;
        }

        private bool IsTimeSlotSequenceMatch(List<TimeSlot> timeslots,int index,double duration)
        {
            double commulativeDurationMinutes=0;
            int prevInitialOrder = timeslots[index].Order;

            for (int i=index;i<=timeslots.Count-1; i++)
            {
                if (timeslots[i].Order > prevInitialOrder + 1)
                    return false;

                commulativeDurationMinutes += timeslots[i].Duration.TotalMinutes;
                prevInitialOrder = timeslots[i].Order;

                if (commulativeDurationMinutes >= duration)
                    return true;
            }
            return false;
        }


        private List<TimeSlot> GenerateWorkingDaySlots(DateTime date)
        {
            var workDuration = _workEnd.Subtract(_workStart);
            var iterationCount = (workDuration.TotalMinutes / _stepMinutes);
            List<TimeSlot> slots = new List<TimeSlot>();

            for (int i = 0; i <= iterationCount; i++)
            {
                var currentStart = _workStart.Add(TimeSpan.FromMinutes(i * _stepMinutes));
                var currentEnd = currentStart.Add(TimeSpan.FromMinutes(_stepMinutes));
                var timeSlot = new TimeSlot
                {
                    Start = date.Date.Add(currentStart),
                    End = date.Date.Add(currentEnd),
                    Duration = TimeSpan.FromMinutes(_stepMinutes),
                    Order = i
                };

                slots.Add(timeSlot);
            }
            return slots;
        }

        public async Task AddGoogleCalendarEvent(Product product,
                                                    DateTime start,
                                                    DateTime end,
                                                    User user,
                                                    double price,
                                                    string phonenumber)
        {

            if (!await IsTimeSlotStillFreeAsync(start, end))
                throw new ApplicationException("Time slot has been occupied");

            var summary = $"AutoCreated Appointment '{product.Title}' with {user.Name} {user.LastName} ";
            var phonenumberStr = string.IsNullOrWhiteSpace(phonenumber) ? "Not defined" : phonenumber;
            var description = $"Phone number: {phonenumberStr} Email:{user.Email} Price: {price}";
            await _googleApi.AddCalendarEvent(start, end, summary, description);


            var dbData = new Appointment
            {
                State = AppointmentState.Pending,
                Start = start.ToUniversalTime(),
                End = end.ToUniversalTime(),
                User = user,
                Price = price,
                PhoneNumber = phonenumber,
                Product = product,
                Comment = description,
                Title = summary
            };

            _context.Appointments.Add(dbData);

            await SaveChangesAsync();
        }

        private async Task<bool> IsTimeSlotStillFreeAsync(DateTime start, DateTime end)
        {
            var freeSlots = await GetFreeTimeSlotsWithDuration(start, end.Subtract(start).TotalMinutes);
            return (freeSlots.Any(f => f.Start == start && f.End == end));
        }

        public async Task<bool> SaveChangesAsync()
        {
            return ((await _context.SaveChangesAsync()) > 0);
        }

    }
}
