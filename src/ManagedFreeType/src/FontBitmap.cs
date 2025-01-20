using FreeTypeSharp;

namespace ManagedFreeType;

public unsafe sealed class FontBitmap : IDisposable {
	readonly IReferenceCounted _parent;
	readonly FT_Bitmap_* _handle;
	bool _disposed;

	internal FontBitmap(IReferenceCounted parent, FT_Bitmap_* handle) {
		_parent = parent;
		_handle = handle;
		parent.AddReference();
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		_parent.RemoveReference();
	}

	public uint Rows {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->rows;
		}
	}

	public uint Width {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->width;
		}
	}

	public byte* Buffer {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->buffer;
		}
	}

	public int Pitch {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->pitch;
		}
	}

	public ushort NumGrays {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->num_grays;
		}
	}

	public FT_Pixel_Mode_ PixelMode {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->pixel_mode;
		}
	}
}
