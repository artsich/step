using System.Text.Json;
using System.Text.Json.Serialization;

namespace Step.Engine.Converters;

public class Vector4JsonConverter : JsonConverter<Vector4f>
{
	public override Vector4f Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var array = JsonSerializer.Deserialize<float[]>(ref reader, options);
		return new Vector4f(array[0], array[1], array[2], array[3]);
	}

	public override void Write(Utf8JsonWriter writer, Vector4f value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		writer.WriteNumberValue(value.X);
		writer.WriteNumberValue(value.Y);
		writer.WriteNumberValue(value.Z);
		writer.WriteNumberValue(value.W);
		writer.WriteEndArray();
	}
}
