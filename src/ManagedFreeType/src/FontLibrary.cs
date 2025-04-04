using System.Buffers;
using FreeTypeSharp;
using static FreeTypeSharp.FT;

namespace ManagedFreeType;

public unsafe sealed class FontLibrary : IReferenceCounted, IDisposable {
	readonly FT_LibraryRec_* _handle;
	bool _disposed;

	public FontLibrary() {
		FT_LibraryRec_* handle;
		Utils.ThrowIfError(FT_Init_FreeType(&handle));
		_handle = handle;
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
		Utils.ThrowIfError(FT_Done_FreeType(_handle));
	}

	internal FT_LibraryRec_* Handle {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle;
		}
	}

	public FontFace NewFace(ReadOnlySpan<byte> data, int index) {
		ObjectDisposedException.ThrowIf(_disposed, this);
		FT_FaceRec_* faceHandle;
		MemoryHandle dataHandle = new Memory<byte>(data.ToArray()).Pin();
		try {
			Utils.ThrowIfError(FT_New_Memory_Face(_handle, (byte*)dataHandle.Pointer, data.Length, index, &faceHandle));
		} catch {
			dataHandle.Dispose();
			throw;
		}
		return new(this, dataHandle, faceHandle);
	}

	public FontStroker NewStroker() {
		ObjectDisposedException.ThrowIf(_disposed, this);
		FT_StrokerRec_* strokerHandle;
		Utils.ThrowIfError(FT_Stroker_New(_handle, &strokerHandle));
		return new(this, strokerHandle);
	}

	public void SetSdfSpread(int spread) {
		ObjectDisposedException.ThrowIf(_disposed, this);
		fixed (byte* module = "sdf\0"u8) {
			fixed (byte* property = "spread\0"u8) {
				Utils.ThrowIfError(FT_Property_Set(_handle, module, property, &spread));
			}
		}
	}
}
