namespace ManagedFreeType;

public unsafe sealed class FontGlyphExtensionBitmap : FontGlyphExtension {
	readonly FT_BitmapGlyphRec_* _handle;

	internal FontGlyphExtensionBitmap(FontGlyph parent, FT_BitmapGlyphRec_* handle) : base(parent) {
		_handle = handle;
	}

	public int Left {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->left;
		}
	}

	public int Top {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->top;
		}
	}

	public FontBitmap GetBitmap() {
		ObjectDisposedException.ThrowIf(_disposed, this);
		return new(_parent, &_handle->bitmap);
	}
}
