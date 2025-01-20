using System.Runtime.InteropServices;
using FreeTypeSharp;

namespace ManagedFreeType;

[StructLayout(LayoutKind.Sequential)]
unsafe struct FT_BitmapGlyphRec_ {
	public FT_GlyphRec_ root;
	public int left;
	public int top;
	public FT_Bitmap_ bitmap;
}
