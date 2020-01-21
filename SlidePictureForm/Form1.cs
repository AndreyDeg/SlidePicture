using SlidePicture;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SlidePictureForm
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private int dataPixelCount;
		private int totalPixelCount;
		private int boxCount;
		private Slider ponyBox;

		private void button1_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				dataPixelCount = 0;
				boxCount = 1;
				var bitmapData = openFileDialog1.FileName.LoadBitmapFromFileAndSelect(color =>
				{
					//Проверка, что пиксель не прозрачный
					if (color.A > 0 && (color.R < 245 || color.G < 245 || color.B < 245))
					{
						dataPixelCount++;
						return true;
					}
					return false;
				});
				totalPixelCount = bitmapData.Length;

				ponyBox = new Slider(bitmapData);

				trackBar1.Value = 1;
				Draw(GetScale());
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (ponyBox != null)
			{
				boxCount++;
				ponyBox.Slide();
				Draw(GetScale());
			}
		}

		private float GetScale()
		{
			if (trackBar1.Value >= 1)
				return trackBar1.Value;
			else
				return -1f / (trackBar1.Value - 2); 
		}

		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			if (ponyBox != null)
				Draw(GetScale());
		}

		private void Draw(float scale = 1)
		{
			var bitmap = openFileDialog1.FileName.LoadBitmapFromFile(scale);

			var slidePixelCount = 0;
			using (var gr = Graphics.FromImage(bitmap))
			{
				foreach (var pb in ponyBox.RootBox.GetAllBoxes())
				{
					slidePixelCount += (pb.maxX - pb.minX + 1) * (pb.maxY - pb.minY + 1);
					gr.DrawRectangle(new Pen(Color.Red, 1),
						pb.minX * scale + 0.5f, pb.minY * scale + 0.5f,
						(pb.maxX - pb.minX + 1) * scale - 1, (pb.maxY - pb.minY + 1) * scale - 1);
				};
			}

			label1.Text = boxCount + ": " + (100f * slidePixelCount / totalPixelCount) + " Total: " + (100f * dataPixelCount / totalPixelCount);

			pictureBox1.AutoSize = true;
			pictureBox1.Image = bitmap;
		}
	}
}
