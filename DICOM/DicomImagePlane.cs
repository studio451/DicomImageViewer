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
using ClearCanvas.ImageViewer.StudyManagement;
using ClearCanvas.ImageViewer;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.Mathematics;
using ClearCanvas.Common.Utilities;

namespace DicomImageViewer.Dicom
{
    using DicomImagePlaneDataCache = Dictionary<string, DicomImagePlane>;
    using System.Drawing;
    using ClearCanvas.Dicom.Iod;

    public class DicomImagePlane
    {
        #region Private Fields

        private static int _referenceCount = 0;
        private static readonly DicomImagePlaneDataCache _imagePlaneDataCache = new DicomImagePlaneDataCache();
        private IPresentationImage _sourceImage;
        private SpatialTransform _sourceImageTransform;
        private Frame _sourceFrame;

        private Vector3D _normal;
        private Vector3D _positionPatientTopLeft;
        private Vector3D _positionPatientTopRight;
        private Vector3D _positionPatientBottomLeft;
        private Vector3D _positionPatientBottomRight;
        private Vector3D _positionPatientCenterOfImage;
        private Vector3D _positionImagePlaneTopLeft;

        #endregion

        private DicomImagePlane()
        {
        }

        public static void InitializeCache()
        {
            ++_referenceCount;
        }

        public static void ReleaseCache()
        {
            if (_referenceCount > 0)
                --_referenceCount;

            if (_referenceCount == 0)
                _imagePlaneDataCache.Clear();
        }

        #region Factory Method

        public static DicomImagePlane FromImage(IPresentationImage sourceImage)
        {
            if (sourceImage == null)
                return null;

            Frame frame = GetFrame(sourceImage);
            SpatialTransform transform = GetSpatialTransform(sourceImage);

            if (transform == null || frame == null)
                return null;

            if (String.IsNullOrEmpty(frame.FrameOfReferenceUid)
                ||
                String.IsNullOrEmpty(frame.ParentImageSop.StudyInstanceUID)
                )
                return null;

            DicomImagePlane plane;
            if (_referenceCount > 0)
                plane = CreateFromCache(frame);
            else
                plane = CreateFromFrame(frame);

            if (plane != null)
            {
                plane._sourceImage = sourceImage;
                plane._sourceImageTransform = transform;
                plane._sourceFrame = frame;
            }

            return plane;
        }

        #endregion

        #region Private Methods

        private static SpatialTransform GetSpatialTransform(IPresentationImage image)
        {
            if (image is ISpatialTransformProvider)
                return ((ISpatialTransformProvider)image).SpatialTransform as SpatialTransform;

            return null;
        }

        private static Frame GetFrame(IPresentationImage image)
        {
            if (image is IImageSopProvider)
                return ((IImageSopProvider)image).Frame;

            return null;
        }

        private static DicomImagePlane CreateFromCache(Frame frame)
        {
            string key = String.Format("{0}:{1}", frame.ParentImageSop.StudyInstanceUID, frame.FrameNumber);

            DicomImagePlane cachedData;
            if (_imagePlaneDataCache.ContainsKey(key))
            {
                cachedData = _imagePlaneDataCache[key];
            }
            else
            {
                cachedData = CreateFromFrame(frame);
                if (cachedData != null)
                    _imagePlaneDataCache[key] = cachedData;
            }

            if (cachedData != null)
            {
                DicomImagePlane plane = new DicomImagePlane();
                plane.InitializeWithCachedData(cachedData);
                return plane;
            }

            return null;
        }

        private static DicomImagePlane CreateFromFrame(Frame frame)
        {
            int height = frame.Rows - 1;
            int width = frame.Columns - 1;

            DicomImagePlane plane = new DicomImagePlane();
            plane.PositionPatientTopLeft = frame.ImagePlaneHelper.ConvertToPatient(new PointF(0, 0));
            plane.PositionPatientTopRight = frame.ImagePlaneHelper.ConvertToPatient(new PointF(width, 0));
            plane.PositionPatientBottomLeft = frame.ImagePlaneHelper.ConvertToPatient(new PointF(0, height));
            plane.PositionPatientBottomRight = frame.ImagePlaneHelper.ConvertToPatient(new PointF(width, height));
            plane.PositionPatientCenterOfImage = frame.ImagePlaneHelper.ConvertToPatient(new PointF(width / 2F, height / 2F));

            plane.Normal = frame.ImagePlaneHelper.GetNormalVector();

            if (plane.Normal == null || plane.PositionPatientCenterOfImage == null)
                return null;

            plane.PositionImagePlaneTopLeft = frame.ImagePlaneHelper.ConvertToImagePlane(plane.PositionPatientTopLeft, Vector3D.Null);

            return plane;
        }

        private void InitializeWithCachedData(DicomImagePlane cachedData)
        {
            Normal = cachedData.Normal;
            PositionPatientTopLeft = cachedData.PositionPatientTopLeft;
            PositionPatientTopRight = cachedData.PositionPatientTopRight;
            PositionPatientBottomLeft = cachedData.PositionPatientBottomLeft;
            PositionPatientBottomRight = cachedData.PositionPatientBottomRight;
            PositionPatientCenterOfImage = cachedData.PositionPatientCenterOfImage;
            PositionImagePlaneTopLeft = cachedData.PositionImagePlaneTopLeft;
        }

        #endregion

        #region Public Properties

        public IPresentationImage SourceImage
        {
            get { return _sourceImage; }
        }

        public SpatialTransform SourceImageTransform
        {
            get { return _sourceImageTransform; }
        }

        public string StudyInstanceUid
        {
            get { return _sourceFrame.ParentImageSop.StudyInstanceUID; }
        }

        public string SeriesInstanceUid
        {
            get { return _sourceFrame.ParentImageSop.StudyInstanceUID; }
        }

        public string SopInstanceUid
        {
            get { return _sourceFrame.ParentImageSop.StudyInstanceUID; }
        }

        public int InstanceNumber
        {
            get { return _sourceFrame.ParentImageSop.InstanceNumber; }
        }

        public int FrameNumber
        {
            get { return _sourceFrame.FrameNumber; }
        }

        public string FrameOfReferenceUid
        {
            get { return _sourceFrame.FrameOfReferenceUid; }
        }

        public float Thickness
        {
            get { return (float)_sourceFrame.SliceThickness; }
        }

        public float Spacing
        {
            get { return (float)_sourceFrame.SpacingBetweenSlices; }
        }

        public Vector3D Normal
        {
            get { return _normal; }
            private set { _normal = value; }
        }

        public Vector3D PositionPatientTopLeft
        {
            get { return _positionPatientTopLeft; }
            private set { _positionPatientTopLeft = value; }
        }

        public Vector3D PositionPatientTopRight
        {
            get { return _positionPatientTopRight; }
            private set { _positionPatientTopRight = value; }
        }

        public Vector3D PositionPatientBottomLeft
        {
            get { return _positionPatientBottomLeft; }
            private set { _positionPatientBottomLeft = value; }
        }

        public Vector3D PositionPatientBottomRight
        {
            get { return _positionPatientBottomRight; }
            private set { _positionPatientBottomRight = value; }
        }

        public Vector3D PositionPatientCenterOfImage
        {
            get { return _positionPatientCenterOfImage; }
            private set { _positionPatientCenterOfImage = value; }
        }

        public Vector3D PositionImagePlaneTopLeft
        {
            get { return _positionImagePlaneTopLeft; }
            private set { _positionImagePlaneTopLeft = value; }
        }

        #endregion

        #region Public Methods

        public Vector3D ConvertToPatient(PointF imagePoint)
        {
            return _sourceFrame.ImagePlaneHelper.ConvertToPatient(imagePoint);
        }

        public Vector3D ConvertToImagePlane(Vector3D positionPatient)
        {
            return _sourceFrame.ImagePlaneHelper.ConvertToImagePlane(positionPatient);
        }

        public Vector3D ConvertToImagePlane(Vector3D positionPatient, Vector3D originPatient)
        {
            return _sourceFrame.ImagePlaneHelper.ConvertToImagePlane(positionPatient, originPatient);
        }

        public PointF ConvertToImage(PointF positionMillimetres)
        {
            return (PointF)_sourceFrame.ImagePlaneHelper.ConvertToImage(positionMillimetres);
        }

        public bool IsInSameFrameOfReference(DicomImagePlane other)
        {
            Frame otherFrame = other._sourceFrame;

            if (_sourceFrame.ParentImageSop.StudyInstanceUID != otherFrame.ParentImageSop.StudyInstanceUID)
                return false;

            return this._sourceFrame.FrameOfReferenceUid == otherFrame.FrameOfReferenceUid;
        }

        public bool IsOrthogonalTo(DicomImagePlane other, float angleTolerance)
        {
            angleTolerance = Math.Abs(angleTolerance);
            float upper = (float)Math.PI / 2 + angleTolerance;
            float lower = (float)Math.PI / 2 - angleTolerance;

            float angle = GetAngleBetween(other);

            return FloatComparer.IsGreaterThan(angle, lower) && FloatComparer.IsLessThan(angle, upper);
        }

        public bool IsParallelTo(DicomImagePlane other, float angleTolerance)
        {
            angleTolerance = Math.Abs(angleTolerance);
            float upper = angleTolerance;
            float lower = -angleTolerance;

            float angle = GetAngleBetween(other);

            bool parallel = FloatComparer.IsGreaterThan(angle, lower) && FloatComparer.IsLessThan(angle, upper);

            if (!parallel)
            {
                upper = (float)Math.PI + angleTolerance;
                lower = (float)Math.PI - angleTolerance;
                parallel = FloatComparer.IsGreaterThan(angle, lower) && FloatComparer.IsLessThan(angle, upper);
            }

            return parallel;
        }

        public bool GetIntersectionPoints(DicomImagePlane other, out Vector3D intersectionPointPatient1, out Vector3D intersectionPointPatient2)
        {
            intersectionPointPatient1 = intersectionPointPatient2 = null;

            Vector3D[,] lineSegmentsImagePlaneBounds = new Vector3D[,]
				{
					{ PositionPatientTopLeft, PositionPatientTopRight },
					{ PositionPatientTopLeft, PositionPatientBottomLeft },
					{ PositionPatientBottomRight, PositionPatientTopRight  },
					{ PositionPatientBottomRight, PositionPatientBottomLeft}
				};

            List<Vector3D> planeIntersectionPoints = new List<Vector3D>();

            for (int i = 0; i < 4; ++i)
            {
                Vector3D intersectionPoint = Vector3D.GetLinePlaneIntersection(other.Normal, other.PositionPatientCenterOfImage,
                                                                        lineSegmentsImagePlaneBounds[i, 0],
                                                                        lineSegmentsImagePlaneBounds[i, 1], true);
                if (intersectionPoint != null)
                    planeIntersectionPoints.Add(intersectionPoint);
            }

            if (planeIntersectionPoints.Count < 2)
                return false;

            intersectionPointPatient1 = planeIntersectionPoints[0];
            intersectionPointPatient2 = CollectionUtils.SelectFirst(planeIntersectionPoints,
                delegate(Vector3D point)
                {
                    return !planeIntersectionPoints[0].Equals(point);
                });

            return intersectionPointPatient1 != null && intersectionPointPatient2 != null;
        }

        public float GetAngleBetween(DicomImagePlane other)
        {
            Vector3D normal1 = this.Normal.Normalize();
            Vector3D normal2 = other.Normal.Normalize();

            float dot = normal1.Dot(normal2);

            if (dot < -1F)
                dot = -1F;
            if (dot > 1F)
                dot = 1F;

            return Math.Abs((float)Math.Acos(dot));
        }

        public Vector3D ConvertToPatient(PointF positionPixels, int z)
        {
            
            ImagePositionPatient pos = this._sourceFrame.ImagePositionPatient;
            Vector3D _imagePositionPatient = new Vector3D((float)pos.X, (float)pos.Y, (float)pos.Z);

            ImageOrientationPatient orientation = this._sourceFrame.ImageOrientationPatient;

            Vector3D left = new Vector3D((float)orientation.RowX, (float)orientation.RowY, (float)orientation.RowZ);
            Vector3D normal = left.Cross(new Vector3D((float)orientation.ColumnX, (float)orientation.ColumnY, (float)orientation.ColumnZ));


            PixelSpacing pixelSpacing = this._sourceFrame.PixelSpacing;

            if (orientation.IsNull || pixelSpacing.IsNull)
                return null;

            Vector3D position = _imagePositionPatient;

            if (positionPixels.X == 0F && positionPixels.Y == 0F)
                return position;

                Matrix _pixelToPatientTransform = new Matrix(4, 4);

                _pixelToPatientTransform.SetColumn(0, (float)(orientation.RowX * pixelSpacing.Column),
                                     (float)(orientation.RowY * pixelSpacing.Column),
                                     (float)(orientation.RowZ * pixelSpacing.Column),0);

                _pixelToPatientTransform.SetColumn(1, (float)(orientation.ColumnX * pixelSpacing.Row),
                                     (float)(orientation.ColumnY * pixelSpacing.Row),
                                     (float)(orientation.ColumnZ * pixelSpacing.Row),0);

            _pixelToPatientTransform.SetColumn(2, (float)(normal.X * pixelSpacing.Row),
                                    (float)(normal.Y * pixelSpacing.Row),
                                    (float)(normal.Z * pixelSpacing.Row),0);

            _pixelToPatientTransform.SetColumn(3, position.X, position.Y, position.Z, 1F);


            Matrix columnMatrix = new Matrix(4, 1);
            columnMatrix.SetColumn(0, positionPixels.X, positionPixels.Y, z, 1F);
            Matrix result = _pixelToPatientTransform * columnMatrix;

            return new Vector3D(result[0, 0], result[1, 0], result[2, 0]);
        }

        #endregion
    }
}
