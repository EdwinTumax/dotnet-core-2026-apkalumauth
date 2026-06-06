using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiKalumAuth.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace ApiKalumAuth.DTOs
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ApplicationUser, UserListDTO>();
            CreateMap<UserCreatedDTO, ApplicationUser>();
            CreateMap<IdentityRole, RoleListDTO>();
            CreateMap<RoleCreatedDTO, IdentityRole>();
        }
    }
}