using YamlDotNet.RepresentationModel;

namespace Step.Engine;

public interface ISerializable
{
	void DeserializeFromYaml(YamlNode data);
} 