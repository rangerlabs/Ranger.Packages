
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;

namespace Ranger.ApiUtilities {
    public class SemicolonDelimitedLngLatArrayModelBinder : IModelBinder
    {
        private readonly ILogger<SemicolonDelimitedLngLatArrayModelBinder> logger;

        public SemicolonDelimitedLngLatArrayModelBinder(ILogger<SemicolonDelimitedLngLatArrayModelBinder> logger)
        {
            this.logger = logger;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var key = bindingContext.ModelName;
            var value = bindingContext.ValueProvider.GetValue(key);
            if (value != ValueProviderResult.None)
            {
                var s = value.FirstValue;
                if (!String.IsNullOrWhiteSpace(s))
                {
                    try
                    {
                        var values = Array.ConvertAll
                        (
                            s.Split(';', StringSplitOptions.RemoveEmptyEntries), 
                            x => JsonConvert.DeserializeObject<LngLat>(x)
                        );
                        bindingContext.Result = ModelBindingResult.Success(values);
                    }
                    catch(Exception ex)
                    {
                        logger.LogDebug(ex, "Failed to bind comma delimited LngLats. Source: {parameter}", s);
                        bindingContext.Result = ModelBindingResult.Failed();
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}