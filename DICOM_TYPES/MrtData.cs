//Copyright © 2018 studio451.ru
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//E-mail: info@studio451.ru
//URL: https://studio451.ru/dicomimageviewer

using System;
using ClearCanvas.Dicom;

namespace DicomImageViewer.DicomTypes
{
    public class MrtData
    {
        private static string zeroAdd(int a, int zeroAmount)
        {
            if (a < 0) { a = -a; }
            if (zeroAmount == 1)
            {
                if (a < 10) return "0" + a.ToString();
                return a.ToString();
            }
            if (zeroAmount == 2)
            {
                if (a < 10) return "00" + a.ToString();
                if (a >= 10 && a < 100) return "0" + a.ToString();
                return a.ToString();
            }
            return "";
        }
        public static string GetDate(DateTime dt)
        {
            return dt.Year.ToString() + MrtData.zeroAdd(dt.Month, 1) + MrtData.zeroAdd(dt.Day, 1);
        }
        public static string GetTime(DateTime dt)
        {
            return MrtData.zeroAdd(dt.Hour, 1) + MrtData.zeroAdd(dt.Minute, 1) + MrtData.zeroAdd(dt.Second, 1) + MrtData.zeroAdd(dt.Millisecond, 2) + "000";
        }
        public static string uid()
        {
            return DicomUid.GenerateUid().UID;
        }

    }

        public class RequiredMrtData : MrtData
    {
        public string PatientPosition = "";
        public string StudyInstanceUid = RequiredMrtData.uid();
        public string SeriesInstanceUid = RequiredMrtData.uid();

        public string StudyId = "";
        public string SeriesNumber = "";
        public string PhotometricInterpretation = "MONOCHROME2";     
          
    }
    public class RequiredMathMrtData : MrtData
    {
        public string SliceThickness = "";
        public string SpacingBetweenSlices = "";

        public string SamplesPerPixel = "";       
        public string BitsAllocated = "";
        public string BitsStored = "";
        public string HighBit = "";
        public string PixelRepresentation = "";
        public string PixelSpacing0 = "";
        public string PixelSpacing1 = "";

        public string ImagePositionPatient0 = "";
        public string ImagePositionPatient1 = "";
        public string ImagePositionPatient2 = "";

        public string ImageOrientationPatient0 = "";
        public string ImageOrientationPatient1 = "";
        public string ImageOrientationPatient2 = "";
        public string ImageOrientationPatient3 = "";
        public string ImageOrientationPatient4 = "";
        public string ImageOrientationPatient5 = "";
    }    
}
