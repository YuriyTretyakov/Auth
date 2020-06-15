using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ColibriWebApi.DL;
using ColibriWebApi.Helpers;
using ColibriWebApi.Identity;
using ColibriWebApi.ViewModels.Scheduler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ColibriWebApi.Controllers
{
    [Route("Scheduler")]
    public class SchedulerController : Controller
    {
        private readonly SchedulerRepository _scheduleRepo;
        private readonly UserManager<User> _userManager;
        private readonly ProductRepository _productRepo;

        public SchedulerController(SchedulerRepository scheduleRepo, UserManager<User> userManager, ProductRepository productRepo)
        {
            _scheduleRepo = scheduleRepo;
            _userManager = userManager;
            _productRepo = productRepo;
        }

        [Authorize]
        [HttpGet("GetBusyTimeSlot/{date}")]
        public async Task<IActionResult> GetBusyTimeSlot(string date)
        {
            if (DateTime.TryParse(date, out DateTime dateTime))
                return BadRequest($"invalid date: {date}");

            var busyTimeSlots = await _scheduleRepo.GetBusyTimeSlots(dateTime, dateTime);
            return Ok(busyTimeSlots);
        }

        [Authorize]
        [HttpGet("GetFreeTimeSlots/{date}")]
        public async Task<IActionResult> GetFreeTimeSlots(string date)
        {

            date = date.Replace("%2F", "/");

            if (!DateTime.TryParse(date, out DateTime dateTime))
                return BadRequest($"invalid date: {date}");

            if (dateTime.Equals(DateTime.MinValue))
                return BadRequest($"invalid date: {date}");

            var freeTimeslots = await _scheduleRepo.GetFreeTimeSlots(dateTime);

            return Ok(freeTimeslots);
        }

        [Authorize]
        [HttpPost("GetFreeTimeSlotsWithDuration")]
        public async Task<IActionResult> GetFreeTimeSlotsWithDuration([FromBody] TimeSlotWithDurationRequest request)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.GetErrors();
                return BadRequest(errors);
            }

            var dateTime = DateTime.ParseExact(request.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            if (request.Date.Equals(DateTime.MinValue))
                return BadRequest($"invalid date: {request.Date}");

            if (request.Duration.Equals(TimeSpan.MinValue))
                return BadRequest($"invalid Duration: {request.Duration}");

            var freeTimeslots = await _scheduleRepo.GetFreeTimeSlotsWithDuration(dateTime, request.Duration.TotalMinutes);

            return Ok(freeTimeslots);
        }

        [Authorize]
        [HttpPost("AddAppointment")]
        public async Task<IActionResult> AddAppointment([FromBody] AddAppointmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.GetErrors();
                return BadRequest(errors);
            }

            try
            {
                var userId = User.Claims.Where(c => c.Type == "ID").FirstOrDefault().Value;
                var user = await _userManager.FindByIdAsync(userId);
                var product = _productRepo.GetProductById(request.ProductId);
                await _scheduleRepo.AddGoogleCalendarEvent(product,
                                                            request.AppointmentStart,
                                                            request.AppointmentEnd,
                                                            user,
                                                            request.Price,
                                                            request.PhoneNumber
                                                            );
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}