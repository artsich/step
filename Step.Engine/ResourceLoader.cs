using System.Reflection;

namespace Step.Engine;

public static class EmbeddedResourceLoader
{
	private static Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

	public static IEnumerable<string> GetEmbeddedResourceNames()
	{
		return GetEmbeddedResourceNames(CurrentAssembly);
	}

	public static IEnumerable<string> GetEmbeddedResourceNames(Assembly assembly)
	{
		return assembly.GetManifestResourceNames();
	}

	public static Stream LoadEmbeddedResource(string resourceName)
	{
		return LoadEmbeddedResource(CurrentAssembly, resourceName);
	}

	public static Stream LoadEmbeddedResource(Assembly assembly, string resourceName)
	{
		if (assembly == null)
			throw new ArgumentNullException(nameof(assembly), "Assembly is missing! 🤦‍♂️");

		if (string.IsNullOrEmpty(resourceName))
			throw new ArgumentNullException(nameof(resourceName), "Resource name is missing! 🤔");

		var stream = assembly.GetManifestResourceStream(resourceName);
		if (stream == null)
			throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly {assembly.FullName} 😢");

		return stream;
	}

	public static string LoadAsString(string resourceName)
	{
		return LoadAsString(CurrentAssembly, resourceName);
	}

	public static string LoadAsString(Assembly assembly, string resourceName)
	{
		using var stream = LoadEmbeddedResource(assembly, resourceName);
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}

	public static byte[] LoadAsBytes(string resourceName)
	{
		return LoadAsBytes(CurrentAssembly, resourceName);
	}

	public static byte[] LoadAsBytes(Assembly assembly, string resourceName)
	{
		using var stream = LoadEmbeddedResource(assembly, resourceName);
		using var memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	public static IEnumerable<string> FindByExtension(string extension)
	{
		return FindByExtension(CurrentAssembly, extension);
	}

	public static IEnumerable<string> FindByExtension(Assembly assembly, string extension)
	{
		if (string.IsNullOrEmpty(extension))
			throw new ArgumentNullException(nameof(extension), "Extension is missing! 🤷‍♂️");

		if (!extension.StartsWith("."))
			extension = "." + extension;

		return GetEmbeddedResourceNames(assembly)
			.Where(name => name.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
	}
}