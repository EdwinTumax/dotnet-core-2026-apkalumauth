using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiKalumAuth.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiKalumAuth.Controllers
{
    [ApiController]
    [Route("kalum-auth/v1/role")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private IMapper _mapper;

        public RoleController(RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            this._roleManager = roleManager;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<RoleListDTO>>> Get()
        {
            List<IdentityRole> roles = await this._roleManager.Roles.ToListAsync();
            if(roles == null || roles.Count == 0)
            {
                return NoContent();
            }
            return Ok(ApiResponseDTO<Object>.Ok(this._mapper.Map<List<RoleListDTO>>(roles)));
        }

        [HttpGet("{id}", Name = "GetById")]
        public async Task<ActionResult<RoleListDTO>> GetById(string id)
        {
            var role = await this._roleManager.FindByIdAsync(id);
            if(role == null)
            {
                return NotFound(ApiResponseDTO<Object>.Fail("Error en busqueda por rol", new List<string>() { $"No existe el rol con el id {id}"}));
            }
            return Ok(ApiResponseDTO<Object>.Ok(this._mapper.Map<RoleListDTO>(role)));
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] RoleCreatedDTO roleCreatedDTO)    
        {
            IdentityRole identityRole = this._mapper.Map<IdentityRole>(roleCreatedDTO);
            var result = await this._roleManager.CreateAsync(identityRole);
            if(result.Succeeded)
            {
                return new CreatedAtRouteResult("GetById", new {id = identityRole.Id});
            }
            return BadRequest(ApiResponseDTO<Object>.Fail("Error al crear role", result.Errors.Select(e => e.Description).ToList()));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var role = await this._roleManager.FindByIdAsync(id);
            if(role == null)
            {
                return NotFound(ApiResponseDTO<Object>.Fail("Error en busqueda de role", new List<string>() {$"No existe el rol con el id {id}"}));
            }
            var result = await this._roleManager.DeleteAsync(role);
            if(result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(ApiResponseDTO<Object>.Fail("Error al intentar eliminar el rol", result.Errors.Select(e => e.Description).ToList()));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, [FromBody] RoleCreatedDTO roleUpdateDTO)
        {
            var role = await this._roleManager.FindByIdAsync(id);
            if(role == null)
            {
                return NotFound(ApiResponseDTO<Object>.Fail("Error al actualizar rol", new List<string>() { $"No existe el rol con el id {id}"}));
            }
            role.Name = roleUpdateDTO.Name;
            var result = await this._roleManager.UpdateAsync(role);
            if(result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(ApiResponseDTO<Object>.Fail("Error al actualizar", result.Errors.Select(e => e.Description).ToList()));
        }
        
    }
}