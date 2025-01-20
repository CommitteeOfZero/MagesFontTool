using System.Runtime.InteropServices;
using FreeTypeSharp;

namespace ManagedFreeType;

[StructLayout(LayoutKind.Sequential)]
unsafe struct FT_OutlineGlyphRec_ {
	public FT_GlyphRec_ root;
	public FT_Outline_ outline;
}
