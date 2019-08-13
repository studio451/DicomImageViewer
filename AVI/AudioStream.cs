using System;
using System.Runtime.InteropServices;

namespace Avi
{
	public class AudioStream : AviStream{
		
		public int CountBitsPerSample{
			get{ return waveFormat.wBitsPerSample; }
		}

		public int CountSamplesPerSecond{
			get{ return waveFormat.nSamplesPerSec; }
		}
		
		public int CountChannels{
			get{ return waveFormat.nChannels; }
		}

		private Avi.PCMWAVEFORMAT waveFormat = new Avi.PCMWAVEFORMAT();

        public AudioStream(int aviFile, IntPtr aviStream){
			this.aviFile = aviFile;
			this.aviStream = aviStream;
			
			int size = Marshal.SizeOf(waveFormat);
			Avi.AVIStreamReadFormat(aviStream, 0, ref waveFormat, ref size);
			Avi.AVISTREAMINFO streamInfo = GetStreamInfo(aviStream);
		}

		private Avi.AVISTREAMINFO GetStreamInfo(IntPtr aviStream){
			Avi.AVISTREAMINFO streamInfo = new Avi.AVISTREAMINFO();
			int result = Avi.AVIStreamInfo(aviStream, ref streamInfo, Marshal.SizeOf(streamInfo));
			if(result != 0) {
				throw new Exception("Exception in AVIStreamInfo: "+result.ToString());
			}
			return streamInfo;
		}

		public Avi.AVISTREAMINFO GetStreamInfo(){
			if(writeCompressed){
				return GetStreamInfo(compressedStream);
			}else{
				return GetStreamInfo(aviStream);
			}
		}
		
		public Avi.PCMWAVEFORMAT GetFormat(){
			Avi.PCMWAVEFORMAT format = new Avi.PCMWAVEFORMAT();
			int size = Marshal.SizeOf(format);
			int result = Avi.AVIStreamReadFormat(aviStream, 0, ref format, ref size);
			return format;
		}

		public IntPtr GetStreamData(ref Avi.AVISTREAMINFO streamInfo, ref Avi.PCMWAVEFORMAT format, ref int streamLength){
			streamInfo = GetStreamInfo();
			
			format = GetFormat();
			streamLength = Avi.AVIStreamLength(aviStream.ToInt32()) * streamInfo.dwSampleSize;
			IntPtr waveData = Marshal.AllocHGlobal(streamLength);
			
			int result = Avi.AVIStreamRead(aviStream, 0, streamLength, waveData, streamLength, 0, 0);
			if(result != 0){
				throw new Exception("Exception in AVIStreamRead: "+result.ToString());
			}
			
			return waveData;
		}

		public override void ExportStream(String fileName){
			Avi.AVICOMPRESSOPTIONS_CLASS opts = new Avi.AVICOMPRESSOPTIONS_CLASS();
			opts.fccType         = (UInt32)Avi.mmioStringToFOURCC("auds", 0);
			opts.fccHandler      = (UInt32)Avi.mmioStringToFOURCC("CAUD", 0);
			opts.dwKeyFrameEvery = 0;
			opts.dwQuality       = 0;
			opts.dwFlags         = 0;
			opts.dwBytesPerSecond= 0;
			opts.lpFormat        = new IntPtr(0);
			opts.cbFormat        = 0;
			opts.lpParms         = new IntPtr(0);
			opts.cbParms         = 0;
			opts.dwInterleaveEvery = 0;
			
			Avi.AVISaveV(fileName, 0, 0, 1, ref aviStream, ref opts);
		}
	}
}
