using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiKalumAuth.DTOs;
using ApiKalumAuth.Enums;
using ApiKalumAuth.Repositories.Interfaces;
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
        private IUtils _utils;

        public RoleController(RoleManager<IdentityRole> roleManager, IMapper mapper, IUtils utils)
        {
            this._roleManager = roleManager;
            this._mapper = mapper;
            this._utils = utils;
        }

        [HttpGet]
        public async Task<ActionResult<List<RoleListDTO>>> Get()
        {
            long initialTime = DateTime.Now.Ticks;
            List<IdentityRole> roles = await this._roleManager.Roles.ToListAsync();
            if(roles == null || roles.Count == 0)
            {
                this._utils.Log(initialTime,"La consulta se realizo con éxito pero no se encontraron datos",204, TypeLog.INFORMATION, HttpContext, MethodLog.GET);
                return NoContent();
            }
            this._utils.Log(initialTime,"La consulta se realizo con éxito",200, TypeLog.INFORMATION, HttpContext, MethodLog.GET);
            return Ok(ApiResponseDTO<Object>.Ok(this._mapper.Map<List<RoleListDTO>>(roles)));
        }

        [HttpGet("{id}", Name = "GetById")]
        public async Task<ActionResult<RoleListDTO>> GetById(string id)
        {
            long initialTime = DateTime.Now.Ticks;
            var role = await this._roleManager.FindByIdAsync(id);
            if(role == null)
            {
                this._utils.Log(initialTime,$"No existe el rol con el id {id}",404, TypeLog.ERROR, HttpContext, MethodLog.GET);
                return NotFound(ApiResponseDTO<Object>.Fail("Error en busqueda por rol", new List<string>() { $"No existe el rol con el id {id}"}));
            }
            this._utils.Log(initialTime,$"La busqueda del rol fue realizada con exito para el id {id}",200, TypeLog.INFORMATION, HttpContext, MethodLog.GET);
            return Ok(ApiResponseDTO<Object>.Ok(this._mapper.Map<RoleListDTO>(role)));
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] RoleCreatedDTO roleCreatedDTO)    
        {
            long initialTime = DateTime.Now.Ticks;
            IdentityRole identityRole = this._mapper.Map<IdentityRole>(roleCreatedDTO);
            var result = await this._roleManager.CreateAsync(identityRole);
            if(result.Succeeded)
            {
                this._utils.Log(initialTime,"La creación del rol fue creado con exito",201, TypeLog.INFORMATION, HttpContext, MethodLog.POST);
                return new CreatedAtRouteResult("GetById", new {id = identityRole.Id});
            }
            this._utils.Log(initialTime,"Error al momento de crear el rol",400, TypeLog.ERROR, HttpContext, MethodLog.GET);
            return BadRequest(ApiResponseDTO<Object>.Fail("Error al crear role", result.Errors.Select(e => e.Description).ToList()));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            long initialTime = DateTime.Now.Ticks;
            var role = await this._roleManager.FindByIdAsync(id);
            if(role == null)
            {
                this._utils.Log(initialTime,$"Error en busqueda del rol con el id {id} para ser eliminado",400, TypeLog.ERROR, HttpContext, MethodLog.DELETE);
                return NotFound(ApiResponseDTO<Object>.Fail("Error en busqueda de role", new List<string>() {$"No existe el rol con el id {id}"}));
            }
            var result = await this._roleManager.DeleteAsync(role);
            if(result.Succeeded)
            {
                this._utils.Log(initialTime,$"El rol con el id {id} fue eliminado con éxito",204, TypeLog.INFORMATION, HttpContext, MethodLog.DELETE);
                return NoContent();
            }
            this._utils.Log(initialTime,$"Error al momento de eliminar el rol con el id {id}",400, TypeLog.ERROR, HttpContext, MethodLog.DELETE);
            return BadRequest(ApiResponseDTO<Object>.Fail("Error al intentar eliminar el rol", result.Errors.Select(e => e.Description).ToList()));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, [FromBody] RoleCreatedDTO roleUpdateDTO)
        {
            long initialTime = DateTime.Now.Ticks;
            var role = await this._roleManager.FindByIdAsync(id);
            if(role == null)
            {
                this._utils.Log(initialTime,$"No existe el rol con el id {id} para ser actualizado",404, TypeLog.ERROR, HttpContext, MethodLog.PUT);
                return NotFound(ApiResponseDTO<Object>.Fail("Error al actualizar rol", new List<string>() { $"No existe el rol con el id {id}"}));
            }
            role.Name = roleUpdateDTO.Name;
            var result = await this._roleManager.UpdateAsync(role);
            if(result.Succeeded)
            {
                this._utils.Log(initialTime,$"El con el id {id} fue actualizado con éxtio",204, TypeLog.INFORMATION, HttpContext, MethodLog.PUT);
                return NoContent();
            }
            this._utils.Log(initialTime,$"Error al momento de actualizar el rol con el id {id}",400, TypeLog.ERROR, HttpContext, MethodLog.PUT);
            return BadRequest(ApiResponseDTO<Object>.Fail("Error al actualizar", result.Errors.Select(e => e.Description).ToList()));
        }
        
    }
}