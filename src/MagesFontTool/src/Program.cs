using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using FreeTypeSharp;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT_Pixel_Mode_;
using static FreeTypeSharp.FT_Render_Mode_;
using static FreeTypeSharp.FT_Size_Request_Type_;
using static FreeTypeSharp.FT_Stroker_LineCap_;
using static FreeTypeSharp.FT_Stroker_LineJoin_;
using ManagedFreeType;

static class Program {
	static readonly UTF8Encoding _strictUtf8 = new(
		encoderShouldEmitUTF8Identifier: false,
		throwOnInvalidBytes: true
	);

	static readonly FT_Matrix_ _fauxItalicMatrix = new() {
		xx = 65536 * 1,
		xy = 65536 * 1/3,
		yx = 65536 * 0,
		yy = 65536 * 1,
	};

	static readonly Option<string> _configOption = new(
		name: "--config"
	) {
		IsRequired = true,
	};

	static readonly Option<string> _outputOption = new(
		name: "--output"
	) {
		IsRequired = true,
	};

	static readonly RootCommand _rootCommand = new();

	static Program() {
		_rootCommand.AddGlobalOption(_configOption);
		_rootCommand.AddGlobalOption(_outputOption);
		_rootCommand.SetHandler(Run);
	}

	static async Task<int> Main(string[] args) {
		return await _rootCommand.InvokeAsync(args);
	}

	static async Task Run(InvocationContext context) {
		string configPath = context.ParseResult.GetValueForOption(_configOption)!;
		string outputPath = context.ParseResult.GetValueForOption(_outputOption)!;

		string configDirectory = Path.GetDirectoryName(configPath)!;
		Config config = JsonSerializer.Deserialize<Config>(await LoadBytes(configPath))!;

		ImmutableArray<GlyphSpec> glyphSpecs = await LoadGlyphSpecs(Path.Join(configDirectory, config.Input.GlyphsSpecification));

		int fontSize = config.Parameters.FontSize;
		int tileSize = config.Parameters.TileSize;
		int fontCenter = (int)Math.Round(config.Parameters.FontCenter*fontSize);

		Point offset = new((tileSize - fontSize) / 2, fontCenter + tileSize / 2);

		using FontLibrary library = new();

		using FontStroker stroker = library.NewStroker();
		using FontFace face = library.NewFace(await LoadBytes(Path.Join(configDirectory, config.Input.Font)), 0);
		using FontGlyphSlot slot = face.Glyph;

		face.RequestSize(new() {
			type = FT_SIZE_REQUEST_TYPE_NOMINAL,
			width = fontSize * 64,
			height = fontSize * 64,
			horiResolution = 72,
			vertResolution = 72,
		});

		Dictionary<int, byte> advanceWidths = [];
		Dictionary<int, PositionedImage<L8>[]> glyphForegrounds = [];
		Dictionary<int, PositionedImage<L8>[]> glyphShadows = [];

		HashSet<int> unitHistory = [];
		foreach (GlyphSpec glyphSpec in glyphSpecs) {
			if (glyphSpec.Units is not [int unit]) {
				throw new Exception("Rasterizing glyphs with less or more one unit is not supported.");
			}
			if (unitHistory.Contains(unit)) {
				throw new Exception($"Duplicate glyph unit: {unit}.");
			}
			unitHistory.Add(unit);

			Point penPoint = new();

			byte advanceWidth = 0;

			List<PositionedImage<L8>> foreground = [];
			List<PositionedImage<L8>> shadow = [];

			foreach (Rune r in glyphSpec.Text.EnumerateRunes()) {
				face.LoadGlyph(face.GetCharIndex((ulong)r.Value), FT_LOAD_NO_BITMAP | FT_LOAD_NO_HINTING);
				using FontGlyph glyph = slot.GetGlyph();
				using var extension = (FontGlyphExtensionOutline)glyph.GetExtension();

				using FontOutline outline = extension.GetOutline();

				if (glyphSpec.FauxItalic) {
					outline.Transform(_fauxItalicMatrix);
				}

				var glyphAdvanceWidth = (byte)Math.Round(glyph.Advance.x / 65536.0);
				advanceWidth += glyphAdvanceWidth;

				{
					PositionedImage<L8>? image = Rasterize(glyph);
					if (image is not null) {
						image.Point += (Size)penPoint;
						foreground.Add(image);
					}
				}

				for (int i = 1; i < 8; i++) {
					stroker.Set(64*i/2, FT_STROKER_LINECAP_ROUND, FT_STROKER_LINEJOIN_ROUND, 0);
					using FontGlyph strokedGlyph = glyph.Stroke(stroker);

					PositionedImage<L8>? image = Rasterize(strokedGlyph);
					if (image is not null) {
						Divide(image.Image, (byte)Math.Ceiling(i / 1.5f));
						image.Point += (Size)penPoint;
						shadow.Add(image);
					}
				}

				penPoint.X += glyphAdvanceWidth;
			}

			advanceWidths[unit] = advanceWidth;
			glyphForegrounds[unit] = [..foreground];
			glyphShadows[unit] = [..shadow];
		}

		Directory.CreateDirectory(outputPath);

		foreach (BitmapConfig bitmapConfig in config.Output.Bitmaps) {
			int tileCountX = bitmapConfig.TileCountX;
			int tileCountY = bitmapConfig.TileCountY;
			Image<L8> foregroundImage = new(tileSize*tileCountX, tileSize*tileCountY);
			Image<L8> shadowImage = new(tileSize*tileCountX, tileSize*tileCountY);
			for (int y = 0; y < tileCountY; y++) {
				for (int x = 0; x < tileCountX; x++) {
					int unit = bitmapConfig.Offset + tileCountX*y + x;
					if (!advanceWidths.ContainsKey(unit)) {
						continue;
					}
					Point origin = offset + new Size(tileSize*x, tileSize*y);
					PositionedImage<L8>[] foreground = glyphForegrounds[unit];
					PositionedImage<L8>[] shadow = glyphShadows[unit];
					foreach (PositionedImage<L8> image in glyphForegrounds[unit]) {
						DrawImage(foregroundImage, origin + (Size)image.Point, image.Image);
					}
					foreach (PositionedImage<L8> image in glyphShadows[unit]) {
						DrawImage(shadowImage, origin + (Size)image.Point, image.Image);
					}
				}
			}
			AlphaToRgba(foregroundImage).SaveAsPng(Path.Join(outputPath, bitmapConfig.Foreground));
			AlphaToRgba(shadowImage).SaveAsPng(Path.Join(outputPath, bitmapConfig.Shadow));
		}

		using (FileStream stream = File.Open(Path.Join(outputPath, config.Output.Metrics.Path), FileMode.Create)) {
			for (int i = 0; i < config.Output.Metrics.Count; i++) {
				int unit = config.Output.Metrics.Offset + i;
				byte value = advanceWidths.GetValueOrDefault(unit);
				stream.WriteByte(value);
			}
		}
	}

	static Image<Rgba32> AlphaToRgba(Image<L8> src) {
		Image<Rgba32> dst = new(src.Width, src.Height);
		dst.ProcessPixelRows(src, (dstAccessor, srcAccessor) => {
			for (int y = 0; y < dstAccessor.Height; y++) {
				Span<Rgba32> dstRow = dstAccessor.GetRowSpan(y);
				Span<L8> srcRow = srcAccessor.GetRowSpan(y);
				for (int x = 0; x < dstAccessor.Width; x++) {
					dstRow[x].A = srcRow[x].PackedValue;
					dstRow[x].R = 0xFF;
					dstRow[x].G = 0xFF;
					dstRow[x].B = 0xFF;
				}
			}
		});
		return dst;
	}

	static void Divide(Image<L8> image, byte divisor) {
		image.ProcessPixelRows(accessor => {
			for (int y = 0; y < accessor.Height; y++) {
				Span<L8> row = accessor.GetRowSpan(y);
				for (int x = 0; x < accessor.Width; x++) {
					row[x].PackedValue /= divisor;
				}
			}
		});
	}

	static void DrawImage(Image<L8> dstImage, Point dstPoint, Image<L8> srcImage) {
		dstImage.Mutate(context => {
			context.DrawImage(srcImage, dstPoint, PixelColorBlendingMode.Screen, 1f);
		});
	}

	sealed class PositionedImage<TPixel> where TPixel : unmanaged, IPixel<TPixel> {
		public Point Point;
		public Image<TPixel> Image;

		public PositionedImage(Point point, Image<TPixel> image) {
			Point = point;
			Image = image;
		}
	}

	static PositionedImage<L8>? Rasterize(FontGlyph glyph) {
		using FontGlyph rasterizedGlyph = glyph.ToBitmap(FT_RENDER_MODE_NORMAL, new() {
			x = 0,
			y = 0,
		});
		using FontGlyphExtensionBitmap extension = (FontGlyphExtensionBitmap)rasterizedGlyph.GetExtension();
		using FontBitmap bitmap = extension.GetBitmap();
		Image<L8>? image = ToImage(bitmap);
		if (image is null) {
			return null;
		}
		return new(
			new(extension.Left, -extension.Top),
			image
		);
	}

	static Image<L8>? ToImage(FontBitmap bitmap) {
		if (bitmap.PixelMode != FT_PIXEL_MODE_GRAY) {
			throw new NotImplementedException($"Pixel mode {bitmap.PixelMode} is not implemented.");
		}
		if (bitmap.NumGrays != 256 && bitmap.NumGrays != 255) {
			throw new NotImplementedException($"Number of grays {bitmap.NumGrays} is not implemented.");
		}
		int pitch = bitmap.Pitch;
		if (pitch < 0) {
			throw new NotImplementedException("Negative bitmap pitch is not implemented.");
		}
		int width = checked((int)bitmap.Width);
		int height = checked((int)bitmap.Rows);
		if (width == 0 || height == 0) {
			return null;
		}
		byte[] dstBuffer = new byte[bitmap.Width * bitmap.Rows];
		unsafe {
			ReadOnlySpan<byte> srcBuffer = new(bitmap.Buffer, checked(pitch * height));
			for (int y = 0; y < height; y++) {
				srcBuffer.Slice(pitch * y, width).CopyTo(dstBuffer.AsSpan(width * y, width));
			}
		}
		return Image.LoadPixelData<L8>(dstBuffer, width, height);
	}

	static async Task<ImmutableArray<GlyphSpec>> LoadGlyphSpecs(string path) {
		using JsonDocument document = JsonDocument.Parse(await LoadText(path));
		List<GlyphSpec> glyphs = [];
		foreach (JsonElement groupJson in document.RootElement.EnumerateArray()) {
			glyphs.AddRange(toGlyphSpecs(groupJson));
		}
		return [..glyphs];

		static ImmutableArray<GlyphSpec> toGlyphSpecs(JsonElement json) {
			JsonElement parametersJson = json.GetProperty("parameters").GetProperty("rasterization");
			if (parametersJson.ValueKind == JsonValueKind.Null) {
				return [];
			}
			bool fauxItalic = parametersJson.GetProperty("faux-italic").GetBoolean();

			List<GlyphSpec> glyphs = [];
			foreach (JsonElement glyphJson in json.GetProperty("glyphs").EnumerateArray()) {
				ImmutableArray<int> units = toUnits(glyphJson.GetProperty("units"));
				string text = toText(glyphJson.GetProperty("text"));
				glyphs.Add(new(units, text, fauxItalic));
			}
			return [..glyphs];
		}

		static ImmutableArray<int> toUnits(JsonElement json) {
			List<int> units = [];
			foreach (JsonElement unitJson in json.EnumerateArray()) {
				units.Add(unitJson.GetInt32());
			}
			return [..units];
		}

		static string toText(JsonElement json) {
			switch (json.ValueKind) {
				case JsonValueKind.String: {
					return json.GetString()!;
				}
				case JsonValueKind.Object: {
					return json.GetProperty("rasterization").GetString()!;
				}
				default: {
					throw new NotImplementedException($"{json.ValueKind} is not implemented.");
				}
			}
		}
	}

	static async Task<string> LoadText(string path) {
		return await File.ReadAllTextAsync(path, _strictUtf8);
	}

	static async Task<byte[]> LoadBytes(string path) {
		return await File.ReadAllBytesAsync(path);
	}
}
