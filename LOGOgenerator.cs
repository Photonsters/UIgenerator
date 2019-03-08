using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

public class Encoder
{
		private static uint SPI_FILE_MAGIC_BASE = 318570496u; //0x12FD0000

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct spi_file_head_t
		{
			public uint magic_num;
			public uint file_size;
		}

		public byte[] StructToBytes(object obj)
		{
			int num = Marshal.SizeOf(obj);
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			Marshal.StructureToPtr(obj, intPtr, false);
			byte[] array = new byte[num];
			Marshal.Copy(intPtr, array, 0, num);
			Marshal.FreeHGlobal(intPtr);
			return array;
		}
		

		private static Bitmap ResizeImage(Bitmap bmp, int newW, int newH)
		{
			try
			{
				Bitmap bitmap = new Bitmap(newW, newH, PixelFormat.Format24bppRgb);
				Graphics graphics = Graphics.FromImage(bitmap);
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.DrawImage(bmp, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
				graphics.Dispose();
				return bitmap;
			}
			catch (Exception ex)
			{
				System.Console.WriteLine(ex.ToString());
				return null;
			}
		}

		private int generatePicDat(int width, int height, string fileName, byte[] dat)
		{
			ushort[] array = new ushort[width * height];
			if (fileName.Length > 0)
			{
				try
				{
					int num = 0;
					Bitmap bmp = new Bitmap(fileName);
					bmp = ResizeImage(bmp, width, height);
					if (bmp == null)
					{
						System.Console.WriteLine("Unable to scale file:" + fileName);
						return -1;
					}
					ushort num2;
					for (int i = 0; i < bmp.Height; i++)
					{
						for (int j = 0; j < bmp.Width; j++)
						{
							Color pixel = bmp.GetPixel(j, i);
							num2 = (ushort)((pixel.B >> 3) + (pixel.G >> 3 << 6) + (pixel.R >> 3 << 11));
							array[num++] = num2;
						}
					}
					int num4 = 0;
					int num5 = 0;
					int num6 = 0;
					num2 = array[0];
					for (int k = 1; k < num; k++)
					{
						if (array[k] != num2 || k - num5 >= 4095 || k >= num - 1)
						{
							num6 = k - num5;
							if (num6 > 1)
							{
								num2 = (ushort)(num2 | 0x20);
								array[num4++] = num2;
								array[num4++] = (ushort)((num6 - 1) | 0x3000);
								num5 = k;
								num2 = array[k];
							}
							else
							{
								num5 = k;
								array[num4++] = num2;
								num2 = array[k];
							}
						}
					}
					array[num4++] = num2;
					for (int l = 0; l < num4; l++)
					{
						dat[2 * l] = (byte)(array[l] >> 8);
						dat[2 * l + 1] = (byte)(array[l] & 0xFF);
					}
					return num4 * 2;
				}
				catch (Exception ex)
				{
					System.Console.WriteLine("Unable to open image file:" + fileName + ex.ToString());
					return -1;
				}
			}
			System.Console.WriteLine("Invalid file name:" + fileName);
			return -1;
		}

		public int GenerateLogo(string SourceFile)
		{
			int num = 320; //Photon LCD width
			int num2 = 240; //Photon LCD height
		
			byte[] array = new byte[num * num2 * 2];
			spi_file_head_t spi_file_head_t = default(spi_file_head_t);
			spi_file_head_t.magic_num = (SPI_FILE_MAGIC_BASE | 0xB);
			int num3 = generatePicDat(num, num2, SourceFile, array);
			if (num3 > 0)
			{
				spi_file_head_t.file_size = (uint)num3;
				string directoryName = AppDomain.CurrentDomain.BaseDirectory; //Path.GetDirectoryName(SourceFile);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(SourceFile);
				Path.GetExtension(SourceFile);
				string text = directoryName + "\\" + fileNameWithoutExtension + "_" + num + "x" + num2 + ".logo";
				StreamWriter streamWriter;
				try
				{
					streamWriter = new StreamWriter(text);
				}
				catch (Exception)
				{
					System.Console.WriteLine("Unable to generate file:" + text);
					return -1;
				}
				byte[] array2 = StructToBytes(spi_file_head_t);
				streamWriter.BaseStream.Write(array2, 0, array2.Length);
				streamWriter.BaseStream.Write(array, 0, num3);
				streamWriter.Close();
				return 0;
			}
			else
				return -1;
		}
}

class MainClass
{
    static int Main(string[] args)
    {
        // Test if input arguments were supplied:
        if (args.Length < 1)
        {
            System.Console.WriteLine("Please enter input file name.");
            System.Console.WriteLine("Usage: LOGOgenerator <input file>");
            return 1;
        }

        // Get the images.
        Encoder encoder = new Encoder();
        int result = encoder.GenerateLogo(args[0]);
        
        if (result < 0)
        {
            System.Console.WriteLine("Logo generation failed!");
            return 1;
        }
        else
        {
            System.Console.WriteLine("Logo file generated sucessfully");
            return 0;
        }
    }
}

