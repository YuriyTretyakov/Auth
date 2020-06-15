using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColibriWebApi.Helpers
{
    public static class ModelStateHelper
    {
        public static IList<ModelError> GetErrors(this ModelStateDictionary modelState)
        {
            return modelState.Select(x => x.Value.Errors).Cast<ModelError>().ToList();
        }
    }
}
