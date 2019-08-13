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
using System.Linq;
using System.Drawing;
using ClearCanvas.ImageViewer;
using ClearCanvas.ImageViewer.Mathematics;
using DicomUtils;

namespace DicomImageViewer.Dicom
{
    public class ReferenceLine
    {
        public readonly PointF StartPoint;
        public readonly PointF EndPoint;
        public readonly string Label;

        public ReferenceLine(PointF startPoint, PointF endPoint, string label)
        {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this.Label = label;
        }
    }
    public class ReferenceLineManager
    {
        private Dicom.ImageViewerManager imageViewerManager;

        public ReferenceLineManager(Dicom.ImageViewerManager imageViewerManager)
        {

            this.imageViewerManager = imageViewerManager;
        }

        private DicomImagePlane _currentReferenceImagePlane
        {
            get
            {
                return DicomImagePlane.FromImage(this.imageViewerManager.CurrentDicomImageViewControl.CurrentDicomElement.PresentationImage);
            }
        }

        private static readonly float _oneDegreeInRadians = (float)(Math.PI / 180);

        private IEnumerable<DicomImagePlane> GetPlanesParallelToReferencePlane()
        {
            DicomImagePlane currentReferenceImagePlane = DicomImagePlane.FromImage(this.imageViewerManager.CurrentDicomImageViewControl.CurrentDicomElement.PresentationImage);
            foreach (DicomElement dicomElement in this.imageViewerManager.CurrentDicomImageViewControl.DicomElements)
            {
                DicomImagePlane plane = DicomImagePlane.FromImage(dicomElement.PresentationImage);
                if (plane != null)
                {
                    if (currentReferenceImagePlane.IsInSameFrameOfReference(plane) &&
                        currentReferenceImagePlane.IsParallelTo(plane, _oneDegreeInRadians))
                    {
                        yield return plane;
                    }
                }
            }
        }

        private static ReferenceLine GetReferenceLine(DicomImagePlane referenceImagePlane, DicomImagePlane targetImagePlane)
        {
            const float parallelTolerance = (float)(Math.PI / 18);
            if (referenceImagePlane.IsParallelTo(targetImagePlane, parallelTolerance))
                return null;

            Vector3D intersectionPatient1, intersectionPatient2;
            if (!referenceImagePlane.GetIntersectionPoints(targetImagePlane, out intersectionPatient1, out intersectionPatient2))
                return null;

            Vector3D intersectionImagePlane1 = targetImagePlane.ConvertToImagePlane(intersectionPatient1);
            Vector3D intersectionImagePlane2 = targetImagePlane.ConvertToImagePlane(intersectionPatient2);

            PointF intersectionImage1 = targetImagePlane.ConvertToImage(new PointF(intersectionImagePlane1.X, intersectionImagePlane1.Y));
            PointF intersectionImage2 = targetImagePlane.ConvertToImage(new PointF(intersectionImagePlane2.X, intersectionImagePlane2.Y));

            return new ReferenceLine(intersectionImage1, intersectionImage2, "");
        }

        private void GetFirstAndLastReferenceLines(DicomImagePlane targetImagePlane, out ReferenceLine firstReferenceLine, out ReferenceLine lastReferenceLine)
        {
            firstReferenceLine = lastReferenceLine = null;

            float firstReferenceImageZComponent = float.MaxValue;
            float lastReferenceImageZComponent = float.MinValue;

            foreach (DicomImagePlane parallelPlane in GetPlanesParallelToReferencePlane())
            {                
                if (parallelPlane.PositionImagePlaneTopLeft.Z < firstReferenceImageZComponent)
                {
                    ReferenceLine referenceLine = GetReferenceLine(parallelPlane, targetImagePlane);
                    if (referenceLine != null)
                    {
                        firstReferenceImageZComponent = parallelPlane.PositionImagePlaneTopLeft.Z;
                        firstReferenceLine = referenceLine;
                    }
                }
                
                if (parallelPlane.PositionImagePlaneTopLeft.Z >= lastReferenceImageZComponent)
                {
                    ReferenceLine referenceLine = GetReferenceLine(parallelPlane, targetImagePlane);
                    if (referenceLine != null)
                    {
                        lastReferenceImageZComponent = parallelPlane.PositionImagePlaneTopLeft.Z;
                        lastReferenceLine = referenceLine;
                    }
                }
            }
        }

        private IEnumerable<ReferenceLine> GetAllReferenceLines(DicomImagePlane targetImagePlane)
        {
            ReferenceLine firstReferenceLine = null;
            ReferenceLine lastReferenceLine = null;
            GetFirstAndLastReferenceLines(targetImagePlane, out firstReferenceLine, out lastReferenceLine);

            if (firstReferenceLine != null)
                yield return firstReferenceLine;

            if (lastReferenceLine != null)
                yield return lastReferenceLine;

            ReferenceLine currentReferenceLine = GetReferenceLine(_currentReferenceImagePlane, targetImagePlane);
            if (currentReferenceLine != null)
                yield return currentReferenceLine;
        }


        public void Refresh()
        {
            foreach (DicomImageViewControl control in this.imageViewerManager.LayoutManager.layoutControls)
            {
                control.ReferenceLines.Clear();
                if (control.CurrentDicomElement != null)
                {
                    IPresentationImage targetImage = control.CurrentDicomElement.PresentationImage;
                    if (targetImage != this.imageViewerManager.CurrentDicomImageViewControl.CurrentDicomElement.PresentationImage)
                    {

                        DicomImagePlane targetImagePlane = DicomImagePlane.FromImage(targetImage);
                        if (targetImagePlane != null)
                        {
                            control.ReferenceLines = GetAllReferenceLines(targetImagePlane).ToList();
                        }
                    }
                }
                control.Invalidate();
            }
        }

        public void Clear()
        {          
            foreach (DicomImageViewControl control in this.imageViewerManager.LayoutManager.layoutControls)
            {
                control.ReferenceLines.Clear();                       
            }
        }
    }
}
