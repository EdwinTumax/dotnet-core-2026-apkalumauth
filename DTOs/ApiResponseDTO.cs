using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiKalumAuth.DTOs
{
    public class ApiResponseDTO<T>
    {
        public bool Success {get;set;}
        public string Message {get;set;} = string.Empty;

        public T Data {get; set;}
        public List<string> Errors {get;set;}
        public static ApiResponseDTO<T> Ok(T data, string message = "Success") => new ApiResponseDTO<T> { Success = true, Data = data, Message = message};
        public static ApiResponseDTO<T> Fail(string message, List<string> errors = null) => new ApiResponseDTO<T> {Success = false, Message = message, Errors = errors};
    }
}