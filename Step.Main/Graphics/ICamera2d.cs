using OpenTK.Mathematics;

namespace Step.Main.Graphics;

public interface ICamera2d
{
	Matrix4 ViewProj { get; }
}
