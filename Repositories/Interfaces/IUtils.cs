using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiKalumAuth.DTOs;
using ApiKalumAuth.Entities;
using ApiKalumAuth.Enums;

namespace ApiKalumAuth.Repositories.Interfaces
{
    public interface IUtils
    {
        public UserTokenDTO BuildToken(ApplicationUser applicationUser, List<string> roles);
        public void Log(long initialTime, string message, int responseCode, TypeLog typeLog, HttpContext httpContext, MethodLog methodLog);
    }
}