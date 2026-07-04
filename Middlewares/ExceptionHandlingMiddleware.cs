using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ApiKalumAuth.DTOs;
using ApiKalumAuth.Enums;
using ApiKalumAuth.Repositories.Interfaces;

namespace ApiKalumAuth.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        private readonly IUtils _utils;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IUtils utils)
        {
            this._next = next;
            this._logger = logger;
            this._utils = utils;
        }

        public async Task Invoke(HttpContext context)
        {
            long initialTime = 0;
            try
            {
                initialTime = DateTime.Now.Ticks;
                await _next(context);
            } 
            catch(Exception ex)
            {
                Console.WriteLine($"Response HasStarted: {context.Response.HasStarted}");
                Console.WriteLine($"Response ContentLength: {context.Response.ContentLength}");
                if(ex is Microsoft.Data.SqlClient.SqlException)
                {
                    context.Response.StatusCode = 503;
                    context.Response.ContentType = "Application/json";
                    var response = ApiResponseDTO<Object>.Fail("Ha ocurrido un error en el legado de SQL Server", new List<string>{"Erorr de conexión hacia la base de datos de SQL Server"});
                    var json = JsonSerializer.Serialize(response);
                    await context.Response.WriteAsync(json);                   
                } 
                else
                {
                    if(!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "Application/json";
                        var response = ApiResponseDTO<Object>.Fail("Ha ocurrido un error interno en el servidor", new List<string>{ex.Message});
                        await context.Response.WriteAsJsonAsync(response);                        
                    }
                }
                Enum.TryParse<MethodLog>(context.Request.Method.ToString(), out MethodLog method);
                this._utils.Log(initialTime,ex.Message,500,TypeLog.ERROR,context, method);
            }
        }
        
    }
}