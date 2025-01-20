using System.Runtime.InteropServices;
using FreeTypeSharp;

namespace FreeType;

[StructLayout(LayoutKind.Sequential)]
unsafe struct FT_BitmapGlyphRec_ {
	public FT_GlyphRec_ root;
	public int left;
	public int top;
	public FT_Bitmap_ bitmap;
}
