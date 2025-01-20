using FreeTypeSharp;
using static FreeTypeSharp.FT;

namespace FreeType;

public unsafe sealed class FontGlyphSlot : IDisposable {
	readonly FontLibrary _library;
	readonly FontFace _face;
	readonly FT_GlyphSlotRec_* _handle;
	bool _disposed;

	internal FontGlyphSlot(FontLibrary library, FontFace face, FT_GlyphSlotRec_* handle) {
		_library = library;
		_face = face;
		_handle = handle;
		((IReferenceCounted)_face).AddReference();
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		((IReferenceCounted)_face).RemoveReference();
	}

	public FontBitmap Bitmap {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return new(_face, &_handle->bitmap);
		}
	}

	public FontGlyph GetGlyph() {
		ObjectDisposedException.ThrowIf(_disposed, this);
		FT_GlyphRec_* glyphHandle;
		Utils.ThrowIfError(FT_Get_Glyph(_handle, &glyphHandle));
		return new(_library, glyphHandle);
	}
}
