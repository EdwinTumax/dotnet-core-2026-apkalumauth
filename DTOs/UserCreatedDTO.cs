using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiKalumAuth.DTOs
{
    public class UserCreatedDTO
    {
        [Required(ErrorMessage = "El campo USERNAME es obligatorio")]
        public string UserName {get;set;}
        [Required(ErrorMessage = "El campo FIRSTNAME es obligatorio")]
        public string FirstName {set;get;}
        [Required(ErrorMessage = "El campo LASTNAME es obligatorio")]
        public string LastName {get;set;}
        [EmailAddress(ErrorMessage = "El correo electronico no es válido")]
        [Required(ErrorMessage = "El campo EMAIL es obligatorio")]
        public string Email {get;set;}
        public string PhoneNumber {set;get;}
        [Required(ErrorMessage = "El campo PASSWORD es obligatorio")]
        public string Password {get;set;}
    }
}