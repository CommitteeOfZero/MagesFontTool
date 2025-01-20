namespace FreeType;

public unsafe abstract class FontGlyphExtension : IDisposable {
	protected readonly FontGlyph _parent;
	protected bool _disposed;

	internal FontGlyphExtension(FontGlyph parent) {
		_parent = parent;
		((IReferenceCounted)_parent).AddReference();
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		((IReferenceCounted)_parent).RemoveReference();
	}
}
