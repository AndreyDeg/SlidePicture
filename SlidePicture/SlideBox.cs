using System.Collections.Generic;

namespace SlidePicture
{
	public class SlideBox
	{
		public SlideBox[] Tree;
		public int minX, maxX, minY, maxY;

		public SlideBox(int _minX = 0, int _minY = 0, int _maxX = int.MaxValue, int _maxY = int.MaxValue)
		{
			minX = _minX;
			minY = _minY;
			maxX = _maxX;
			maxY = _maxY;
		}

		public IEnumerable<SlideBox> GetAllBoxes()
		{
			if (Tree == null)
				yield return this;
			else
				foreach (var node in Tree)
					foreach (var pb in node.GetAllBoxes())
						yield return pb;
		}
	}
}
