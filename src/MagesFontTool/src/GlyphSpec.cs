using System.Collections.Immutable;

sealed class GlyphSpec {
	public readonly ImmutableArray<int> Units;
	public readonly string Text;
	public readonly string Font;

	public GlyphSpec(ImmutableArray<int> units, string text, string font) {
		if (units.Length == 0) {
			throw new ArgumentException("Unit sequence must not be empty.", nameof(units));
		}
		Units = units;
		Text = text;
		Font = font;
	}
}
