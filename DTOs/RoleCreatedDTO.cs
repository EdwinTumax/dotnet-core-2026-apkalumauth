using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiKalumAuth.DTOs
{
    public class RoleCreatedDTO
    {
        [Required(ErrorMessage = "El campo name es requerido")]
        public string Name {get;set;}
    }
}