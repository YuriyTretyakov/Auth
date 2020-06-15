using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using ColibriWebApi.DL;
using ColibriWebApi.Identity;
using ColibriWebApi.ViewModels.Feedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ColibriWebApi.Controllers
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

            var userId= User.Claims.Where(c => c.Type == "ID").FirstOrDefault().Value;

            var user = await _userManager.FindByIdAsync(userId);

            await _feedbackRepo.AddFeedBack(feedback, user);
            return Ok(_feedbackRepo.GetAllFeedbacks());
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var feedbacks= _feedbackRepo.GetAllFeedbacks();
            return Ok(feedbacks);
        }

        [HttpGet("get/{pagenumber}/{pagesize}")]
        public IActionResult GetPagedFeedbacks(int pagenumber,int pagesize)
        {
            var feedbacks = _feedbackRepo.GetAllFeedbacks(pagenumber, pagesize);
            return Ok(feedbacks);
        }

        [HttpGet("get/{userid}")]
        public IActionResult GetByUser(string userid)
        {
            return Ok();
        }

        [Authorize]
        [HttpPost("AddCommentForFeedback")]
        public async Task<IActionResult> AddCommentForFeedback([FromBody] AddCommentRequest request)
        {

            if (!ModelState.IsValid)
                return BadRequest("Invalid data provided");

            var userId = User.Claims.Where(c => c.Type == "ID").FirstOrDefault().Value;
            var user = await _userManager.FindByIdAsync(userId);

            await _feedbackRepo.AddCommentToFeedback(request, user);

            return Ok(_feedbackRepo.GetAllFeedbacks());
        }
        

    }
}