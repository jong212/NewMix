using UnityEngine;

namespace Drawing.Glint
{
	public enum DrawMode : int
	{
		Lines = GL.LINES,
		LineStrip = GL.LINE_STRIP,
		Triangles = GL.TRIANGLES,
		TriangleStrip = GL.TRIANGLE_STRIP,
		Quads = GL.QUADS
	}

	public struct GLCommand
	{
		public DrawMode mode;
		public Color color;
		public Vector3[] verts;

		public GLCommand(DrawMode mode, Color color, params Vector3[] verts)
		{
			this.mode = mode;
			this.color = color;
			this.verts = verts;
		}
	}

	public interface ICommandInstruction
	{
		GLCommand ToCommand();
	}
}