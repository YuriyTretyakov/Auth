using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ColibriWebApi.Identity;
using ColibriWebApi.ViewModels.UserData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ColibriWebApi.Controllers
{
    public class UserDataController : Controller
    {
        private readonly UserManager<User> _userManager;

        public UserDataController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("GetUser/{id}")]
        public async Task<IActionResult> GetUserInfo(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("User id should be specified");

            var identityUser = await _userManager.FindByIdAsync(id);

            if (identityUser==null)
                return NotFound("Can't find the user by provided id");

            return Ok(new UserData
            {
                Name = identityUser.Name,
                LastName = identityUser.LastName,
                UserPic = identityUser.UserPicture,
                Id=identityUser.Id,
                Email=identityUser.Email,
                PhoneNumber=identityUser.PhoneNumber
            });
}
    }
}