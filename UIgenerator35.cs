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
			new ui_head_t(3793551361, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551362, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551363, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551364, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551365, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551366, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551367, 0, 0, 0, 480, 320, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551371, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551372, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551373, 0, 0, 0, 480, 320, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551378, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551379, 0, 309, 96, 126, 127, 0),
			new ui_head_t(3793551380, 0, 307, 94, 131, 133, 0),
			new ui_head_t(3793551381, 0, 173, 93, 131, 134, 0),
			new ui_head_t(3793551382, 0, 41, 93, 128, 134, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551384, 0, 77, 18, 140, 141, 0),
			new ui_head_t(3793551385, 0, 79, 163, 138, 137, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551389, 0, 261, 162, 139, 136, 0),
			new ui_head_t(3793551390, 0, 27, 24, 136, 133, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551393, 0, 312, 24, 139, 135, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551395, 0, 24, 164, 139, 135, 0),
			new ui_head_t(3793551396, 0, 261, 18, 139, 139, 0),
			new ui_head_t(3793551397, 0, 372, 30, 90, 90, 0),
			new ui_head_t(3793551398, 0, 372, 126, 92, 90, 0),
			new ui_head_t(3793551399, 0, 370, 115, 98, 90, 0),
			new ui_head_t(3793551400, 0, 370, 17, 100, 96, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551405, 0, 204, 126, 86, 71, 0),
			new ui_head_t(3793551406, 0, 342, 126, 82, 72, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551409, 0, 60, 126, 76, 75, 0),
			new ui_head_t(3793551410, 0, 342, 30, 84, 72, 0),
			new ui_head_t(3793551411, 0, 240, 30, 84, 72, 0),
			new ui_head_t(3793551412, 0, 138, 30, 90, 72, 0),
			new ui_head_t(3793551413, 0, 282, 219, 90, 75, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551421, 0, 48, 104, 85, 46, 0),
			new ui_head_t(3793551422, 0, 145, 104, 83, 46, 0),
			new ui_head_t(3793551423, 0, 241, 105, 84, 44, 0),
			new ui_head_t(3793551424, 0, 48, 152, 87, 46, 0),
			new ui_head_t(3793551425, 0, 144, 152, 83, 48, 0),
			new ui_head_t(3793551426, 0, 241, 152, 85, 45, 0),
			new ui_head_t(3793551427, 0, 48, 203, 87, 46, 0),
			new ui_head_t(3793551428, 0, 144, 203, 85, 44, 0),
			new ui_head_t(3793551429, 0, 242, 203, 84, 43, 0),
			new ui_head_t(3793551430, 0, 48, 252, 86, 42, 0),
			new ui_head_t(3793551431, 0, 144, 251, 84, 44, 0),
			new ui_head_t(3793551432, 0, 243, 250, 82, 45, 0),
			new ui_head_t(3793551433, 0, 338, 106, 97, 60, 0),
			new ui_head_t(3793551434, 0, 338, 170, 98, 60, 0),
			new ui_head_t(3793551435, 0, 336, 233, 98, 62, 0),
			new ui_head_t(3793551436, 0, 37, 25, 407, 66, 0),
			new ui_head_t(3793551437, 0, 22, 165, 164, 81, 0),
			new ui_head_t(3793551438, 0, 22, 32, 164, 88, 0),
			new ui_head_t(3793551439, 0, 198, 33, 162, 87, 0),
			new ui_head_t(3793551440, 0, 358, 118, 93, 92, 0),
			new ui_head_t(3793551441, 0, 358, 114, 96, 91, 0),
			new ui_head_t(3793551442, 0, 359, 18, 94, 92, 0),
			new ui_head_t(3793551443, 0, 358, 214, 96, 92, 0),
			new ui_head_t(3793551444, 0, 375, 222, 90, 78, 0),
			new ui_head_t(3793551445, 0, 371, 209, 96, 93, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551447, 0, 316, 167, 136, 135, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551449, 0, 380, 247, 94, 71, 0),
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
			new ui_head_t(3793551464, 0, 177, 108, 301, 42, 0),
			new ui_head_t(3793551465, 0, 177, 162, 301, 42, 0),
			new ui_head_t(3793551466, 0, 384, 237, 90, 79, 0),
			new ui_head_t(3793551467, 0, 192, 236, 80, 70, 0),
			new ui_head_t(3793551468, 0, 198, 233, 80, 75, 0),
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
			new ui_head_t(3793551480, 0, 186, 267, 61, 24, 0),
			new ui_head_t(3793551481, 0, 89, 267, 67, 25, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551483, 0, 312, 48, 156, 42, 0),
			new ui_head_t(3793551484, 0, 312, 108, 156, 36, 0),
			new ui_head_t(3793551485, 0, 312, 156, 156, 36, 0),
			new ui_head_t(3793551486, 0, 312, 204, 156, 36, 0),
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
			new ui_head_t(3793551499, 0, 25, 12, 333, 60, 0),
			new ui_head_t(3793551500, 0, 91, 214, 263, 52, 0),
			new ui_head_t(3793551501, 0, 31, 190, 323, 24, 0),
			new ui_head_t(3793551502, 0, 120, 222, 84, 72, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551511, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551512, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551513, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551514, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551515, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551516, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551517, 0, 0, 0, 480, 320, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551521, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551522, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551523, 0, 0, 0, 480, 320, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551528, 0, 0, 0, 480, 320, 0),
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
			new ui_head_t(3793551562, 0, 31, 22, 323, 162, 0),
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
			new ui_head_t(3793551574, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551575, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551576, 0, 197, 102, 276, 28, 0),
			new ui_head_t(3793551577, 0, 197, 140, 276, 34, 0),
			new ui_head_t(3793551578, 0, 197, 177, 276, 31, 0),
			new ui_head_t(3793551579, 0, 198, 213, 276, 33, 0),
			new ui_head_t(3793551580, 0, 366, 27, 76, 60, 0),
			new ui_head_t(3793551581, 0, 366, 24, 73, 67, 0),
			new ui_head_t(3793551582, 0, 375, 252, 102, 58, 0),
			new ui_head_t(3793551583, 0, 175, 96, 128, 128, 0),
			new ui_head_t(3793551584, 0, 43, 95, 126, 129, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551586, 0, 80, 23, 135, 134, 0),
			new ui_head_t(3793551587, 0, 80, 164, 134, 135, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551591, 0, 265, 164, 134, 138, 0),
			new ui_head_t(3793551592, 0, 28, 24, 135, 135, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551595, 0, 316, 24, 135, 133, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551597, 0, 29, 167, 134, 132, 0),
			new ui_head_t(3793551598, 0, 316, 165, 135, 135, 0),
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
			new ui_head_t(3793551610, 0, 265, 22, 134, 135, 0),
			new ui_head_t(3793551611, 0, 366, 114, 95, 93, 0),
			new ui_head_t(3793551612, 0, 366, 17, 98, 95, 0),
			new ui_head_t(3793551613, 0, 366, 209, 96, 94, 0),
			new ui_head_t(3793551614, 0, 372, 30, 90, 90, 0),
			new ui_head_t(3793551615, 0, 373, 125, 87, 90, 0),
			new ui_head_t(3793551616, 0, 372, 222, 90, 68, 0),
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
			new ui_head_t(3793551646, 0, 25, 78, 332, 219, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551649, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551650, 0, 0, 0, 480, 320, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551658, 0, 198, 164, 164, 82, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551661, 0, 26, 22, 116, 72, 0),
			new ui_head_t(3793551662, 0, 26, 22, 116, 72, 0),
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
			new ui_head_t(3793551700, 0, 284, 267, 70, 24, 0),
			new ui_head_t(3793551701, 0, 170, 24, 137, 133, 0),
			new ui_head_t(3793551702, 0, 171, 24, 136, 133, 0),
			new ui_head_t(3793551703, 0, 56, 130, 85, 67, 0),
			new ui_head_t(3793551704, 0, 215, 130, 99, 67, 0),
			new ui_head_t(3793551705, 0, 325, 130, 99, 67, 0),
			new ui_head_t(3793551706, 0, 56, 229, 128, 67, 0),
			new ui_head_t(3793551707, 0, 56, 229, 128, 67, 0),
			new ui_head_t(3793551708, 0, 297, 229, 128, 67, 0),
			new ui_head_t(3793551709, 0, 52, 22, 376, 80, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551711, 0, 59, 208, 227, 96, 0),
			new ui_head_t(3793551712, 0, 0, 0, 480, 320, 0),
			new ui_head_t(0, 0, 0, 0, 0, 0, 0),
			new ui_head_t(3793551714, 0, 293, 215, 131, 71, 0),
			new ui_head_t(3793551715, 0, 0, 0, 480, 320, 0),
			new ui_head_t(3793551716, 0, 59, 31, 365, 162, 0)
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

