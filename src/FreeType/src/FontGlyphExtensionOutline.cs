namespace FreeType;

public unsafe sealed class FontGlyphExtensionOutline : FontGlyphExtension {
	readonly FT_OutlineGlyphRec_* _handle;

	internal FontGlyphExtensionOutline(FontGlyph parent, FT_OutlineGlyphRec_* handle) : base(parent) {
		_handle = handle;
	}

	public FontOutline GetOutline() {
		ObjectDisposedException.ThrowIf(_disposed, this);
		return new FontOutlineReference(_parent, &_handle->outline);
	}
}
