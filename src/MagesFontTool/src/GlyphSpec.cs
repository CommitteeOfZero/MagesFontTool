using System.Collections.Immutable;

sealed class GlyphSpec {
	public readonly ImmutableArray<int> Units;
	public readonly string Text;
	public readonly bool FauxItalic;

	public GlyphSpec(ImmutableArray<int> units, string text, bool fauxItalic) {
		if (units.Length == 0) {
			throw new ArgumentException("Unit sequence must not be empty.", nameof(units));
		}
		Units = units;
		Text = text;
		FauxItalic = fauxItalic;
	}
}
