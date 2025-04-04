using System.Text.Json.Serialization;

sealed class Config {
	[JsonPropertyName("input")]
	public required InputConfig Input { get; init; }
	[JsonPropertyName("output")]
	public required OutputConfig Output { get; init; }
}

sealed class InputConfig {
	[JsonPropertyName("glyphs-specification")]
	public required string GlyphsSpecification { get; init; }
	[JsonPropertyName("fonts")]
	public required Dictionary<string, Font> Fonts { get; init; }
}

sealed class Font {
	[JsonPropertyName("file")]
	public required string File { get; init; }
	[JsonPropertyName("center-line")]
	public required double CenterLine { get; init; }
	[JsonPropertyName("size")]
	public required int Size { get; init; }
	[JsonPropertyName("transform-matrix")]
	public Matrix2x2 TransformMatrix { get; init; } = new() {
		XX = 1,
		XY = 0,
		YX = 0,
		YY = 1,
	};
}

sealed class Matrix2x2 {
	[JsonPropertyName("xx")]
	public required double XX { get; init; }
	[JsonPropertyName("xy")]
	public required double XY { get; init; }
	[JsonPropertyName("yx")]
	public required double YX { get; init; }
	[JsonPropertyName("yy")]
	public required double YY { get; init; }
}

sealed class OutputConfig {
	[JsonPropertyName("bitmaps")]
	public required BitmapConfig[] Bitmaps { get; init; }
	[JsonPropertyName("metrics")]
	public required MetricsConfig Metrics { get; init; }
}

sealed class BitmapConfig {
	[JsonPropertyName("foreground")]
	public required string Foreground { get; init; }
	[JsonPropertyName("shadow")]
	public required string Shadow { get; init; }
	[JsonPropertyName("offset")]
	public required int Offset { get; init; }
	[JsonPropertyName("tile-count-x")]
	public required int TileCountX { get; init; }
	[JsonPropertyName("tile-count-y")]
	public required int TileCountY { get; init; }
	[JsonPropertyName("tile-size")]
	public required int TileSize { get; init; }
	[JsonPropertyName("tile-padding")]
	public required int TilePadding { get; init; }
}

sealed class MetricsConfig {
	[JsonPropertyName("path")]
	public required string Path { get; init; }

	[JsonPropertyName("offset")]
	public required int Offset { get; init; }

	[JsonPropertyName("count")]
	public required int Count { get; init; }

	[JsonPropertyName("scale")]
	public required double Scale { get; init; }
}
