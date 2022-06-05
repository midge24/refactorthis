﻿using RefactorThis.Helpers;
using RefactorThis.Persistence.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.DTO.Invoices
{
    public class GetInvoiceOutputDto
    {
        public IEnumerable<InvoicesDto> Invoices { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GetInvoiceInputDto : ValidationExtensions
    {
        [Display(Name = "Reference")]
        public string Reference { get; set; }

        /// <summary>
        /// Custom validation implementation
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override IEnumerable<ReLeasedValidationResult> Validate(ValidationContext context)
        {
            ValidationResults = new List<ReLeasedValidationResult>();

            if (string.IsNullOrEmpty(this.Reference))
            {
                ValidationResults.Add(new ReLeasedValidationResult("Reference is null", new[] { nameof(this.Reference) }));
            }

            this.SetValidated(); 
            
            return ValidationResults;
        }
    }
}
