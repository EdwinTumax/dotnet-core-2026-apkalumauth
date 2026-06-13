using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiKalumAuth.DTOs
{
    public class UserLoginDTO
    {
        [Required(ErrorMessage = "El Username es obligatorio")]
        public string UserName {get;set;}
        [Required(ErrorMessage = "El Password es obligatorio")]
        public string Password {get;set;}
    }
}