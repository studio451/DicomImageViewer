//Copyright © 2018 studio451.ru
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//E-mail: info@studio451.ru
//URL: https://studio451.ru/dicomimageviewer

using System;
using log4net;
using Avi;
using System.IO;
using System.Drawing;
using DicomImageViewer.Properties;


namespace DicomUtils
{
    class VideoExporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(VideoExporter));
        
        public void exportTo(DirectoryInfo folder)
        {
            string aviFilePath = Path.Combine(Settings.Default.ExportPath, folder.Name + ".avi");
            if (File.Exists(aviFilePath))
            {
                aviFilePath = renameToNonExistentFileName(aviFilePath);
            }
            doExporting(aviFilePath, folder);
        }

        private string renameToNonExistentFileName(string filePath)
        {
            int cnt = 0;
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string folderPath = Path.GetDirectoryName(filePath);
            string newFileName = fileName + cnt + extension;

            while (File.Exists(Path.Combine(folderPath, newFileName)))
            {
                cnt++;
                newFileName = fileName + cnt + extension;
            }
            return Path.Combine(folderPath, newFileName);
        }
              
        private void doExporting(string tmpFilePath, DirectoryInfo folder)
        {

            AviManager aviManager = new AviManager(tmpFilePath, false);          

            bool first = true;
            Bitmap bitmap = null;
            VideoStream aviStream = null;
            foreach (FileInfo file in folder.GetFiles("*.dcm"))
            {
                DicomElement currentDicomElement = new DicomElement(file.FullName);
                if (first)
                {
                    bitmap = currentDicomElement.Bitmap;
                    aviStream = aviManager.AddVideoStream(createCompressedOptions(), Settings.Default.Fps, bitmap);
                    first = false;
                }
                bitmap = currentDicomElement.Bitmap;
                aviStream.AddFrame(bitmap);

            }
            if(bitmap != null)
            bitmap.Dispose();
            aviManager.Close();
        }

        private Avi.Avi.AVICOMPRESSOPTIONS createCompressedOptions()
        {
            Avi.Avi.AVICOMPRESSOPTIONS opts = new Avi.Avi.AVICOMPRESSOPTIONS();
            opts.fccType = (UInt32)Avi.Avi.mmioStringToFOURCC("vids", 0);
            opts.fccHandler = (UInt32)Avi.Avi.mmioStringToFOURCC("CVID", 0);
            opts.dwKeyFrameEvery = 0;
            opts.dwQuality = 0;
            opts.dwFlags = Avi.Avi.AVICOMPRESSF_DATARATE;
            opts.dwBytesPerSecond = 0;
            opts.lpFormat = new IntPtr(0);
            opts.cbFormat = 0;
            opts.lpParms = new IntPtr(0);
            opts.cbParms = 0;
            opts.dwInterleaveEvery = 0;

            return opts;
        }
    }
}
