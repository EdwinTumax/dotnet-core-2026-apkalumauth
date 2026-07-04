using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApiKalumAuth.Shared.Helpers
{
    public class PhonePrefix502Attribute : ValidationAttribute
    {
        public PhonePrefix502Attribute()
        {
            ErrorMessage = "El campo {0} debe iniciar con el prefijo 502";            
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessage!, name);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value is null)
            {
                return ValidationResult.Success;
            }
            
            var phone = value.ToString();
            if(string.IsNullOrWhiteSpace(phone))
            {
                return ValidationResult.Success;
            }

            //Elimna los espacios en blanco
            phone = Regex.Replace(phone, @"[\s\-\(\)]","");

            if(!phone.StartsWith("502"))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            return ValidationResult.Success;
        }
        
    }
}