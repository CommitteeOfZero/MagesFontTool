using System.Text.Json.Serialization;

sealed class Config {
	[JsonPropertyName("input")]
	[JsonRequired]
	public InputConfig Input { get; set; } = default!;

	[JsonPropertyName("parameters")]
	[JsonRequired]
	public ParametersConfig Parameters { get; set; } = default!;

	[JsonPropertyName("output")]
	[JsonRequired]
	public OutputConfig Output { get; set; } = default!;
}

sealed class InputConfig {
	[JsonPropertyName("glyphs-specification")]
	[JsonRequired]
	public string GlyphsSpecification { get; set; } = default!;

	[JsonPropertyName("font")]
	[JsonRequired]
	public string Font { get; set; } = default!;
}

sealed class ParametersConfig {
	[JsonPropertyName("tile-size")]
	[JsonRequired]
	public int TileSize { get; set; } = default!;

	[JsonPropertyName("font-size")]
	[JsonRequired]
	public int FontSize { get; set; } = default!;

	[JsonPropertyName("font-center")]
	[JsonRequired]
	public double FontCenter { get; set; } = default!;
}

sealed class OutputConfig {
	[JsonPropertyName("bitmaps")]
	[JsonRequired]
	public BitmapConfig[] Bitmaps { get; set; } = default!;

	[JsonPropertyName("metrics")]
	[JsonRequired]
	public MetricsConfig Metrics { get; set; } = default!;
}

sealed class BitmapConfig {
	[JsonPropertyName("foreground")]
	[JsonRequired]
	public string Foreground { get; set; } = default!;

	[JsonPropertyName("shadow")]
	[JsonRequired]
	public string Shadow { get; set; } = default!;

	[JsonPropertyName("offset")]
	[JsonRequired]
	public int Offset { get; set; }

	[JsonPropertyName("tile-count-x")]
	[JsonRequired]
	public int TileCountX { get; set; }

	[JsonPropertyName("tile-count-y")]
	[JsonRequired]
	public int TileCountY { get; set; }
}

sealed class MetricsConfig {
	[JsonPropertyName("path")]
	[JsonRequired]
	public string Path { get; set; } = default!;

	[JsonPropertyName("offset")]
	[JsonRequired]
	public int Offset { get; set; }

	[JsonPropertyName("count")]
	[JsonRequired]
	public int Count { get; set; }
}
