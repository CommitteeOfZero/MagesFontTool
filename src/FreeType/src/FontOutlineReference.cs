using FreeTypeSharp;

namespace FreeType;

public unsafe sealed class FontOutlineReference : FontOutline {
	readonly IReferenceCounted _parent;

	internal FontOutlineReference(IReferenceCounted parent, FT_Outline_* handle) : base(handle) {
		_parent = parent;
		parent.AddReference();
	}

	protected override void DisposeCore() {
		_parent.RemoveReference();
	}
}
