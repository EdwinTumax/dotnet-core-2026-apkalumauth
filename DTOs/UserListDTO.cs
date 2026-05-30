using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiKalumAuth.DTOs
{
    public class UserListDTO
    {
        public string Id {get;set;}
        public string FirstName {get;set;}
        public string LastName {get;set;}
        public string UserName {set;get;}
        public string Email {set;get;}
        public string PhoneNumber {get;set;}        
     }
}