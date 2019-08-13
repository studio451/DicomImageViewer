//Copyright © 2018 studio451.ru
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//E-mail: info@studio451.ru
//URL: https://studio451.ru/dicomimageviewer

using System;
using System.Collections.Generic;
using ClearCanvas.Dicom;

namespace DicomImageViewer.DicomTypes
{
    public class Mrt
    {

        public DicomFile dicomFile;

        public Mrt(String dicomFilePath)
        {
            this.dicomFile = new DicomFile(dicomFilePath);
        }
        public void SetupMetaInfo()
        {
            dicomFile.TransferSyntax = TransferSyntax.ExplicitVrLittleEndian;

            dicomFile.MetaInfo[DicomTags.ImplementationClassUid].SetStringValue("ClassUid");
            dicomFile.MetaInfo[DicomTags.ImplementationVersionName].SetStringValue("VersionName");
        }       
        public void SetupRequiredMrtData(DicomTypes.RequiredMrtData data)
        {
            dicomFile.DataSet[DicomTags.PatientPosition].SetStringValue(data.PatientPosition);
            dicomFile.DataSet[DicomTags.PhotometricInterpretation].SetStringValue(data.PhotometricInterpretation);

            dicomFile.DataSet[DicomTags.StudyInstanceUid].SetStringValue(data.StudyInstanceUid);
            dicomFile.DataSet[DicomTags.SeriesInstanceUid].SetStringValue(data.SeriesInstanceUid);
            dicomFile.DataSet[DicomTags.FrameOfReferenceUid].SetStringValue(RequiredMrtData.uid());


            dicomFile.DataSet[DicomTags.InstanceCreationDate].SetStringValue(RequiredMrtData.GetDate(DateTime.Now));
            dicomFile.DataSet[DicomTags.InstanceCreationTime].SetStringValue(RequiredMrtData.GetTime(DateTime.Now));
        }
        public void SetupRequiredMathMrtData(DicomTypes.RequiredMathMrtData data, uint rows, uint columns, uint frames, byte[] pixelArray)
        {
           
            dicomFile.DataSet[DicomTags.SliceThickness].SetStringValue(data.SliceThickness);
            dicomFile.DataSet[DicomTags.SpacingBetweenSlices].SetStringValue(data.SpacingBetweenSlices);                      

            dicomFile.DataSet[DicomTags.ImagePositionPatient].SetString(0, data.ImagePositionPatient0);
            dicomFile.DataSet[DicomTags.ImagePositionPatient].SetString(1, data.ImagePositionPatient1);
            dicomFile.DataSet[DicomTags.ImagePositionPatient].SetString(2, data.ImagePositionPatient2);

            dicomFile.DataSet[DicomTags.ImageOrientationPatient].SetString(0, data.ImageOrientationPatient0);
            dicomFile.DataSet[DicomTags.ImageOrientationPatient].SetString(1, data.ImageOrientationPatient1);
            dicomFile.DataSet[DicomTags.ImageOrientationPatient].SetString(2, data.ImageOrientationPatient2);
            dicomFile.DataSet[DicomTags.ImageOrientationPatient].SetString(3, data.ImageOrientationPatient3);
            dicomFile.DataSet[DicomTags.ImageOrientationPatient].SetString(4, data.ImageOrientationPatient4);
            dicomFile.DataSet[DicomTags.ImageOrientationPatient].SetString(5, data.ImageOrientationPatient5);


            dicomFile.DataSet[DicomTags.SamplesPerPixel].SetStringValue(data.SamplesPerPixel);


            dicomFile.DataSet[DicomTags.Rows].SetUInt32(0, rows);
            dicomFile.DataSet[DicomTags.Columns].SetUInt32(0, columns);
            dicomFile.DataSet[DicomTags.PixelSpacing].SetString(0, data.PixelSpacing0);
            dicomFile.DataSet[DicomTags.PixelSpacing].SetString(1, data.PixelSpacing1);
            dicomFile.DataSet[DicomTags.PixelRepresentation].SetStringValue(data.PixelRepresentation);

            dicomFile.DataSet[DicomTags.BitsAllocated].SetStringValue(data.BitsAllocated);
            dicomFile.DataSet[DicomTags.BitsStored].SetStringValue(data.BitsStored);
            dicomFile.DataSet[DicomTags.HighBit].SetStringValue(data.HighBit);            

            uint length = rows * columns * frames;
            if (length % 2 == 1)
                length++;
            DicomAttributeOW pixels = new DicomAttributeOW(DicomTags.PixelData);

            pixelArray[length - 1] = 0x00;

            pixels.Values = pixelArray;

            dicomFile.DataSet[DicomTags.PixelData] = pixels;
        }
        public void SetupAdditionalMrtData(Dictionary <uint , string> data)
        {
            if (data != null)
            {
                foreach (var item in data)
                {
                    dicomFile.DataSet[item.Key].SetStringValue(item.Value);
                }
            }          
        }
        
        public void Save()
        {
            dicomFile.Save(DicomWriteOptions.Default);
        }
    }    
}
