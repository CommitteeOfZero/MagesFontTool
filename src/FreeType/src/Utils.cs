using FreeTypeSharp;

namespace FreeType;

static class Utils {
	public static void ThrowIfError(FT_Error error) {
		if (error != FT_Error.FT_Err_Ok) {
			throw new FreeTypeException(error);
		}
	}
}
