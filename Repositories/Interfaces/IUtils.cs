using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiKalumAuth.DTOs;
using ApiKalumAuth.Entities;

namespace ApiKalumAuth.Repositories.Interfaces
{
    public interface IUtils
    {
        public UserTokenDTO BuildToken(ApplicationUser applicationUser, List<string> roles);
    }
}