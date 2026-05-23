using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiKalumAuth.DBContext;
using ApiKalumAuth.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiKalumAuth.Controllers
{
    [ApiController]
    [Route("kalum-auth/v1/user")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(UserManager<ApplicationUser> userManager)
        {
            this._userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<ApplicationUser>> Get()
        {
            List<ApplicationUser> users = await this._userManager.Users.ToListAsync();
            return Ok(users);
        }
        
    }
}