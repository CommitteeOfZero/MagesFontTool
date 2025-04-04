using System.Buffers;
using FreeTypeSharp;
using static FreeTypeSharp.FT;

namespace ManagedFreeType;

public unsafe sealed class FontFace : IReferenceCounted, IDisposable {
	readonly FontLibrary _library;
	readonly MemoryHandle _dataHandle;
	readonly FT_FaceRec_* _handle;
	bool _disposed;

	internal FontFace(FontLibrary library, MemoryHandle dataHandle, FT_FaceRec_* handle) {
		_library = library;
		_dataHandle = dataHandle;
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
		Utils.ThrowIfError(FT_Done_Face(_handle));
		_dataHandle.Dispose();
		((IReferenceCounted)_library).RemoveReference();
	}

	public short Descender {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->descender;
		}
	}

	public short Ascender {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->ascender;
		}
	}

	public ushort UnitsPerEm {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->units_per_EM;
		}
	}

	public nint SizeMetricsAscender {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->size->metrics.ascender;
		}
	}

	public nint SizeMetricsDescender {
		get {
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _handle->size->metrics.descender;
		}
	}

	public FontGlyphSlot GetGlyph() {
		ObjectDisposedException.ThrowIf(_disposed, this);
		return new(_library, this, _handle->glyph);
	}

	public uint GetCharIndex(ulong code) {
		ObjectDisposedException.ThrowIf(_disposed, this);
		return FT_Get_Char_Index(_handle, checked((nuint)code));
	}

	public void LoadGlyph(uint index, FT_LOAD flags) {
		ObjectDisposedException.ThrowIf(_disposed, this);
		Utils.ThrowIfError(FT_Load_Glyph(_handle, index, flags));
	}

	public void RequestSize(FT_Size_RequestRec_ request) {
		ObjectDisposedException.ThrowIf(_disposed, this);
		Utils.ThrowIfError(FT_Request_Size(_handle, &request));
	}
}
