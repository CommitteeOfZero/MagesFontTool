using System.Text.Json.Serialization;

sealed class Config {
	[JsonPropertyName("input")]
	public required InputConfig Input { get; init; }
	[JsonPropertyName("parameters")]
	public required ParametersConfig Parameters { get; init; }
	[JsonPropertyName("output")]
	public required OutputConfig Output { get; init; }
}

sealed class InputConfig {
	[JsonPropertyName("glyphs-specification")]
	public required string GlyphsSpecification { get; init; }
	[JsonPropertyName("font")]
	public required string Font { get; init; }
}

sealed class ParametersConfig {
	[JsonPropertyName("tile-size")]
	public required int TileSize { get; init; }
	[JsonPropertyName("font-size")]
	public required int FontSize { get; init; }
	[JsonPropertyName("font-center")]
	public required double FontCenter { get; init; }
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
}

sealed class MetricsConfig {
	[JsonPropertyName("path")]
	public required string Path { get; init; }

	[JsonPropertyName("offset")]
	public required int Offset { get; init; }

	[JsonPropertyName("count")]
	public required int Count { get; init; }
}
