using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.Rest.Models;

namespace Umbraco.Web.Rest
{
    public class ModelValidationException : Exception
    {
        public ValidationErrorListRepresentation Errors { get; private set; }
        public int? Id { get; set; }

        public ModelValidationException(ValidationErrorListRepresentation errors, int? id = null)
        {
            Errors = errors;
            Id = id;
        }
    }
}
