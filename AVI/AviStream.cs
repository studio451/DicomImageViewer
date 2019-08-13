using System;

namespace Avi
{
	public abstract class AviStream
	{
		protected int aviFile;
		protected IntPtr aviStream;
		protected IntPtr compressedStream;
		protected bool writeCompressed;

        internal int FilePointer {
            get { return aviFile; }
        }

        internal virtual IntPtr StreamPointer {
            get { return aviStream; }
        }

        internal bool WriteCompressed {
            get { return writeCompressed; }
        }

        public virtual void Close(){
			if(writeCompressed){
				Avi.AVIStreamRelease(compressedStream);
			}
			Avi.AVIStreamRelease(StreamPointer);
		}

		public abstract void ExportStream(String fileName);
		
	}
}
