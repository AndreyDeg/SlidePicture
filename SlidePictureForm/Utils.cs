using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SlidePictureForm
{
    public static class Utils
    {
        public static Bitmap LoadBitmapFromFile(this string fileName)
        {
            using (var bmpTemp = new Bitmap(fileName))
                return new Bitmap(bmpTemp);
        }

        public static Bitmap LoadBitmapFromFile(this string fileName, float scale)
        {
            using (var bmpTemp = new Bitmap(fileName))
            {
                var bitmap = new Bitmap((int)Math.Ceiling(bmpTemp.Width * scale), (int)Math.Ceiling(bmpTemp.Height * scale));
                using (var gr = Graphics.FromImage(bitmap))
                {
                    //gr.CompositingMode = CompositingMode.SourceCopy;
                    //gr.CompositingQuality = CompositingQuality.HighQuality;
                    gr.InterpolationMode = InterpolationMode.NearestNeighbor;
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gr.DrawImage(bmpTemp,
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        new Rectangle(0, 0, bmpTemp.Width, bmpTemp.Height),
                        GraphicsUnit.Pixel);
                }

                return bitmap;
            }
        }

        public static T[,] LoadBitmapFromFileAndSelect<T>(this string fileName, Func<Color, T> f)
        {
            using (var bitmap = new Bitmap(fileName))
            {
                T[,] result = new T[bitmap.Width, bitmap.Height];
                for (int x = 0; x < bitmap.Width; x++)
                    for (int y = 0; y < bitmap.Height; y++)
                        result[x, y] = f(bitmap.GetPixel(x, y));

                return result;
            }
        }
    }
}
