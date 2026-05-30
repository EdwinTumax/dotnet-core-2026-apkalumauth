using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiKalumAuth.DTOs
{
    public class UserCreatedDTO
    {
        public string UserName {get;set;}
        public string FirstName {set;get;}
        public string LastName {get;set;}
        public string Email {get;set;}
        public string PhoneNumber {set;get;}
        public string Password {get;set;}
    }
}