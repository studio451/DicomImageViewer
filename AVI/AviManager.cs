using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Avi
{
	public class AviManager
	{
		private int aviFile = 0;
		private ArrayList streams = new ArrayList();

		public AviManager(String fileName, bool open){
			Avi.AVIFileInit();
			int result;

			if(open){
				
				result = Avi.AVIFileOpen(
					ref aviFile, fileName,
					Avi.OF_READWRITE, 0);

			}else{
				
				result = Avi.AVIFileOpen(
					ref aviFile, fileName, 
					Avi.OF_WRITE | Avi.OF_CREATE, 0);
			
			}

			if(result != 0) {
				throw new Exception("Exception in AVIFileOpen: "+result.ToString());
			}
		}

        private AviManager(int aviFile) {
            this.aviFile = aviFile;
        }

		public VideoStream GetVideoStream(){
			IntPtr aviStream;

			int result = Avi.AVIFileGetStream(
				aviFile,
				out aviStream,
				Avi.streamtypeVIDEO, 0);
			
			if(result != 0){
				throw new Exception("Exception in AVIFileGetStream: "+result.ToString());
			}

			VideoStream stream = new VideoStream(aviFile, aviStream);
			streams.Add(stream);
			return stream;
		}

		public AudioStream GetWaveStream(){
			IntPtr aviStream;

			int result = Avi.AVIFileGetStream(
				aviFile,
				out aviStream,
				Avi.streamtypeAUDIO, 0);
			
			if(result != 0){
				throw new Exception("Exception in AVIFileGetStream: "+result.ToString());
			}

			AudioStream stream = new AudioStream(aviFile, aviStream);
			streams.Add(stream);
			return stream;
		}

		public VideoStream GetOpenStream(int index){
			return (VideoStream)streams[index];
		}

		public VideoStream AddVideoStream(bool isCompressed, double frameRate, int frameSize, int width, int height, PixelFormat format){
			VideoStream stream = new VideoStream(aviFile, isCompressed, frameRate, frameSize, width, height, format);
			streams.Add(stream);
			return stream;
		}

        public VideoStream AddVideoStream(Avi.AVICOMPRESSOPTIONS compressOptions, double frameRate, Bitmap firstFrame) {
            VideoStream stream = new VideoStream(aviFile, compressOptions, frameRate, firstFrame);
            streams.Add(stream);
            return stream;
        }

		public VideoStream AddVideoStream(bool isCompressed, double frameRate, Bitmap firstFrame){
			VideoStream stream = new VideoStream(aviFile, isCompressed, frameRate, firstFrame);
			streams.Add(stream);
			return stream;
		}

        public void AddAudioStream(String waveFileName, int startAtFrameIndex) {
            AviManager audioManager = new AviManager(waveFileName, true);
			AudioStream newStream = audioManager.GetWaveStream();
            AddAudioStream(newStream, startAtFrameIndex);
            audioManager.Close();
		}

        private IntPtr InsertSilence(int countSilentSamples, IntPtr waveData, int lengthWave, ref Avi.AVISTREAMINFO streamInfo) {

            int lengthSilence = countSilentSamples * streamInfo.dwSampleSize;
            byte[] silence = new byte[lengthSilence];


            int lengthNewStream = lengthSilence + lengthWave;
            IntPtr newWaveData = Marshal.AllocHGlobal(lengthNewStream);


            Marshal.Copy(silence, 0, newWaveData, lengthSilence);


            byte[] sound = new byte[lengthWave];
            Marshal.Copy(waveData, sound, 0, lengthWave);
            IntPtr startOfSound = new IntPtr(newWaveData.ToInt32() + lengthSilence);
            Marshal.Copy(sound, 0, startOfSound, lengthWave);

            Marshal.FreeHGlobal(newWaveData);

            streamInfo.dwLength = lengthNewStream;
            return newWaveData;
        }

        public void AddAudioStream(AudioStream newStream, int startAtFrameIndex) {
            Avi.AVISTREAMINFO streamInfo = new Avi.AVISTREAMINFO();
			Avi.PCMWAVEFORMAT streamFormat = new Avi.PCMWAVEFORMAT();
			int streamLength = 0;

			IntPtr rawData = newStream.GetStreamData(ref streamInfo, ref streamFormat, ref streamLength);
			IntPtr waveData = rawData;
			
			if (startAtFrameIndex > 0) {
                double framesPerSecond = GetVideoStream().FrameRate;
                double samplesPerSecond = newStream.CountSamplesPerSecond;
                double startAtSecond = startAtFrameIndex / framesPerSecond;
                int startAtSample = (int)(samplesPerSecond * startAtSecond);

                waveData = InsertSilence(startAtSample - 1, waveData, streamLength, ref streamInfo);
            }

            IntPtr aviStream;
			int result = Avi.AVIFileCreateStream(aviFile, out aviStream, ref streamInfo);
			if(result != 0){
				throw new Exception("Exception in AVIFileCreateStream: "+result.ToString());
			}

			result = Avi.AVIStreamSetFormat(aviStream, 0, ref streamFormat, Marshal.SizeOf(streamFormat));
			if(result != 0){
				throw new Exception("Exception in AVIStreamSetFormat: "+result.ToString());
			}
			
			result = Avi.AVIStreamWrite(aviStream, 0, streamLength, waveData, streamLength, Avi.AVIIF_KEYFRAME, 0, 0);
			if(result != 0){
				throw new Exception("Exception in AVIStreamWrite: "+result.ToString());
			}
			
			result = Avi.AVIStreamRelease(aviStream);
			if(result != 0){
				throw new Exception("Exception in AVIStreamRelease: "+result.ToString());
			}

			Marshal.FreeHGlobal(waveData);
		}

        public void AddAudioStream(IntPtr waveData, Avi.AVISTREAMINFO streamInfo, Avi.PCMWAVEFORMAT streamFormat, int streamLength) {
            IntPtr aviStream;
            int result = Avi.AVIFileCreateStream(aviFile, out aviStream, ref streamInfo);
            if (result != 0) {
                throw new Exception("Exception in AVIFileCreateStream: " + result.ToString());
            }

            result = Avi.AVIStreamSetFormat(aviStream, 0, ref streamFormat, Marshal.SizeOf(streamFormat));
            if (result != 0) {
                throw new Exception("Exception in AVIStreamSetFormat: " + result.ToString());
            }

            result = Avi.AVIStreamWrite(aviStream, 0, streamLength, waveData, streamLength, Avi.AVIIF_KEYFRAME, 0, 0);
            if (result != 0) {
                throw new Exception("Exception in AVIStreamWrite: " + result.ToString());
            }

            result = Avi.AVIStreamRelease(aviStream);
            if (result != 0) {
                throw new Exception("Exception in AVIStreamRelease: " + result.ToString());
            }
        }

        public AviManager CopyTo(String newFileName, float startAtSecond, float stopAtSecond) {
            AviManager newFile = new AviManager(newFileName, false);

            try {

                VideoStream videoStream = GetVideoStream();

                int startFrameIndex = (int)(videoStream.FrameRate * startAtSecond);
                int stopFrameIndex = (int)(videoStream.FrameRate * stopAtSecond);

                videoStream.GetFrameOpen();
                Bitmap bmp = videoStream.GetBitmap(startFrameIndex);
                VideoStream newStream = newFile.AddVideoStream(false, videoStream.FrameRate, bmp);
                for (int n = startFrameIndex + 1; n <= stopFrameIndex; n++) {
                    bmp = videoStream.GetBitmap(n);
                    newStream.AddFrame(bmp);
                }
                videoStream.GetFrameClose();


                AudioStream waveStream = GetWaveStream();

                Avi.AVISTREAMINFO streamInfo = new Avi.AVISTREAMINFO();
                Avi.PCMWAVEFORMAT streamFormat = new Avi.PCMWAVEFORMAT();
                int streamLength = 0;
                IntPtr ptrRawData = waveStream.GetStreamData(
                    ref streamInfo,
                    ref streamFormat,
                    ref streamLength);

				int startByteIndex = (int)( startAtSecond * (float)(waveStream.CountSamplesPerSecond * streamFormat.nChannels * waveStream.CountBitsPerSample) / 8);
				int stopByteIndex = (int)( stopAtSecond * (float)(waveStream.CountSamplesPerSecond * streamFormat.nChannels * waveStream.CountBitsPerSample) / 8);

                IntPtr ptrWavePart = new IntPtr(ptrRawData.ToInt32() + startByteIndex);

                byte[] rawData = new byte[stopByteIndex - startByteIndex];
				Marshal.Copy(ptrWavePart, rawData, 0, rawData.Length);
				Marshal.FreeHGlobal(ptrRawData);

                streamInfo.dwLength = rawData.Length;
                streamInfo.dwStart = 0;

                IntPtr unmanagedRawData = Marshal.AllocHGlobal(rawData.Length);
                Marshal.Copy(rawData, 0, unmanagedRawData, rawData.Length);
                newFile.AddAudioStream(unmanagedRawData, streamInfo, streamFormat, rawData.Length);
				Marshal.FreeHGlobal(unmanagedRawData);
            } catch (Exception ex) {
                newFile.Close();
                throw ex;
            }

            return newFile;
        }

		public void Close(){
			foreach(AviStream stream in streams){
				stream.Close();
			}
			
			Avi.AVIFileRelease(aviFile);
			Avi.AVIFileExit();
		}

        public static void MakeFileFromStream(String fileName, AviStream stream) {
            IntPtr newFile = IntPtr.Zero;
            IntPtr streamPointer = stream.StreamPointer;

            Avi.AVICOMPRESSOPTIONS_CLASS opts = new Avi.AVICOMPRESSOPTIONS_CLASS();
            opts.fccType = (uint)Avi.streamtypeVIDEO;
            opts.lpParms = IntPtr.Zero;
            opts.lpFormat = IntPtr.Zero;
            Avi.AVISaveOptions(IntPtr.Zero, Avi.ICMF_CHOOSE_KEYFRAME | Avi.ICMF_CHOOSE_DATARATE, 1, ref streamPointer, ref opts);
            Avi.AVISaveOptionsFree(1, ref opts);

            Avi.AVISaveV(fileName, 0, 0, 1, ref streamPointer, ref opts);
        }
    }
}
