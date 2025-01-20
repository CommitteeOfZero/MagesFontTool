using FreeTypeSharp;
using static FreeTypeSharp.FT;
using static FreeTypeSharp.FT_Glyph_Format_;

namespace ManagedFreeType;

public unsafe sealed class FontGlyph : IReferenceCounted, IDisposable {
	readonly FontLibrary _library;
	readonly FT_GlyphRec_* _handle;
	bool _disposed;

	internal FontGlyph(FontLibrary library, FT_GlyphRec_* handle) {
		_library = library;
		_handle = handle;
		((IReferenceCounted)_library).AddReference();
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		((IReferenceCounted)this).RemoveReference();
	}

	int IReferenceCounted.ReferenceCounter { get; set; } = 1;

	void IReferenceCounted.DisposeCore() {
		FT_Done_Glyph(_handle);
		((IReferenceCounted)_library).RemoveReference();
	}

	public FT_Vector_ Advance {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->advance;
		}
	}

	public FontGlyphExtension GetExtension() {
		ObjectDisposedException.ThrowIf(_disposed, this);
		return _handle->format switch {
			FT_GLYPH_FORMAT_BITMAP => new FontGlyphExtensionBitmap(this, (FT_BitmapGlyphRec_*)_handle),
			FT_GLYPH_FORMAT_OUTLINE => new FontGlyphExtensionOutline(this, (FT_OutlineGlyphRec_*)_handle),
			_ => throw new NotImplementedException($"Glyph format {_handle->format} extension is not implemented."),
		};
	}

	public FontGlyph Copy() {
		ObjectDisposedException.ThrowIf(_disposed, this);
		FT_GlyphRec_* glyphHandle;
		Utils.ThrowIfError(FT_Glyph_Copy(_handle, &glyphHandle));
		return new(_library, glyphHandle);
	}

	public void Transform(FT_Matrix_ matrix, FT_Vector_ delta) {
		ObjectDisposedException.ThrowIf(_disposed, this);
		Utils.ThrowIfError(FT_Glyph_Transform(_handle, &matrix, &delta));
	}

	public FontGlyph Stroke(FontStroker stroker) {
		ObjectDisposedException.ThrowIf(_disposed, this);
		FT_GlyphRec_* glyphHandle = _handle;
		Utils.ThrowIfError(FT_Glyph_Stroke(&glyphHandle, stroker.Handle, 0));
		return new(_library, glyphHandle);
	}

	public FontGlyph ToBitmap(FT_Render_Mode_ renderMode, FT_Vector_ origin) {
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (_handle->format == FT_GLYPH_FORMAT_BITMAP) {
			throw new InvalidOperationException("The glyph is already a bitmap.");
		}
		FT_GlyphRec_* glyphHandle = _handle;
		Utils.ThrowIfError(FT_Glyph_To_Bitmap(&glyphHandle, renderMode, &origin, 0));
		return new(_library, glyphHandle);
	}
}
