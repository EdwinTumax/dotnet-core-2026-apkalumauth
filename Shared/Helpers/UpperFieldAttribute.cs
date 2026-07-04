using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiKalumAuth.Shared.Helpers
{
    public class UpperFieldAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }
            string text = value.ToString();            
            if (!char.IsUpper(text[0]))
            {
                return new ValidationResult(ErrorMessage ?? $"El valor del campo {validationContext.DisplayName} debe iniciar con letra inicial mayuscula");
            }
            return ValidationResult.Success;
        }
        
    }
}