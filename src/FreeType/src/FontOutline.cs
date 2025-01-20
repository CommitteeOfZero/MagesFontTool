using FreeTypeSharp;
using static FreeTypeSharp.FT;

namespace FreeType;

public unsafe abstract class FontOutline : IDisposable {
	protected readonly FT_Outline_* _handle;
	protected bool _disposed;

	internal FontOutline(FT_Outline_* handle) {
		_handle = handle;
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		DisposeCore();
	}

	protected abstract void DisposeCore();

	public void Transform(FT_Matrix_ matrix) {
		ObjectDisposedException.ThrowIf(_disposed, this);
		FT_Outline_Transform(_handle, &matrix);
	}
}
