using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSGN.Application.Config
{
    public class UnixTimestampDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(
            ref Utf8JsonReader reader, 
            Type typeToConvert, 
            JsonSerializerOptions options)
        {
            long timestamp = reader.GetInt64();
            if (timestamp == 0)
                return null;

            return DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
        }

        public override void Write(Utf8JsonWriter writer, 
            DateTime? value, 
            JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteNumberValue(
                new DateTimeOffset(value.Value).ToUnixTimeSeconds()
            );
        }
    }
}
