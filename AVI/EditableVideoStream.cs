using System;
using System.Runtime.InteropServices;

namespace Avi {
    public class EditableVideoStream : VideoStream {
        private IntPtr editableStream = IntPtr.Zero;

        internal override IntPtr StreamPointer {
            get { return editableStream; }
        }

        public EditableVideoStream(VideoStream stream) : base(stream.FrameSize, stream.FrameRate, stream.Width, stream.Height, stream.CountBitsPerPixel, stream.CountFrames, stream.CompressOptions, stream.WriteCompressed) {
            Avi.AVIFileInit();
            int result = Avi.CreateEditableStream(ref editableStream, stream.StreamPointer);

            if (result != 0) {
                throw new Exception("Exception in CreateEditableStream: " + result.ToString());
            } 
            
            SetInfo(stream.StreamInfo);
        }

        public override void Close() {
            base.Close();
            Avi.AVIFileExit();
        }

        public IntPtr Copy(int start, int length) {
            IntPtr copyPointer = IntPtr.Zero;
            int result = Avi.EditStreamCopy(editableStream, ref start, ref length, ref copyPointer);

            if (result != 0) {
                throw new Exception("Exception in Copy: " + result.ToString());
            }

            return copyPointer;
        }

        public IntPtr Cut(int start, int length) {
            IntPtr copyPointer = IntPtr.Zero;
            int result = Avi.EditStreamCut(editableStream, ref start, ref length, ref copyPointer);

            if (result != 0) {
                throw new Exception("Exception in Cut: " + result.ToString());
            } 
            
            countFrames -= length;
            return copyPointer;
        }

        public void Paste(VideoStream sourceStream, int copyPosition, int pastePosition, int length) {
            Paste(sourceStream.StreamPointer, copyPosition, pastePosition, length);
        }

        public void Paste(IntPtr sourceStream, int copyPosition, int pastePosition, int length) {
            int pastedLength = 0;
            int result = Avi.EditStreamPaste(editableStream, ref pastePosition, ref pastedLength, sourceStream, copyPosition, length);
            
            if (result != 0) {
                throw new Exception("Exception in Paste: " + result.ToString());
            }

            countFrames += pastedLength;
        }

        public void SetInfo(Avi.AVISTREAMINFO info) {
            int result = Avi.EditStreamSetInfo(editableStream, ref info, Marshal.SizeOf(info));
            if (result != 0) {
                throw new Exception("Exception in SetInfo: " + result.ToString());
            }
        
            frameRate = info.dwRate / info.dwScale;
        }
    }
}
