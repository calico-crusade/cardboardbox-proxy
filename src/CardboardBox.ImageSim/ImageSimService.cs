using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Text;

namespace CardboardBox.ImageSim
{
	public interface IImageSimService
	{
		Task<string> HashImage(string path, int? resize = ImageSimService.RESIZE_DEFAULT);
		Task<string> HashImage(Stream input, int? resize = ImageSimService.RESIZE_DEFAULT);
	}

	public class ImageSimService : IImageSimService
	{
		public const int RESIZE_DEFAULT = 64;

		public async Task<string> HashImage(string path, int? resize = RESIZE_DEFAULT)
		{
			using var io = File.OpenRead(path);
			return await HashImage(io, resize);
		}

		public async Task<string> HashImage(Stream input, int? resize = RESIZE_DEFAULT)
		{
			using var image = await Image.LoadAsync<Rgba32>(input);

			if (resize != null)
			{
				var (w, h) = Ratio(image.Width, image.Height, resize.Value, resize.Value);
				image.Mutate(t => t.Resize(w, h));
			}

			var output = "";
			image.ProcessPixelRows(accessor =>
			{
				var bits = new List<bool>();
				for(int y = 0; y < accessor.Height; y++)
					foreach(ref var pixel in accessor.GetRowSpan(y))
					{
						int r = pixel.R, g = pixel.G, b = pixel.B;
						var brt = DetermineBrightness(r, g, b);
						bits.Add(brt < 0.5);
					}

				output = string.Join("", bits.Select(t => t ? "1" : "0"));
			});

			return output;
		}

		public IEnumerable<byte> FromBoolArray(IEnumerable<bool> data)
		{
			byte current = 0;
			int i = 0;
			foreach(var bit in data)
			{
				if (i == 8)
				{
					yield return current;
					current = 0;
					i = 0;
				}

				if (bit) current |= (byte)(1 << (7 - i));

				i++;
			}

			if (current != 0) yield return current;
		}

		public float DetermineBrightness(int r, int g, int b)
		{
			return (0.2126f * r + 0.7152f * g + 0.0722f * b);
		}

		public double DetermineRatio(int width, int height, int maxWidth, int maxHeight)
		{
			if (width <= maxWidth && height <= maxHeight) return 1;

			if (width > height) return (double)maxWidth / width;

			return (double)maxHeight / height;
		}

		public (int width, int height) Ratio(int width, int height, int maxWidth, int maxHeight)
		{
			var ratio = DetermineRatio(width, height, maxWidth, maxHeight);
			return ((int)(width * ratio), (int)(height * ratio));
		}
	}
}