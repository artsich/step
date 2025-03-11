using System.Text.Json;
using System.Text.Json.Serialization;

namespace Step.Engine.Converters;

public class Vector2JsonConverter : JsonConverter<Vector2f>
{
	public override Vector2f Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var array = JsonSerializer.Deserialize<float[]>(ref reader, options);
		return new Vector2f(array[0], array[1]);
	}

	public override void Write(Utf8JsonWriter writer, Vector2f value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		writer.WriteNumberValue(value.X);
		writer.WriteNumberValue(value.Y);
		writer.WriteEndArray();
	}
}
