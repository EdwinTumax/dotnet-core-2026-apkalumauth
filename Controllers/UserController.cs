using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiKalumAuth.DBContext;
using ApiKalumAuth.DTOs;
using ApiKalumAuth.Entities;
using AutoMapper;
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
        private readonly IMapper _mapper;
        public UserController(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            this._userManager = userManager;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserListDTO>>> Get()
        {
            List<ApplicationUser> users = await this._userManager.Users.ToListAsync();
            if(users == null || users.Count == 0)
            {
                return NoContent();
            }
            List<UserListDTO> usersListDTO = this._mapper.Map<List<UserListDTO>>(users);
            return Ok(usersListDTO);
        }
        
        [HttpGet("{id}", Name ="GetUserById")]
        public async Task<ActionResult<UserListDTO>> GetById(String id)
        {
            IdentityUser user = await this._userManager.Users.FirstOrDefaultAsync(U => U.Id == id);
            if(user == null)
            {
                return NoContent();
            }
            return Ok(this._mapper.Map<UserListDTO>(user));

        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] UserCreatedDTO userCreatedDTO)
        {     
            ApplicationUser applicationUser = this._mapper.Map<ApplicationUser>(userCreatedDTO);
            var userNew = await this._userManager.CreateAsync(applicationUser,userCreatedDTO.Password);
            if(userNew.Succeeded)
            {
                var user = await this._userManager.FindByEmailAsync(userCreatedDTO.Email);
                return new CreatedAtRouteResult("GetUserById", new {id = user.Id});
            }
            return BadRequest("Error en el registro del usuario");            
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            ApplicationUser applicationUser = await this._userManager.FindByIdAsync(id);
            if(applicationUser == null)
            {
                return NotFound($"No existe el usuario con el id ${id}");
            }
            var result = await this._userManager.DeleteAsync(applicationUser);
            if(result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest("No se logro eliminar el registro");
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] UserUpdateDTO userUpdatedDTO)
        {
            ApplicationUser applicationUser = await this._userManager.FindByIdAsync(id);
            if(applicationUser == null)
            {
                return NotFound($"No existe el usuario con el id ${id}");
            }
            applicationUser.UserName = userUpdatedDTO.UserName ?? applicationUser.UserName;
            applicationUser.FirstName = userUpdatedDTO.FirstName ?? applicationUser.FirstName;
            applicationUser.LastName = userUpdatedDTO.LastName ?? applicationUser.LastName;
            applicationUser.PhoneNumber = userUpdatedDTO.PhoneNumber ?? applicationUser.PhoneNumber;
            var result = await this._userManager.UpdateAsync(applicationUser);
            if(result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest("Error al momento de actualizar la información");
        }

    }
}