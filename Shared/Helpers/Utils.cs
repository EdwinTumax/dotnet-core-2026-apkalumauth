using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ApiKalumAuth.DTOs;
using ApiKalumAuth.Entities;
using ApiKalumAuth.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace ApiKalumAuth.Shared.Helpers
{
    public class Utils : IUtils
    {
        public UserTokenDTO BuildToken(ApplicationUser applicationUser, List<string> roles)
        {
            List<Claim> claims =  roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
            claims.Add(new Claim("username",applicationUser.UserName));
            claims.Add(new Claim("email", applicationUser.Email));            
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("019ec1e5-44d7-72d9-9d9d-bd28ea77dee0"));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            DateTime expiration = DateTime.UtcNow.AddHours(1);
            JwtSecurityToken token = new JwtSecurityToken(issuer: "kalum-auth", audience: "kalum-auth", claims: claims, expires: expiration, signingCredentials: credentials);
            return new UserTokenDTO()
            {
              Token = new JwtSecurityTokenHandler().WriteToken(token),
              Expiration = expiration  
            };
        }
    }
}