using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Avi
{
	public class VideoStream : AviStream
	{
		private int getFrameObject;

		private int frameSize;
        public int FrameSize {
            get { return frameSize; }
        }

        protected double frameRate;
        public double FrameRate {
            get{ return frameRate; }
		}

		private int width;
		public int Width{
			get{ return width; }
		}

		private int height;
		public int Height{
			get{ return height; }
		}

		private Int16 countBitsPerPixel;
        public Int16 CountBitsPerPixel {
            get{ return countBitsPerPixel; }
		}

		protected int countFrames = 0;
		public int CountFrames{
			get{ return countFrames; }
		}

		protected int firstFrame = 0;
		public int FirstFrame
		{
			get { return firstFrame; }
		}

        private Avi.AVICOMPRESSOPTIONS compressOptions;
        public Avi.AVICOMPRESSOPTIONS CompressOptions {
            get { return compressOptions; }
        }

        public Avi.AVISTREAMINFO StreamInfo {
            get { return GetStreamInfo(aviStream); }
        }

        public VideoStream(int aviFile, bool writeCompressed, double frameRate, int frameSize, int width, int height, PixelFormat format) {
			this.aviFile = aviFile;
			this.writeCompressed = writeCompressed;
			this.frameRate = frameRate;
			this.frameSize = frameSize;
			this.width = width;
			this.height = height;
			this.countBitsPerPixel = ConvertPixelFormatToBitCount(format);
			this.firstFrame = 0;

			CreateStream();
		}

        public VideoStream(int aviFile, bool writeCompressed, double frameRate, Bitmap firstFrame) {
            Initialize(aviFile, writeCompressed, frameRate, firstFrame);
            CreateStream();
			AddFrame(firstFrame);
		}

        public VideoStream(int aviFile, Avi.AVICOMPRESSOPTIONS compressOptions, double frameRate, Bitmap firstFrame) {
            Initialize(aviFile, true, frameRate, firstFrame);
            CreateStream(compressOptions);
            AddFrame(firstFrame);
        }

		public VideoStream(int aviFile, IntPtr aviStream){
			this.aviFile = aviFile;
			this.aviStream = aviStream;
			
			Avi.BITMAPINFOHEADER bih = new Avi.BITMAPINFOHEADER();
			int size = Marshal.SizeOf(bih);
			Avi.AVIStreamReadFormat(aviStream, 0, ref bih, ref size);
			Avi.AVISTREAMINFO streamInfo = GetStreamInfo(aviStream);
			
			this.frameRate = (float)streamInfo.dwRate / (float)streamInfo.dwScale;
			this.width = (int)streamInfo.rcFrame.right;
			this.height = (int)streamInfo.rcFrame.bottom;
			this.frameSize = bih.biSizeImage;
			this.countBitsPerPixel = bih.biBitCount;
			this.firstFrame = Avi.AVIStreamStart(aviStream.ToInt32());
			this.countFrames = Avi.AVIStreamLength(aviStream.ToInt32());
		}

        internal VideoStream(int frameSize, double frameRate, int width, int height, Int16 countBitsPerPixel, int countFrames, Avi.AVICOMPRESSOPTIONS compressOptions, bool writeCompressed) {
            this.frameSize = frameSize;
            this.frameRate = frameRate;
            this.width = width;
            this.height = height;
            this.countBitsPerPixel = countBitsPerPixel;
            this.countFrames = countFrames;
            this.compressOptions = compressOptions;
            this.writeCompressed = writeCompressed;
			this.firstFrame = 0;
        }

        private void Initialize(int aviFile, bool writeCompressed, double frameRate, Bitmap firstFrameBitmap) {
            this.aviFile = aviFile;
            this.writeCompressed = writeCompressed;
            this.frameRate = frameRate;
			this.firstFrame = 0;

			BitmapData bmpData = firstFrameBitmap.LockBits(new Rectangle(
				0, 0, firstFrameBitmap.Width, firstFrameBitmap.Height),
				ImageLockMode.ReadOnly, firstFrameBitmap.PixelFormat);

            this.frameSize = bmpData.Stride * bmpData.Height;
			this.width = firstFrameBitmap.Width;
			this.height = firstFrameBitmap.Height;
			this.countBitsPerPixel = ConvertPixelFormatToBitCount(firstFrameBitmap.PixelFormat);

			firstFrameBitmap.UnlockBits(bmpData);
        }

		private Int16 ConvertPixelFormatToBitCount(PixelFormat format){
			String formatName = format.ToString();
			if(formatName.Substring(0, 6) != "Format"){
				throw new Exception("Unknown pixel format: "+formatName);
			}

			formatName = formatName.Substring(6, 2);
			Int16 bitCount = 0;
			if( Char.IsNumber(formatName[1]) ){
				bitCount = Int16.Parse(formatName);
			}else{
				bitCount = Int16.Parse(formatName[0].ToString());
			}

			return bitCount;
		}

		private PixelFormat ConvertBitCountToPixelFormat(int bitCount){
			String formatName;
			if(bitCount > 16){
				formatName = String.Format("Format{0}bppRgb", bitCount);
			}else if(bitCount == 16){
				formatName = "Format16bppRgb555";
			}else{
				formatName = String.Format("Format{0}bppIndexed", bitCount);
			}
			
			return (PixelFormat)Enum.Parse(typeof(PixelFormat), formatName);
		}

		private Avi.AVISTREAMINFO GetStreamInfo(IntPtr aviStream){
			Avi.AVISTREAMINFO streamInfo = new Avi.AVISTREAMINFO();
			int result = Avi.AVIStreamInfo(StreamPointer, ref streamInfo, Marshal.SizeOf(streamInfo));
			if(result != 0) {
				throw new Exception("Exception in VideoStreamInfo: "+result.ToString());
			}
			return streamInfo;
		}

        private void GetRateAndScale(ref double frameRate, ref int scale) {
            scale = 1;
            while (frameRate != (long)frameRate) {
                frameRate = frameRate * 10;
                scale *= 10;
            }
        }

        private void CreateStreamWithoutFormat() {
            int scale = 1;
            double rate = frameRate;
            GetRateAndScale(ref rate, ref scale);

            Avi.AVISTREAMINFO strhdr = new Avi.AVISTREAMINFO();
            strhdr.fccType = Avi.mmioStringToFOURCC("vids", 0);
            strhdr.fccHandler = Avi.mmioStringToFOURCC("CVID", 0);
            strhdr.dwFlags = 0;
            strhdr.dwCaps = 0;
            strhdr.wPriority = 0;
            strhdr.wLanguage = 0;
            strhdr.dwScale = (int)scale;
            strhdr.dwRate = (int)rate;
            strhdr.dwStart = 0;
            strhdr.dwLength = 0;
            strhdr.dwInitialFrames = 0;
            strhdr.dwSuggestedBufferSize = frameSize;
            strhdr.dwQuality = -1;
            strhdr.dwSampleSize = 0;
            strhdr.rcFrame.top = 0;
            strhdr.rcFrame.left = 0;
            strhdr.rcFrame.bottom = (uint)height;
            strhdr.rcFrame.right = (uint)width;
            strhdr.dwEditCount = 0;
            strhdr.dwFormatChangeCount = 0;
            strhdr.szName = new UInt16[64];

            int result = Avi.AVIFileCreateStream(aviFile, out aviStream, ref strhdr);

            if (result != 0) {
                throw new Exception("Exception in AVIFileCreateStream: " + result.ToString());
            }
        }

        private void CreateStream() {
            CreateStreamWithoutFormat();

            if (writeCompressed) {
                CreateCompressedStream();
            } else {
                SetFormat(aviStream);
            }
        }

		private void CreateStream(Avi.AVICOMPRESSOPTIONS options){
            CreateStreamWithoutFormat();
            CreateCompressedStream(options);
		}

		private void CreateCompressedStream(){
			
			Avi.AVICOMPRESSOPTIONS_CLASS options = new Avi.AVICOMPRESSOPTIONS_CLASS();
			options.fccType = (uint)Avi.streamtypeVIDEO;

            options.lpParms = IntPtr.Zero;
			options.lpFormat = IntPtr.Zero;
			Avi.AVISaveOptions(IntPtr.Zero, Avi.ICMF_CHOOSE_KEYFRAME | Avi.ICMF_CHOOSE_DATARATE, 1, ref aviStream, ref options);
			Avi.AVISaveOptionsFree(1, ref options);
                        
            this.compressOptions = options.ToStruct();
            int result = Avi.AVIMakeCompressedStream(out compressedStream, aviStream, ref compressOptions, 0);
            if(result != 0) {
				throw new Exception("Exception in AVIMakeCompressedStream: "+result.ToString());
			}

			SetFormat(compressedStream);
		}

        private void CreateCompressedStream(Avi.AVICOMPRESSOPTIONS options) {
            int result = Avi.AVIMakeCompressedStream(out compressedStream, aviStream, ref options, 0);
            if (result != 0) {
                throw new Exception("Exception in AVIMakeCompressedStream: " + result.ToString());
            }

            this.compressOptions = options;

            SetFormat(compressedStream);
        }

		public void AddFrame(Bitmap bmp){
			bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

			BitmapData bmpDat = bmp.LockBits(
				new Rectangle(
				0,0, bmp.Width, bmp.Height),
				ImageLockMode.ReadOnly, bmp.PixelFormat);

            int result = Avi.AVIStreamWrite(writeCompressed ? compressedStream : StreamPointer,
                countFrames, 1, 
				bmpDat.Scan0, 
				(Int32)(bmpDat.Stride * bmpDat.Height), 
				0, 0, 0); 

			if (result!= 0) {
				throw new Exception("Exception in VideoStreamWrite: "+result.ToString());
			}

			bmp.UnlockBits(bmpDat);

			countFrames++;
		}

		private void SetFormat(IntPtr aviStream){
			Avi.BITMAPINFOHEADER bi = new Avi.BITMAPINFOHEADER();
			bi.biSize      = Marshal.SizeOf(bi);
			bi.biWidth     = width;
			bi.biHeight    = height;
			bi.biPlanes    = 1;
			bi.biBitCount  = countBitsPerPixel;
			bi.biSizeImage = frameSize;

			int result = Avi.AVIStreamSetFormat(aviStream, 0, ref bi, bi.biSize);
			if(result != 0){ throw new Exception("Error in VideoStreamSetFormat: "+result.ToString()); }
		}

		public void GetFrameOpen(){
            Avi.AVISTREAMINFO streamInfo = GetStreamInfo(StreamPointer);

			Avi.BITMAPINFOHEADER bih = new Avi.BITMAPINFOHEADER();
			bih.biBitCount = countBitsPerPixel;
			bih.biClrImportant = 0;
			bih.biClrUsed = 0;
			bih.biCompression = 0;
			bih.biPlanes = 1;
			bih.biSize = Marshal.SizeOf(bih);
			bih.biXPelsPerMeter = 0;
			bih.biYPelsPerMeter = 0;

			bih.biHeight = 0;
			bih.biWidth = 0;
            			
			if (bih.biBitCount > 24)
			{
				bih.biBitCount = 32;
			}
			else if (bih.biBitCount > 16)
			{
				bih.biBitCount = 24;
			}
			else if (bih.biBitCount > 8)
			{
				bih.biBitCount = 16;
			}
			else if (bih.biBitCount > 4)
			{
				bih.biBitCount = 8;
			}
			else if (bih.biBitCount > 0)
			{
				bih.biBitCount = 4;
			}
        
            getFrameObject = Avi.AVIStreamGetFrameOpen(StreamPointer, ref bih);

            if(getFrameObject == 0){ throw new Exception("Exception in VideoStreamGetFrameOpen!"); }
		}

		public void ExportBitmap(int position, String dstFileName){
			Bitmap bmp = GetBitmap(position);
			bmp.Save(dstFileName, ImageFormat.Bmp);
			bmp.Dispose();
		}

		public Bitmap GetBitmap(int position){
			if(position > countFrames){
				throw new Exception("Invalid frame position: "+position);
			}

            Avi.AVISTREAMINFO streamInfo = GetStreamInfo(StreamPointer);

            int dib = Avi.AVIStreamGetFrame(getFrameObject, firstFrame + position);
			Avi.BITMAPINFOHEADER bih = new Avi.BITMAPINFOHEADER();
			bih = (Avi.BITMAPINFOHEADER)Marshal.PtrToStructure(new IntPtr(dib), bih.GetType());

			if(bih.biSizeImage < 1){
				throw new Exception("Exception in VideoStreamGetFrame");
			}
		
			byte[] bitmapData;
			int address = dib + Marshal.SizeOf(bih);
			if(bih.biBitCount < 16){
				bitmapData = new byte[bih.biSizeImage + Avi.PALETTE_SIZE];
			}else{
				bitmapData = new byte[bih.biSizeImage];
			}

            Marshal.Copy(new IntPtr(address), bitmapData, 0, bitmapData.Length);

			byte[] bitmapInfo = new byte[Marshal.SizeOf(bih)];
			IntPtr ptr;
			ptr = Marshal.AllocHGlobal(bitmapInfo.Length);
			Marshal.StructureToPtr(bih, ptr, false);
			address = ptr.ToInt32();
            Marshal.Copy(new IntPtr(address), bitmapInfo, 0, bitmapInfo.Length);

            Marshal.FreeHGlobal(ptr);

			Avi.BITMAPFILEHEADER bfh = new Avi.BITMAPFILEHEADER();
			bfh.bfType = Avi.BMP_MAGIC_COOKIE;
			bfh.bfSize = (Int32)(55 + bih.biSizeImage);
			bfh.bfReserved1 = 0;
			bfh.bfReserved2 = 0;
			bfh.bfOffBits = Marshal.SizeOf(bih) + Marshal.SizeOf(bfh);
			if(bih.biBitCount < 16){				
				bfh.bfOffBits += Avi.PALETTE_SIZE;
			}
		
			BinaryWriter bw = new BinaryWriter( new MemoryStream() );

			bw.Write(bfh.bfType);
			bw.Write(bfh.bfSize);
			bw.Write(bfh.bfReserved1);
			bw.Write(bfh.bfReserved2);
			bw.Write(bfh.bfOffBits);
			bw.Write(bitmapInfo);
			bw.Write(bitmapData);
			
			Bitmap bmp = (Bitmap)Image.FromStream(bw.BaseStream);
			Bitmap saveableBitmap = new Bitmap(bmp.Width, bmp.Height);
			Graphics g = Graphics.FromImage(saveableBitmap);
			g.DrawImage(bmp, 0,0);
			g.Dispose();
			bmp.Dispose();

			bw.Close();
			return saveableBitmap;
		}

		public void GetFrameClose(){
			if(getFrameObject != 0){
				Avi.AVIStreamGetFrameClose(getFrameObject);
				getFrameObject = 0;
			}
		}

		public AviManager DecompressToNewFile(String fileName, bool recompress, out VideoStream newStream2){
			AviManager newFile = new AviManager(fileName, false);
			
			this.GetFrameOpen();
			
			Bitmap frame = GetBitmap(0);
			VideoStream newStream = newFile.AddVideoStream(recompress, frameRate, frame);
			frame.Dispose();

			for(int n=1; n<countFrames; n++){
				frame = GetBitmap(n);
				newStream.AddFrame(frame);
				frame.Dispose();
			}

			this.GetFrameClose();
			
			newStream2 = newStream;
			return newFile;
		}

		public override void ExportStream(String fileName){
			Avi.AVICOMPRESSOPTIONS_CLASS opts = new Avi.AVICOMPRESSOPTIONS_CLASS();
			opts.fccType = (uint)Avi.streamtypeVIDEO;
			opts.lpParms = IntPtr.Zero;
			opts.lpFormat = IntPtr.Zero;
            IntPtr streamPointer = StreamPointer;
            Avi.AVISaveOptions(IntPtr.Zero, Avi.ICMF_CHOOSE_KEYFRAME | Avi.ICMF_CHOOSE_DATARATE, 1, ref streamPointer, ref opts);
            Avi.AVISaveOptionsFree(1, ref opts);

			Avi.AVISaveV(fileName, 0, 0, 1, ref aviStream, ref opts);
		}
    }
}
