using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TourManagement.API.Helpers
{


    // ******************* This is a custom ModelBinder that will take an array of Guids coming in an Http Request
    //                      api/tours/{tourId}/showcollections/(id1,id2, … )
    //    and convert it to an IEnumerable<Guids> and place it in the Action's parameter showIds
    //          [Route("api/tours/{tourId}/showcollections")]
    //          ...
    //          [HttpGet("({showIds})", Name ="GetShowCollection")]
    //          public async Task<IActionResult> GetShowCollection( Guid tourId,
    //                        [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> showIds)
    //    The above says that for the showIds parameter, use the custom Model Binder to take the (id1, id2, etc) from the Http Request
    //    and stuff it in the showIds because the standard ModelBinder cannot deal with this unusual case ....
            
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // Our binder works only on enumerable types
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            // Get the inputted value through the value provider
            var value = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName).ToString();

            // If that value is null or whitespace, we return null
            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            // The value isn't null or whitespace, 
            // and the type of the model is enumerable. 
            // Get the enumerable's type, and a converter 
            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(elementType);

            // Convert each item in the value list to the enumerable type
            var values = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => converter.ConvertFromString(x.Trim()))
                .ToArray();

            // Create an array of that type, and set it as the Model value 
            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues;

            // return a successful result, passing in the Model 
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }
}
