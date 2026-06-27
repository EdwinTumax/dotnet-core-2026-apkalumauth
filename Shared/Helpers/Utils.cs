using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ApiKalumAuth.DTOs;
using ApiKalumAuth.Entities;
using ApiKalumAuth.Enums;
using ApiKalumAuth.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace ApiKalumAuth.Shared.Helpers
{
    public class Utils : IUtils
    {
        private readonly ILogger<Utils> _logger;

        public Utils(ILogger<Utils> logger)
        {
            this._logger = logger;
        }
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

        public void Log(long initialTime, string message, int responseCode, TypeLog typeLog, HttpContext httpContext, MethodLog methodLog)
        {
            LogDTO log = new LogDTO();
            log.Name = "api-kalum-auth";
            log.HostName = httpContext.Request.Host.Value;
            httpContext.Request.Headers.TryGetValue("Authorization", out var apikey);
            log.ApiKey = String.IsNullOrEmpty(apikey) ? "" : apikey.ToString().Split(" ")[1].Split(".")[1];
            log.Uri = httpContext.Request.Path;
            log.ResponseCode = responseCode;
            log.ResponseTime = DateTime.Now.Ticks - initialTime;
            log.ClientIp = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? httpContext.Connection.RemoteIpAddress.ToString();
            log.Pid = "1";
            log.Method = methodLog.ToString();
            log.Message = message;
            log.DateTime = GetDateWithFormat(DateTime.Now);
            log.Version = 1;
            switch(typeLog)
            {
                case TypeLog.DEBUG:
                    log.Level = 10;
                    this._logger.LogDebug(JsonSerializer.Serialize(log));
                    break;
                case TypeLog.INFORMATION:
                    log.Level = 20;
                    this._logger.LogInformation(JsonSerializer.Serialize(log));
                    break;
                case TypeLog.WARNING:
                    log.Level = 30;
                    this._logger.LogWarning(JsonSerializer.Serialize(log));
                    break;
                case TypeLog.ERROR:
                    log.Level = 40;
                    this._logger.LogError(JsonSerializer.Serialize(log));
                    break;
                case TypeLog.CRITICAL:
                    log.Level = 50;
                    this._logger.LogCritical(JsonSerializer.Serialize(log));
                    break;
                default:
                    log.Level = 10;
                    this._logger.LogDebug(JsonSerializer.Serialize(log));
                    break;
            }
        }

        public string GetDateWithFormat(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        }
    }

}