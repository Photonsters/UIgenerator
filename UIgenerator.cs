using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

public class Packer
{
		private const int MaxImages = 357;
		private static uint UI_BMP_MAGIC_BASE = 3793551360u; //0xE21D0000
		private static uint SPI_FILE_MAGIC_BASE = 318570496u; //0x12FD0000
		private byte[] fileDat = new byte[0];
		//Table of Photon image properties. Loaded from UItable.csv file (offset will be recalculated later).
		private ui_head_t[] header_array = new ui_head_t[MaxImages];

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct spi_file_head_t
		{
			public uint magic_num;
			public uint file_size;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct ui_head_t
		{
			public uint ui_magic;
			public int offset;
			public ushort x_pos;
			public ushort y_pos;
			public ushort x_width;
			public ushort y_height;
			public uint reserved;
			
			public ui_head_t(uint a, int b, ushort c, ushort d, ushort e, ushort f, uint g)
			{
				ui_magic = a;
				offset = b;
				x_pos = c;
				y_pos = d;
				x_width = e;
				y_height = f;
				reserved = g;
			}
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
		
		public object BytesToStruct(byte[] buf, int len, Type type)
		{
			IntPtr intPtr = Marshal.AllocHGlobal(len);
			Marshal.Copy(buf, 0, intPtr, len);
			object result = Marshal.PtrToStructure(intPtr, type);
			Marshal.FreeHGlobal(intPtr);
			return result;
		}

		public object BytesToStruct(byte[] buf, Type type)
		{
			return BytesToStruct(buf, buf.Length, type);
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
			return -1;
		}

		public int GenerateUI(string SourceFolder, string OutputFile)
		{
			int ImageNum = 0;
			spi_file_head_t spi_file_head_t = default(spi_file_head_t);
			ui_head_t ui_head_t = default(ui_head_t);

			//Initial offset is after headers (8+ 357*20)
			int ImageOffset = Marshal.SizeOf((object)spi_file_head_t) + MaxImages * Marshal.SizeOf((object)ui_head_t);

			string text = AppDomain.CurrentDomain.BaseDirectory + "\\" + OutputFile;
			StreamWriter streamWriter;
			try
			{
				streamWriter = new StreamWriter(text);
			}
			catch (Exception)
			{
				System.Console.WriteLine("Unable to generate output file:" + text);
				return -1;
			}
			string path = AppDomain.CurrentDomain.BaseDirectory + "\\" + SourceFolder;
			if(!Directory.Exists(path))
			{
				System.Console.WriteLine("Input folder doesn't exist:" + path);
				return -1;
			}
			//Read the UItable
			string text2 = path + "\\UItable.csv";
			StreamReader streamReader;
			try
			{
				streamReader = new StreamReader(text2);
			}
			catch (Exception)
			{
				System.Console.WriteLine("Unable to open UI table file:" + text2);
				return -1;
			}
			string line;
			int line_count = 1;
			Regex parts = new Regex(@"^(\d+)\,(\d+)\,(\d+)\,(\d+)\,(\d+)\,(\d+)\,(\d+)");
			line = streamReader.ReadLine(); //Discard first line
			for (int i = 0; i < MaxImages; i++)
			{
				line_count++;
				if ((line = streamReader.ReadLine()) != null)
				{
					Match match = parts.Match(line);
					if (match.Success)
					{
						ui_head_t.ui_magic = uint.Parse(match.Groups[1].Value);
						//This allows reading CSV files with image ID and files with image magic (previous version)
						if (ui_head_t.ui_magic != 0)
						{
							ui_head_t.ui_magic = (ui_head_t.ui_magic & 0xFFFF) | UI_BMP_MAGIC_BASE;
						}
						ui_head_t.offset = int.Parse(match.Groups[2].Value);
						ui_head_t.x_pos = ushort.Parse(match.Groups[3].Value);
						ui_head_t.y_pos = ushort.Parse(match.Groups[4].Value);
						ui_head_t.x_width = ushort.Parse(match.Groups[5].Value);
						ui_head_t.y_height = ushort.Parse(match.Groups[6].Value);
						ui_head_t.reserved = uint.Parse(match.Groups[7].Value);
						header_array[i] = ui_head_t;
					}
					else
					{
						System.Console.WriteLine("Error in UI table file:" + text2+ "at line {0}", line_count);
						return -1;
					}
				}
				else
				{
					System.Console.WriteLine("Error in UI table file:" + text2+ "at line {0}", line_count);
					return -1;
				}
			}

			//Init output file
			fileDat = new byte[ImageOffset];
			streamWriter.BaseStream.Write(fileDat, 0, ImageOffset); 
			//Export images
			for (int i = 0; i < MaxImages; i++)
			{
				ui_head_t ui_header = header_array[i];
				if (ui_header.ui_magic != 0)
				{
					string InputFile = AppDomain.CurrentDomain.BaseDirectory + "\\" + SourceFolder + "\\" + i.ToString() + ".bmp";
					if(!File.Exists(InputFile))
					{
						System.Console.WriteLine("Image {0} doesn't exist:", i.ToString());
						return -1;
					}
					else
					{
						byte[] array5 = new byte[ui_header.x_width * ui_header.y_height * 2];
						int num4 = generatePicDat(ui_header.x_width, ui_header.y_height, InputFile, array5);
						if (num4 > 0)
						{
							spi_file_head_t.magic_num = (uint)((int)UI_BMP_MAGIC_BASE | i);
							spi_file_head_t.file_size = (uint)num4;
							byte[] array6 = new byte[Marshal.SizeOf((object)spi_file_head_t)];
							array6 = StructToBytes(spi_file_head_t);
							streamWriter.BaseStream.Write(array6, 0, array6.Length);
							streamWriter.BaseStream.Write(array5, 0, num4);
							header_array[i].offset = ImageOffset; //Update image offset in table
							ImageOffset += array6.Length + num4;
							ImageNum++;
						}
					}
				}
			}
			//Update headers
			streamWriter.BaseStream.Position = 0L;
			spi_file_head_t.magic_num = (SPI_FILE_MAGIC_BASE | 0xC);
			spi_file_head_t.file_size = (uint)ImageOffset;
			byte[] array4 = new byte[Marshal.SizeOf((object)spi_file_head_t)];
			array4 = StructToBytes(spi_file_head_t);
			streamWriter.BaseStream.Write(array4, 0, array4.Length);
			byte[] array7 = new byte[Marshal.SizeOf((object)ui_head_t)];
			for (int l = 0; l < MaxImages; l++)
			{
				array7 = StructToBytes(header_array[l]);
				streamWriter.BaseStream.Write(array7, 0, array7.Length);
			}
			streamWriter.Close();
			return ImageNum;
		}
}

class MainClass
{
    static int Main(string[] args)
    {
        // Test if input arguments were supplied:
        if (args.Length < 2)
        {
            System.Console.WriteLine("CBD UI file generator v0.4");
            System.Console.WriteLine("Please enter input folder and binary UI package output.");
            System.Console.WriteLine("Usage: UIgenerator <input folder> <output UI binary> ");
            return 1;
        }

        // Get the images.
        Packer packer = new Packer();
        int result = packer.GenerateUI(args[0], args[1]);
        
        if (result < 0)
        {
            System.Console.WriteLine("UI generation failed!", result);
            return 0;
        }
        else
        {
            System.Console.WriteLine("UI file generated with {0} images", result);
            return 0;
        }
    }
}

