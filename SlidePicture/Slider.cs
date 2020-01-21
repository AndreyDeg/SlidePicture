using System;
using System.Collections.Generic;

namespace SlidePicture
{
	public class Slider
	{
		public SlideBox RootBox;

		private readonly int[,] sumx;
		private readonly int[,] sumy;

		Dictionary<SlideBox, int> hashSquare = new Dictionary<SlideBox, int>();
		Dictionary<SlideBox, int> hashSquareInner = new Dictionary<SlideBox, int>();
		Dictionary<SlideBox, SlideBox[]> hashSlideX = new Dictionary<SlideBox, SlideBox[]>();
		Dictionary<SlideBox, SlideBox[]> hashSlideY = new Dictionary<SlideBox, SlideBox[]>();

		public Slider(bool[,] bitmap)
		{
			int width = bitmap.GetLength(0);
			int height = bitmap.GetLength(1);
			int[] sx = new int[width];
			int[] sy = new int[height];
			sumx = new int[width, height + 1];
			sumy = new int[width + 1, height];
			for (int x = 0; x < width; x++)
				for (int y = 0; y < height; y++)
				{
					if (bitmap[x, y])
					{
						sx[x]++;
						sy[y]++;
					}

					sumx[x, y + 1] = sx[x];
					sumy[x + 1, y] = sy[y];
				}

			RootBox = new SlideBox();
			CropPicture(RootBox);
		}

		private void CropPicture(SlideBox pb)
		{
			int W = sumx.GetLength(0) - 1;
			int H = sumy.GetLength(1) - 1;

			pb.minX = Math.Min(Math.Max(0, pb.minX), W);
			pb.maxX = Math.Min(Math.Max(0, pb.maxX), W);
			pb.minY = Math.Min(Math.Max(0, pb.minY), H);
			pb.maxY = Math.Min(Math.Max(0, pb.maxY), H);

			while (pb.minX <= W && sumx[pb.minX, pb.minY] == sumx[pb.minX, pb.maxY + 1]) pb.minX++;
			while (pb.minY <= H && sumy[pb.minX, pb.minY] == sumy[pb.maxX + 1, pb.minY]) pb.minY++;
			while (pb.maxX >= 0 && sumx[pb.maxX, pb.minY] == sumx[pb.maxX, pb.maxY + 1]) pb.maxX--;
			while (pb.maxY >= 0 && sumy[pb.minX, pb.maxY] == sumy[pb.maxX + 1, pb.maxY]) pb.maxY--;
			hashSquare[pb] = (pb.maxX - pb.minX + 1) * (pb.maxY - pb.minY + 1);

			int x1 = 0, y1 = 0;
			for (int x = pb.minX; x <= pb.maxX; x++) if (sumx[x, pb.minY] != sumx[x, pb.maxY + 1]) x1++;
			for (int y = pb.minY; y <= pb.maxY; y++) if (sumy[pb.minX, y] != sumy[pb.maxX + 1, y]) y1++;
			hashSquareInner[pb] = x1 * y1;
		}

		private SlideBox[] SlideX(SlideBox pb)
		{
			if (hashSlideX.TryGetValue(pb, out var hash))
				return hash;

			int betterX = 0;
			int minS = int.MaxValue;
			for (int x = pb.minX; x < pb.maxX;)
			{
				int x1 = x, x2 = x + 1;
				while (x2 < pb.maxX && sumx[x2, pb.minY] == sumx[x2, pb.maxY + 1]) x2++;

				int y1 = 0, y2 = 0;
				for (int i = pb.minY; i <= pb.maxY; i++) if (sumy[pb.minX, i] != sumy[x + 1, i]) y1++;
				for (int i = pb.minY; i <= pb.maxY; i++) if (sumy[x + 1, i] != sumy[pb.maxX + 1, i]) y2++;

				int S = (x1 - pb.minX + 1) * y1 + (pb.maxX - x2 + 1) * y2;
				if (minS > S)
				{
					betterX = x;
					minS = S;
				}

				x = x2;
			}

			var pbFirst = new SlideBox(pb.minX, pb.minY, betterX, pb.maxY);
			var pbSecond = new SlideBox(betterX + 1, pb.minY, pb.maxX, pb.maxY);
			CropPicture(pbFirst);
			CropPicture(pbSecond);

			return minS == int.MaxValue ? null : hashSlideX[pb] = new[] { pbFirst, pbSecond };
		}

		private SlideBox[] SlideY(SlideBox pb)
		{
			if (hashSlideY.TryGetValue(pb, out var hash))
				return hash;

			int betterY = 0;
			int minS = int.MaxValue;
			for (int y = pb.minY; y < pb.maxY;)
			{
				int x1 = 0, x2 = 0;
				for (int i = pb.minX; i <= pb.maxX; i++) if (sumx[i, pb.minY] != sumx[i, y + 1]) x1++;
				for (int i = pb.minX; i <= pb.maxX; i++) if (sumx[i, y + 1] != sumx[i, pb.maxY + 1]) x2++;

				int y1 = y, y2 = y + 1;
				while (y2 < pb.maxY && sumy[pb.minX, y2] == sumy[pb.maxX + 1, y2]) y2++;

				int S = x1 * (y1 - pb.minY + 1) + x2 * (pb.maxY - y2 + 1);
				if (minS > S)
				{
					betterY = y1;
					minS = S;
				}

				y = y2;
			}

			var pbFirst = new SlideBox(pb.minX, pb.minY, pb.maxX, betterY);
			var pbSecond = new SlideBox(pb.minX, betterY + 1, pb.maxX, pb.maxY);
			CropPicture(pbFirst);
			CropPicture(pbSecond);

			return minS == int.MaxValue ? null : hashSlideY[pb] = new[] { pbFirst, pbSecond };
		}

		public void Slide()
		{
			int maxS = 0;
			SlideBox betterPonyBox = null;
			SlideBox[] appendPonyBoxs = null;
			foreach (var pb in RootBox.GetAllBoxes())
			{
				var pbSlideX = SlideX(pb);
				if (pbSlideX != null)
				{
					var squareEmpty = hashSquare[pb] - hashSquare[pbSlideX[0]] - hashSquare[pbSlideX[1]];
					var squareInnerEmpty = hashSquareInner[pb] - hashSquareInner[pbSlideX[0]] - hashSquareInner[pbSlideX[1]];
					var SqX = Math.Max(squareEmpty, squareInnerEmpty);
					if (maxS < SqX)
					{
						maxS = SqX;
						betterPonyBox = pb;
						appendPonyBoxs = pbSlideX;
					}
				}

				var pbSlideY = SlideY(pb);
				if (pbSlideY != null)
				{
					var squareEmpty = hashSquare[pb] - hashSquare[pbSlideY[0]] - hashSquare[pbSlideY[1]];
					var squareInnerEmpty = hashSquareInner[pb] - hashSquareInner[pbSlideY[0]] - hashSquareInner[pbSlideY[1]];
					var SqY = Math.Max(squareEmpty, squareInnerEmpty);
					if (maxS < SqY)
					{
						maxS = SqY;
						betterPonyBox = pb;
						appendPonyBoxs = pbSlideY;
					}
				}
			};

			if (betterPonyBox != null)
			{
				betterPonyBox.Tree = appendPonyBoxs;
				hashSquare.Remove(betterPonyBox);
				hashSquareInner.Remove(betterPonyBox);
				hashSlideY.Remove(betterPonyBox);
				hashSlideY.Remove(betterPonyBox);
			}
		}
	}
}
