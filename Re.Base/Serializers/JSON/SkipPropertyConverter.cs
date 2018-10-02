using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Re.Base.Serializers.JSON
{
    public class SkipPropertyConverter<TModel> : Newtonsoft.Json.JsonConverter<TModel>
        where TModel : class
    {
        public override TModel ReadJson(JsonReader reader, Type objectType, TModel existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, TModel value, JsonSerializer serializer)
        {
            
        }
    }
}
