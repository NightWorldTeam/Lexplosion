using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Lexplosion.Tools
{
	static class ImageTools
	{
		public static byte[] ResizeImage(byte[] imageBytes, int width, int height)
		{
			if (imageBytes.Length == 0)
				return imageBytes;

			Image image;
			using (var ms = new MemoryStream(imageBytes))
			{
				image = Image.FromStream(ms);
			}

			var destRect = new Rectangle(0, 0, width, height);
			var destImage = new Bitmap(width, height);

			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(destImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using (var wrapMode = new ImageAttributes())
				{
					wrapMode.SetWrapMode(WrapMode.TileFlipXY);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
				}
			}

			using (MemoryStream stream = new MemoryStream())
			{
				destImage.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
				return stream.ToArray();
			}
		}

		public static byte[] GenerateRandomGradientImage(int width, int height)
		{
			var random = new Random();

			int value = random.Next(0, 16777215);
			int startR = (byte)value;
			int startG = (byte)(value >> 8);
			int startB = (byte)(value >> 16);
			MakeColorSaturated(ref startR, ref startG, ref startB);

			value = random.Next(0, 16777215);
			int endR = (byte)value;
			int endG = (byte)(value >> 8);
			int endB = (byte)(value >> 16);

			endR = ColorShift(startR, endR);
			endG = ColorShift(startG, endG);
			endB = ColorShift(startB, endB);

			var startColor = Color.FromArgb(startR, startG, startB);
			var endColor = Color.FromArgb(endR, endG, endB);

			return GenerateGradient(width, height, startColor, endColor, random.Next(0, 180));
		}

		private static void MakeColorSaturated(ref int r, ref int g, ref int b)
		{
			if (r > g && r > b)
			{
				if (g >= b) MakeColorSaturated(ref r, ref g);
				else MakeColorSaturated(ref r, ref b);
			}
			else if (g > r && g > b)
			{
				if (r >= b) MakeColorSaturated(ref g, ref r);
				else MakeColorSaturated(ref g, ref b);
			}
			else
			{
				if (r >= g) MakeColorSaturated(ref b, ref r);
				else MakeColorSaturated(ref b, ref g);
			}
		}

		private static void MakeColorSaturated(ref int maxColor, ref int minColor)
		{
			double diffRatio = maxColor / minColor;
			double ratio = diffRatio / 2;

			maxColor += (int)(maxColor * ratio);
			minColor -= (int)(minColor * ratio);

			if (minColor < 0)
			{
				int step = minColor * -1;
				minColor = 0;
				maxColor += step;
			}

			if (maxColor > 255) maxColor = 255;
		}

		private static int ColorShift(int compareColor, int targetColor)
		{
			double diffRatio;
			if (compareColor > targetColor) diffRatio = -1 * (targetColor / compareColor);
			else diffRatio = compareColor / targetColor;

			double ratio = diffRatio * 0.7;

			targetColor += (int)(targetColor * ratio);

			if (targetColor > 255) targetColor = 255;
			else if (targetColor < 0) targetColor = 0;

			return targetColor;
		}

		public static byte[] GenerateGradient(int width, int height, Color startColor, Color endColor, float angleDegrees = 0f)
		{
			using (Bitmap bmp = new Bitmap(width, height))
			{
				Rectangle rect = new Rectangle(0, 0, width, height);
				BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

				IntPtr ptr = bmpData.Scan0;
				int bytes = Math.Abs(bmpData.Stride) * height;
				byte[] rgbValues = new byte[bytes];

				// Преобразуем угол в радианы
				float angle = angleDegrees * (float)Math.PI / 180f;
				float cosAngle = (float)Math.Cos(angle);
				float sinAngle = (float)Math.Sin(angle);

				// Центр изображения
				float centerX = width / 2f;
				float centerY = height / 2f;

				// Максимальное расстояние от центра до угла
				float maxDistance = (float)Math.Sqrt(centerX * centerX + centerY * centerY);

				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						// Вычисляем позицию относительно центра
						float relX = x - centerX;
						float relY = y - centerY;

						// Проецируем точку на направление градиента
						float distance = relX * cosAngle + relY * sinAngle;

						// Нормализуем значение от 0 до 1
						float t = (distance + maxDistance) / (2 * maxDistance);
						t = Math.Max(0, Math.Min(1, t)); // Ограничиваем диапазон

						// Интерполируем цвета
						int r = Interpolate(startColor.R, endColor.R, t);
						int g = Interpolate(startColor.G, endColor.G, t);
						int b = Interpolate(startColor.B, endColor.B, t);
						int a = Interpolate(startColor.A, endColor.A, t);

						int position = y * bmpData.Stride + x * 4;
						rgbValues[position] = (byte)b;     // B
						rgbValues[position + 1] = (byte)g; // G
						rgbValues[position + 2] = (byte)r;  // R
						rgbValues[position + 3] = (byte)a;  // A
					}
				}

				Marshal.Copy(rgbValues, 0, ptr, bytes);
				bmp.UnlockBits(bmpData);
				return ImageToByte(bmp);
			}
		}

		private static int Interpolate(int start, int end, float t)
		{
			return (int)(start + (end - start) * t);
		}

		public static byte[] ImageToByte(Image img)
		{
			using (var stream = new MemoryStream())
			{
				img.Save(stream, ImageFormat.Png);
				return stream.ToArray();
			}
		}
	}
}
