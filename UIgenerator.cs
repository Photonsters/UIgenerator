using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

public class Packer
{
		private const int MaxImages = 357;
		private static uint UI_BMP_MAGIC_BASE = 3793551360u; //0xE21D0000
		private static uint SPI_FILE_MAGIC_BASE = 318570496u; //0x12FD0000
		private byte[] fileDat = new byte[0];

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
		
		//Table of Photon image properties. Offset will be set later.
		//Not too elegant to use non-static array, but need to update the offset while computing images
		private ui_head_t[] header_array = new ui_head_t[MaxImages]
		{
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551361, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551362, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551363, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551364, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551365, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551366, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551367, 0, 0, 0, 320, 240, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551371, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551372, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551373, 0, 0, 0, 320, 240, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551378, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551379, 0, 10, 66, 94, 106, 0),
			new ui_head_t(3793551380, 0, 10, 66, 93, 105, 0),
			new ui_head_t(3793551381, 0, 113, 66, 94, 105, 0),
			new ui_head_t(3793551382, 0, 217, 66, 93, 105, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551384, 0, 216, 7, 93, 106, 0),
			new ui_head_t(3793551385, 0, 10, 9, 93, 105, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551389, 0, 217, 124, 93, 105, 0),
			new ui_head_t(3793551390, 0, 9, 9, 93, 105, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551393, 0, 216, 9, 94, 105, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551395, 0, 9, 124, 93, 104, 0),
			new ui_head_t(3793551396, 0, 112, 9, 94, 105, 0),
			new ui_head_t(3793551397, 0, 250, 23, 60, 62, 0),
			new ui_head_t(3793551398, 0, 250, 93, 60, 60, 0),
			new ui_head_t(3793551399, 0, 250, 85, 60, 69, 0),
			new ui_head_t(3793551400, 0, 250, 9, 60, 67, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551405, 0, 112, 85, 94, 66, 0),
			new ui_head_t(3793551406, 0, 216, 85, 93, 66, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551409, 0, 9, 85, 94, 66, 0),
			new ui_head_t(3793551410, 0, 216, 16, 86, 54, 0),
			new ui_head_t(3793551411, 0, 113, 16, 92, 54, 0),
			new ui_head_t(3793551412, 0, 17, 16, 86, 54, 0),
			new ui_head_t(3793551413, 0, 216, 162, 94, 66, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551421, 0, 9, 73, 72, 36, 0),
			new ui_head_t(3793551422, 0, 84, 73, 72, 36, 0),
			new ui_head_t(3793551423, 0, 160, 73, 72, 36, 0),
			new ui_head_t(3793551424, 0, 9, 112, 73, 36, 0),
			new ui_head_t(3793551425, 0, 84, 113, 72, 36, 0),
			new ui_head_t(3793551426, 0, 160, 113, 72, 36, 0),
			new ui_head_t(3793551427, 0, 9, 153, 72, 36, 0),
			new ui_head_t(3793551428, 0, 84, 153, 72, 36, 0),
			new ui_head_t(3793551429, 0, 159, 153, 73, 36, 0),
			new ui_head_t(3793551430, 0, 9, 192, 72, 36, 0),
			new ui_head_t(3793551431, 0, 84, 192, 72, 36, 0),
			new ui_head_t(3793551432, 0, 160, 193, 72, 36, 0),
			new ui_head_t(3793551433, 0, 241, 73, 69, 49, 0),
			new ui_head_t(3793551434, 0, 241, 126, 69, 49, 0),
			new ui_head_t(3793551435, 0, 241, 179, 69, 49, 0),
			new ui_head_t(3793551436, 0, 9, 9, 301, 54, 0),
			new ui_head_t(3793551437, 0, 10, 126, 113, 63, 0),
			new ui_head_t(3793551438, 0, 10, 24, 113, 63, 0),
			new ui_head_t(3793551439, 0, 127, 24, 115, 63, 0),
			new ui_head_t(3793551440, 0, 250, 85, 61, 68, 0),
			new ui_head_t(3793551441, 0, 249, 87, 61, 68, 0),
			new ui_head_t(3793551442, 0, 250, 9, 60, 67, 0),
			new ui_head_t(3793551443, 0, 249, 163, 61, 69, 0),
			new ui_head_t(3793551444, 0, 250, 161, 60, 62, 0),
			new ui_head_t(3793551445, 0, 250, 162, 60, 67, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551447, 0, 216, 124, 94, 105, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551449, 0, 241, 186, 69, 43, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551464, 0, 65, 127, 231, 31, 0),
			new ui_head_t(3793551465, 0, 79, 78, 218, 28, 0),
			new ui_head_t(3793551466, 0, 241, 186, 69, 42, 0),
			new ui_head_t(3793551467, 0, 9, 186, 69, 42, 0),
			new ui_head_t(3793551468, 0, 9, 186, 69, 42, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551480, 0, 105, 210, 59, 12, 0),
			new ui_head_t(3793551481, 0, 29, 210, 61, 12, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551483, 0, 171, 24, 120, 22, 0),
			new ui_head_t(3793551484, 0, 171, 99, 120, 24, 0),
			new ui_head_t(3793551485, 0, 171, 135, 120, 24, 0),
			new ui_head_t(3793551486, 0, 171, 61, 120, 24, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551499, 0, 8, 7, 232, 66, 0),
			new ui_head_t(3793551500, 0, 30, 186, 208, 15, 0),
			new ui_head_t(3793551501, 0, 12, 164, 226, 20, 0),
			new ui_head_t(3793551502, 0, 9, 162, 95, 66, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551511, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551512, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551513, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551514, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551515, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551516, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551517, 0, 0, 0, 320, 240, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551521, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551522, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551523, 0, 0, 0, 320, 240, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551528, 0, 0, 0, 320, 240, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551562, 0, 10, 10, 230, 142, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551574, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551575, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551576, 0, 53, 27, 162, 28, 0),
			new ui_head_t(3793551577, 0, 130, 78, 86, 29, 0),
			new ui_head_t(3793551578, 0, 108, 129, 108, 30, 0),
			new ui_head_t(3793551579, 0, 134, 183, 82, 29, 0),
			new ui_head_t(3793551580, 0, 240, 9, 69, 43, 0),
			new ui_head_t(3793551581, 0, 240, 9, 70, 43, 0),
			new ui_head_t(3793551582, 0, 240, 186, 70, 43, 0),
			new ui_head_t(3793551583, 0, 112, 66, 94, 105, 0),
			new ui_head_t(3793551584, 0, 216, 66, 93, 105, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551586, 0, 216, 8, 93, 106, 0),
			new ui_head_t(3793551587, 0, 9, 8, 94, 106, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551591, 0, 216, 124, 94, 105, 0),
			new ui_head_t(3793551592, 0, 10, 9, 93, 105, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551595, 0, 216, 9, 93, 105, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551597, 0, 9, 124, 93, 105, 0),
			new ui_head_t(3793551598, 0, 216, 124, 93, 105, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551610, 0, 112, 8, 93, 106, 0),
			new ui_head_t(3793551611, 0, 250, 85, 60, 67, 0),
			new ui_head_t(3793551612, 0, 250, 9, 60, 67, 0),
			new ui_head_t(3793551613, 0, 250, 162, 60, 67, 0),
			new ui_head_t(3793551614, 0, 250, 23, 59, 62, 0),
			new ui_head_t(3793551615, 0, 250, 93, 60, 60, 0),
			new ui_head_t(3793551616, 0, 250, 160, 60, 63, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551646, 0, 8, 74, 232, 156, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551649, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551650, 0, 0, 0, 320, 240, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551658, 0, 127, 124, 115, 64, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551661, 0, 240, 97, 70, 43, 0),
			new ui_head_t(3793551662, 0, 240, 97, 70, 43, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551700, 0, 181, 210, 58, 12, 0),
			new ui_head_t(3793551701, 0, 113, 9, 93, 105, 0),
			new ui_head_t(3793551702, 0, 113, 9, 93, 105, 0),
			new ui_head_t(3793551703, 0, 12, 124, 96, 43, 0),
			new ui_head_t(3793551704, 0, 162, 122, 69, 49, 0),
			new ui_head_t(3793551705, 0, 240, 122, 70, 49, 0),
			new ui_head_t(3793551706, 0, 9, 179, 79, 50, 0),
			new ui_head_t(3793551707, 0, 9, 178, 80, 51, 0),
			new ui_head_t(3793551708, 0, 230, 178, 80, 51, 0),
			new ui_head_t(3793551709, 0, 32, 26, 264, 70, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551711, 0, 12, 161, 175, 63, 0),
			new ui_head_t(3793551712, 0, 0, 0, 320, 240, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551714, 0, 216, 157, 94, 70, 0),
			new ui_head_t(3793551715, 0, 0, 0, 320, 240, 0),
			new ui_head_t(3793551716, 0, 10, 10, 300, 141, 0)
		};

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

