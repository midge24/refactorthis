using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace RefactorThis.Persistence.Models
{
    public class ReLeasedValidationResult: ValidationResult
    {
        public string ErrorCode { get; set; }

        public ReLeasedValidationResult(string errorMessage) : base(errorMessage)
        {
        }

        public ReLeasedValidationResult(string errorMessage, string errorCode) : base(errorMessage)
        {
            ErrorCode = errorCode;
        }

        public ReLeasedValidationResult(string errorMessage, IEnumerable<string> memberNames) : base(errorMessage, memberNames)
        {
        }

        public ReLeasedValidationResult(string errorMessage, string errorCode, IEnumerable<string> memberNames) : base(errorMessage, memberNames)
        {
            ErrorCode = errorCode;
        }
    }
}

