using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Authorization.DL;
using Authorization.Identity;
using Authorization.ViewModels.Feedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Controllers
{
    [Route("FeedBack")]
    public class FeedBackController : Controller
    {
        private readonly FeedBackRepository _feedbackRepo;
        private readonly UserManager<User> _userManager;

        public FeedBackController(FeedBackRepository feedbackRepo, UserManager<User> userManager)
        {
            _feedbackRepo = feedbackRepo;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost("Add")]
        public async  Task<IActionResult> AddFeedback([FromBody] RequestFeedback feedback)
        {
            if (!ModelState.IsValid)
                return BadRequest("Can't add feedback");

            var name = User.Claims.Where(c => c.Type == "Name").FirstOrDefault().Value;
            var picture = User.Claims.Where(c => c.Type == "Pict").FirstOrDefault().Value;

            await _feedbackRepo.AddFeedBack(feedback, name,picture);
                return Ok();
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var feedbacks= _feedbackRepo.GetAllFeedbacks();
            return Ok(feedbacks);
        }

        [HttpGet("get/{userid}")]
        public IActionResult GetByUser(string userid)
        {
            return Ok();
        }

    }
}