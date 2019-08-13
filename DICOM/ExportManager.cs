//Copyright © 2018 studio451.ru
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//E-mail: info@studio451.ru
//URL: https://studio451.ru/dicomimageviewer

using System;
using DicomUtils;
using DicomImageViewer.Properties;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using log4net;

namespace DicomImageViewer.Dicom
{
    public class ExportManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ExportManager));
        private DirectoryInfo folders;

        public ExportManager(DirectoryInfo folders)
        {
            this.folders = folders;
        }

        public void doBitmapExporting(BackgroundWorker worker)
        {
            int count = 0;
            foreach (DirectoryInfo folder in folders.GetDirectories())
            {
                if (worker.CancellationPending)
                {
                    break;
                }
                foreach (FileInfo file in folder.GetFiles("*.dcm"))
                {
                    DicomElement currentDicomElement = new DicomElement(file.FullName);

                    Bitmap bmp = currentDicomElement.Bitmap;
                    string exportPath = Settings.Default.ExportPath;
                    string fileName = Path.GetFileNameWithoutExtension(currentDicomElement.FilePath);

                    if (Settings.Default.ExportToBmp)
                    {
                        string bmpExportPath = exportPath + currentDicomElement.GetSubFolderPath("bmp") + folder.Name + "\\";
                        Directory.CreateDirectory(bmpExportPath);
                        bmp.Save(bmpExportPath + fileName + ".bmp", ImageFormat.Bmp);
                    }

                    if (Settings.Default.ExportToJpg)
                    {
                        string jpgExportPath = exportPath + currentDicomElement.GetSubFolderPath("jpg") + folder.Name + "\\";
                        Directory.CreateDirectory(jpgExportPath);
                        bmp.Save(jpgExportPath + fileName + ".jpg", ImageFormat.Jpeg);
                    }

                    if (Settings.Default.ExportToPng)
                    {
                        string pngExportPath = exportPath + currentDicomElement.GetSubFolderPath("png") + folder.Name + "\\";
                        Directory.CreateDirectory(pngExportPath);
                        bmp.Save(pngExportPath + fileName + ".png", ImageFormat.Png);
                    }
                    count++;
                    worker.ReportProgress(count);
                }
            }
        }

        public void doVideoExporting(BackgroundWorker worker)
        {
            int count = 0;
            foreach (DirectoryInfo folder in folders.GetDirectories())
            {
                if (worker.CancellationPending)
                {
                    break;
                }
                try
                {
                    new VideoExporter().exportTo(folder);
                }
                catch (Exception e)
                {
                    log.Error("Error while video exporting from folder: " + folder.Name);
                    log.Error(e.StackTrace);
                }
                count++;
                worker.ReportProgress(count);
            }
        }
    }
}
