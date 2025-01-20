using FreeTypeSharp;
using static FreeTypeSharp.FT;

namespace FreeType;

public unsafe sealed class FontStroker : IDisposable {
	readonly FontLibrary _library;
	readonly FT_StrokerRec_* _handle;
	bool _disposed;

	internal FontStroker(FontLibrary library, FT_StrokerRec_* handle) {
		_library = library;
		_handle = handle;
		((IReferenceCounted)_library).AddReference();
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		FT_Stroker_Done(_handle);
		((IReferenceCounted)_library).RemoveReference();
	}

	internal FT_StrokerRec_* Handle {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle;
		}
	}

	public void Set(int radius, FT_Stroker_LineCap_ lineCap, FT_Stroker_LineJoin_ lineJoin, int miterLimit) {
		ObjectDisposedException.ThrowIf(_disposed, this);
		FT_Stroker_Set(_handle, radius, lineCap, lineJoin, miterLimit);
	}
}
