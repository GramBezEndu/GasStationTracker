using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GasStationTracker
{
    public class SingleValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(SingleValue));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            JToken token = JToken.Load(reader);
            if (token["Type"].ToString().Contains("Single"))
            {
                return serializer.Deserialize(reader, typeof(SingleValue<float>));
            }
            if (token["Type"].ToString().Contains("Int32"))
            {
                return serializer.Deserialize(reader, typeof(SingleValue<int>));
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
