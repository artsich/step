using OpenTK.Mathematics;

namespace Step.Engine.Graphics;

public interface ICamera2d
{
	Matrix4 ViewProj { get; }
}
