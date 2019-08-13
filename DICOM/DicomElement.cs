//Copyright © 2018 studio451.ru
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//E-mail: info@studio451.ru
//URL: https://studio451.ru/dicomimageviewer

using System;
using ClearCanvas.ImageViewer.StudyManagement;
using System.Drawing;
using ClearCanvas.ImageViewer;
using ClearCanvas.Dicom;

namespace DicomUtils
{
    public class DicomElement : IComparable
    {
        public Bitmap Bitmap;
        public string FilePath;
        public ImageSop ImageSop;
        public DicomFile DicomFile;
        public IPresentationImage PresentationImage;
        public int InstanceNumber = 0;

        public DicomElement(string filePath)
        {

            this.FilePath = filePath;

            DicomFile = new DicomFile(filePath);
            DicomFile.Load();

            ImageSop = new ClearCanvas.ImageViewer.StudyManagement.LocalImageSop(filePath);

            PresentationImage =
                 PresentationImageFactory.Create(ImageSop.Frames[1]);

            int width = ImageSop.Frames[1].Columns;
            int height = ImageSop.Frames[1].Rows;

            this.Bitmap = PresentationImage.DrawToBitmap(width, height);
            if (DicomFile.DataSet[DicomTags.InstanceNumber] == null)
            {
                throw new Exception("Tag 'Instance Number' not found!");
            }
            this.InstanceNumber = DicomFile.DataSet[DicomTags.InstanceNumber].GetInt32(0, 1);
        }

        public string GetSubFolderPath(string subfolder)
        {
            string patientsName = ImageSop.PatientsName.LastName + "_" + ImageSop.PatientsName.FirstName;
            if (patientsName != null && patientsName.Length != 0)
            {
                return "\\" + patientsName + "\\" + subfolder + "\\";
            }
            return "\\" + subfolder + "\\";
        }


        #region Члены IComparable

        public int CompareTo(object obj)
        {
            if (obj is DicomElement)
            {
                return this.InstanceNumber.CompareTo(((DicomElement)obj).InstanceNumber);
            }
            throw new Exception("Type error, type 'DicomElement' expected! ");
        }

        #endregion
    }
}
