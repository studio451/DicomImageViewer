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
using System.Text;
using System.Drawing;
using ClearCanvas.ImageViewer.StudyManagement;
using ClearCanvas.ImageViewer;
using ClearCanvas.Dicom;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.Mathematics;
using ClearCanvas.Common.Utilities;
using DicomUtils;
using System.Drawing.Drawing2D;
using ClearCanvas.Dicom.Iod;
using System.Windows.Forms;
using static ClearCanvas.Dicom.Iod.ImageOrientationPatient;

namespace DicomImageViewer.Dicom
{
    public class Slice2D
    {
        public Point p0;
        public Point p1;
        public Point p2;
        public Point p3;

        public int Number;
    }
    public class Slice3D
    {
        public Vector3D p0;
        public Vector3D p1;
        public Vector3D p2;
        public Vector3D p3;

        public int Number;

        public void Rotate(double x_angel = 0, double y_angel = 0, double z_angel = 0, Vector3D offset = null)
        {
            if (offset == null)
            {
                offset = new Vector3D(0, 0, 0);
            }

            p0 = p0 - offset;
            p1 = p1 - offset;
            p2 = p2 - offset;
            p3 = p3 - offset;

            //Углы переводим в радианы
            x_angel = x_angel * Math.PI / 180;
            y_angel = y_angel * Math.PI / 180;
            z_angel = z_angel * Math.PI / 180;

            float sx = (float)Math.Sin(x_angel);
            float sy = (float)Math.Sin(y_angel);
            float sz = (float)Math.Sin(z_angel);

            float cx = (float)Math.Cos(x_angel);
            float cy = (float)Math.Cos(y_angel);
            float cz = (float)Math.Cos(z_angel);

            Matrix transform = new Matrix(4, 4);

            transform.SetColumn(0,
                                    1 * cy * cz,
                                    sx * sy * cz + cx * 1 * sz,
                                    sx * 1 * sz - cx * sy * cz,
                                    0);

            transform.SetColumn(1,
                                    -1 * cy * sz,
                                    cx * 1 * cz - sx * sy * sz,
                                    sx * 1 * cz + cx * sy * sz,
                                    0);

            transform.SetColumn(2,
                                    1 * sy * 1,
                                    -sx * cy * 1,
                                    cx * cy * 1,
                                    0);


            transform.SetColumn(3, 0, 0, 0, 1F);

            Matrix point3D = new Matrix(4, 1);

            point3D.SetColumn(0, p0.X, p0.Y, p0.Z, 1F);
            Matrix result0 = transform * point3D;
            p0 = new Vector3D(result0[0, 0], result0[1, 0], result0[2, 0]);

            point3D.SetColumn(0, p1.X, p1.Y, p1.Z, 1F);
            Matrix result1 = transform * point3D;
            p1 = new Vector3D(result1[0, 0], result1[1, 0], result1[2, 0]);

            point3D.SetColumn(0, p2.X, p2.Y, p2.Z, 1F);
            Matrix result2 = transform * point3D;
            p2 = new Vector3D(result2[0, 0], result2[1, 0], result2[2, 0]);

            point3D.SetColumn(0, p3.X, p3.Y, p3.Z, 1F);
            Matrix result3 = transform * point3D;
            p3 = new Vector3D(result3[0, 0], result3[1, 0], result3[2, 0]);

            p0 = p0 + offset;
            p1 = p1 + offset;
            p2 = p2 + offset;
            p3 = p3 + offset;
        }

        public void Move(Vector3D offset = null)
        {
            if (offset == null)
            {
                offset = new Vector3D(0, 0, 0);
            }

            p0 = p0 + offset;
            p1 = p1 + offset;
            p2 = p2 + offset;
            p3 = p3 + offset;
        }
    }
    public class Localize
    {
        public List<Slice3D> Slices3D = new List<Slice3D>();

        DicomImageViewControl Control;

        private int sliceNumber;
        private float sliceWidth;
        private float sliceSize;
        private int xFOV;
        private int yFOV;

        public Vector3D p0 = new Vector3D(0, 0, 0);

        //Направляющие косинусы плоскости перпендикулярной к локализации
        public ImageOrientationPatient ImageOrientationPatientOfReferenceImage;

        public Localize(DicomImageViewControl control, int sliceNumber, float sliceWidth, float sliceSize, float xFOV, float yFOV)
        {
            this.Control = control;

            this.sliceNumber = sliceNumber;
            this.sliceWidth = (int)(sliceWidth / this.Control.CurrentDicomElement.ImageSop.Frames[1].PixelSpacing.Row);
            this.sliceSize = (int)(sliceSize / this.Control.CurrentDicomElement.ImageSop.Frames[1].PixelSpacing.Row);
            this.xFOV = (int)(xFOV / this.Control.CurrentDicomElement.ImageSop.Frames[1].PixelSpacing.Row);
            this.yFOV = (int)(yFOV / this.Control.CurrentDicomElement.ImageSop.Frames[1].PixelSpacing.Column);


            this.ImageOrientationPatientOfReferenceImage = new ImageOrientationPatient(
                this.Control.CurrentDicomElement.ImageSop.Frames[1].ImageOrientationPatient.RowX,
                this.Control.CurrentDicomElement.ImageSop.Frames[1].ImageOrientationPatient.RowY,
                this.Control.CurrentDicomElement.ImageSop.Frames[1].ImageOrientationPatient.RowZ,
                this.Control.CurrentDicomElement.ImageSop.Frames[1].ImageOrientationPatient.ColumnX,
                this.Control.CurrentDicomElement.ImageSop.Frames[1].ImageOrientationPatient.ColumnY,
                this.Control.CurrentDicomElement.ImageSop.Frames[1].ImageOrientationPatient.ColumnZ
            );

            CreateSplices3D();
        }

        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        public void Rotate(int dif, Directions row, Directions column)
        {


            //Ориентация пациента относительно изображения
            //None = 0
            //Left = 1
            //Right = -1
            //Posterior = 2
            //Anterior = -2
            //Head = 3
            //Foot = -3

            int angel_x = 0;
            int angel_y = 0;
            int angel_z = 0;


            if ((row == Directions.Anterior || row == Directions.Posterior) && (column == Directions.Head || column == Directions.Foot))
            {
                angel_x = dif;
            }
            if ((row == Directions.Left || row == Directions.Right) && (column == Directions.Head || column == Directions.Foot))
            {
                angel_y = dif;
            }
            if ((row == Directions.Left || row == Directions.Right) && (column == Directions.Anterior || column == Directions.Posterior))
            {
                angel_z = dif;
            }


            foreach (var slice3D in this.Slices3D)
            {
                slice3D.Rotate(angel_x, angel_y, angel_z, this.p0);
            }
        }

        public void Move(DicomElement dicomElement, Vector3D offset3D)
        {
            p0 = p0 + offset3D;
            foreach (var slice3D in this.Slices3D)
            {
                slice3D.Move(offset3D);
            }
        }

        public void CreateSplices3D()
        {
            Slices3D.Clear();

            DicomImagePlane referenceImagePlane = DicomImagePlane.FromImage(this.Control.CurrentDicomElement.PresentationImage);
            //Поиск центра 3D координат на 2D
            Vector3D _p = referenceImagePlane.ConvertToImagePlane(new Vector3D(0, 0, 0));
            PointF _image_p = referenceImagePlane.ConvertToImage(new PointF(_p.X, _p.Y));
            Point __p = new Point((int)_image_p.X, (int)_image_p.Y);



            this.p0 = referenceImagePlane.ConvertToPatient(__p);

            for (int i = 0; i < sliceNumber; i++)
            {
                Point p1 = new Point(__p.X - xFOV / 2, __p.Y - (int)(i * (sliceWidth + sliceSize)));
                Point p2 = new Point(__p.X - xFOV / 2, __p.Y - (int)(i * (sliceWidth + sliceSize)));
                Point p3 = new Point(__p.X + xFOV / 2, __p.Y - (int)(i * (sliceWidth + sliceSize)));
                Point p4 = new Point(__p.X + xFOV / 2, __p.Y - (int)(i * (sliceWidth + sliceSize)));

                Slice3D slice3D = new Slice3D();
                slice3D.p0 = referenceImagePlane.ConvertToPatient(p1, -yFOV / 2);
                slice3D.p1 = referenceImagePlane.ConvertToPatient(p2, yFOV / 2);
                slice3D.p2 = referenceImagePlane.ConvertToPatient(p3, yFOV / 2);
                slice3D.p3 = referenceImagePlane.ConvertToPatient(p4, -yFOV / 2);
                slice3D.Number = i + 1;

                Slices3D.Add(slice3D);
            }
        }


        public int SliceNumber
        {
            get { return sliceNumber; }
            set
            {
                this.sliceNumber = value;
                this.CreateSplices3D();
            }
        }

        public float SliceWidth
        {
            get { return sliceWidth; }
            set
            {
                this.sliceWidth = value;
                this.CreateSplices3D();
            }
        }

        public void Repaint(Graphics g, DicomImageViewControl control)
        {
            if (control.CurrentDicomElement != null)
            {
                IPresentationImage targetImage = control.CurrentDicomElement.PresentationImage;

                DicomImagePlane targetImagePlane = DicomImagePlane.FromImage(targetImage);

                List<Slice2D> Slices2D = new List<Slice2D>();

                foreach (var slice3D in this.Slices3D)
                {
                    List<Vector3D> planeIntersectionPoints = new List<Vector3D>();
                    Vector3D intersectionPoint = null;

                    // Ищем пересечения
                    intersectionPoint = Vector3D.GetLinePlaneIntersection(targetImagePlane.Normal, targetImagePlane.PositionPatientCenterOfImage, slice3D.p0, slice3D.p1, true);
                    if (intersectionPoint != null) planeIntersectionPoints.Add(intersectionPoint);
                    intersectionPoint = Vector3D.GetLinePlaneIntersection(targetImagePlane.Normal, targetImagePlane.PositionPatientCenterOfImage, slice3D.p1, slice3D.p2, true);
                    if (intersectionPoint != null) planeIntersectionPoints.Add(intersectionPoint);
                    intersectionPoint = Vector3D.GetLinePlaneIntersection(targetImagePlane.Normal, targetImagePlane.PositionPatientCenterOfImage, slice3D.p2, slice3D.p3, true);
                    if (intersectionPoint != null) planeIntersectionPoints.Add(intersectionPoint);
                    intersectionPoint = Vector3D.GetLinePlaneIntersection(targetImagePlane.Normal, targetImagePlane.PositionPatientCenterOfImage, slice3D.p3, slice3D.p0, true);
                    if (intersectionPoint != null) planeIntersectionPoints.Add(intersectionPoint);

                    if (planeIntersectionPoints.Count < 2) continue;


                    Vector3D pTargetImagePlane0 = targetImagePlane.ConvertToImagePlane(planeIntersectionPoints[0]);
                    Vector3D pTargetImagePlane1 = targetImagePlane.ConvertToImagePlane(planeIntersectionPoints[1]);

                    PointF pImage0 = targetImagePlane.ConvertToImage(new PointF(pTargetImagePlane0.X, pTargetImagePlane0.Y));
                    PointF pImage1 = targetImagePlane.ConvertToImage(new PointF(pTargetImagePlane1.X, pTargetImagePlane1.Y));

                    Slice2D slice2D = new Slice2D();

                    slice2D.p0 = new Point((int)pImage0.X, (int)pImage0.Y);
                    slice2D.p1 = new Point((int)pImage1.X, (int)pImage1.Y);
                    slice2D.Number = slice3D.Number;

                    Slices2D.Add(slice2D);
                }


                Pen pRed = new Pen(new SolidBrush(Color.FromArgb(255, Color.Red)), 1);
                Pen pYellow = new Pen(new SolidBrush(Color.FromArgb(255, Color.Yellow)), 1);
                Pen pBlue = new Pen(new SolidBrush(Color.FromArgb(255, Color.Blue)), 1);


                foreach (var slice in Slices2D)
                {
                    Point p0 = control.convertToDestination(slice.p0);
                    Point p1 = control.convertToDestination(slice.p1);

                    if (slice.Number == 1)
                    {
                        g.DrawLine(pRed, p0, p1);
                        g.DrawString("1", new Font("Tahoma", 7), Brushes.Red, new Point(p0.X - 10, p0.Y - 6));
                    }
                    else
                    {
                        if (selected)
                        {
                            g.DrawLine(pYellow, p0, p1);
                            g.DrawString(slice.Number.ToString(), new Font("Tahoma", 7), Brushes.Yellow, new Point(p0.X - 10, p0.Y - 6));
                        }
                        else
                        {
                            g.DrawLine(pBlue, p0, p1);
                            g.DrawString(slice.Number.ToString(), new Font("Tahoma", 7), Brushes.Blue, new Point(p0.X - 10, p0.Y - 6));
                        }
                    }
                }


                //Отрисовка центра 3D координат
                Vector3D _p = targetImagePlane.ConvertToImagePlane(new Vector3D(0, 0, 0));
                PointF _image_p = targetImagePlane.ConvertToImage(new PointF(_p.X, _p.Y));
                Point __p = new Point((int)_image_p.X, (int)_image_p.Y);
                Point ___p = control.convertToDestination(__p);

                g.DrawLine(pYellow, new Point(___p.X - 5, ___p.Y), new Point(___p.X + 5, ___p.Y));
                g.DrawLine(pYellow, new Point(___p.X, ___p.Y - 5), new Point(___p.X, ___p.Y + 5));

            }



        }

        public bool Contain(Point point, DicomImageViewControl control)
        {
            if (control.CurrentDicomElement != null)
            {
                IPresentationImage targetImage = control.CurrentDicomElement.PresentationImage;

                DicomImagePlane targetImagePlane = DicomImagePlane.FromImage(targetImage);

                List<Slice2D> Slices2D = new List<Slice2D>();

                foreach (var slice3D in this.Slices3D)
                {
                    List<Vector3D> planeIntersectionPoints = new List<Vector3D>();
                    Vector3D intersectionPoint = null;

                    // Ищем пересечения
                    intersectionPoint = Vector3D.GetLinePlaneIntersection(targetImagePlane.Normal, targetImagePlane.PositionPatientCenterOfImage, slice3D.p0, slice3D.p1, true);
                    if (intersectionPoint != null) planeIntersectionPoints.Add(intersectionPoint);
                    intersectionPoint = Vector3D.GetLinePlaneIntersection(targetImagePlane.Normal, targetImagePlane.PositionPatientCenterOfImage, slice3D.p1, slice3D.p2, true);
                    if (intersectionPoint != null) planeIntersectionPoints.Add(intersectionPoint);
                    intersectionPoint = Vector3D.GetLinePlaneIntersection(targetImagePlane.Normal, targetImagePlane.PositionPatientCenterOfImage, slice3D.p2, slice3D.p3, true);
                    if (intersectionPoint != null) planeIntersectionPoints.Add(intersectionPoint);
                    intersectionPoint = Vector3D.GetLinePlaneIntersection(targetImagePlane.Normal, targetImagePlane.PositionPatientCenterOfImage, slice3D.p3, slice3D.p0, true);
                    if (intersectionPoint != null) planeIntersectionPoints.Add(intersectionPoint);

                    if (planeIntersectionPoints.Count < 2) continue;

                    Vector3D pTargetImagePlane0 = targetImagePlane.ConvertToImagePlane(planeIntersectionPoints[0]);
                    Vector3D pTargetImagePlane1 = targetImagePlane.ConvertToImagePlane(planeIntersectionPoints[1]);

                    PointF pImage0 = targetImagePlane.ConvertToImage(new PointF(pTargetImagePlane0.X, pTargetImagePlane0.Y));
                    PointF pImage1 = targetImagePlane.ConvertToImage(new PointF(pTargetImagePlane1.X, pTargetImagePlane1.Y));

                    Slice2D slice2D = new Slice2D();

                    slice2D.p0 = new Point((int)pImage0.X, (int)pImage0.Y);
                    slice2D.p1 = new Point((int)pImage1.X, (int)pImage1.Y);

                    Slices2D.Add(slice2D);

                }

                GraphicsPath path = new GraphicsPath();

                if (Slices2D.Count > 1)
                {

                    Point p0 = control.convertToDestination(Slices2D[0].p0);
                    Point p1 = control.convertToDestination(Slices2D[0].p1);

                    Point p2 = control.convertToDestination(Slices2D[Slices2D.Count - 1].p1);
                    Point p3 = control.convertToDestination(Slices2D[Slices2D.Count - 1].p0);

                    path.StartFigure();
                    path.AddLine(p0, p1);
                    path.AddLine(p1, p2);
                    path.AddLine(p2, p3);
                    path.AddLine(p3, p0);
                    path.CloseFigure();

                    if (path.IsVisible(point.X, point.Y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }




    }
    public class LocalizeManager
    {

        public HashSet<Localize> Localizes = new HashSet<Localize>();

        public int DefaultSliceNumber = 10;
        public float DefaultSliceWidth = 2;
        public float DefaultSliceSize = 5;
        public float xFOV = 100;
        public float yFOV = 80;

        private Dicom.ImageViewerManager imageViewerManager;


        public LocalizeManager(Dicom.ImageViewerManager imageViewerManager)
        {
            this.imageViewerManager = imageViewerManager;
        }

        public void CreateLocalize(DicomImageViewControl referenceDicomElement)
        {
            if (referenceDicomElement.CurrentDicomElement != null)
            {
                Localizes.Add(new Localize(referenceDicomElement, this.DefaultSliceNumber, this.DefaultSliceWidth, this.DefaultSliceSize, this.xFOV, this.yFOV));
                if (Localizes.Count() > 0)
                {
                    imageViewerManager.ReferenceLineManager.Refresh();
                }
                this.RepaintAllLocalizers(referenceDicomElement);
            }
        }

        public void RepaintAllLocalizers(DicomImageViewControl butThis = null)
        {
            foreach (DicomImageViewControl control in this.imageViewerManager.LayoutManager.layoutControls)
            {
                if (control != butThis || butThis == null)
                {
                    control.Invalidate();
                }
            }
        }

        public void Select(Localize localizeSelected)
        {

            foreach (Localize localize in Localizes)
            {
                if (localizeSelected == localize)
                {
                    localize.Selected = true;
                }
                else
                {
                    localize.Selected = false;
                }
            }

        }

        public void RepaintLocalizers(Graphics g, DicomImageViewControl control)
        {
            if (this.Localizes.Count > 0)
            {
                if (control.CurrentDicomElement != null)
                {
                    foreach (Localize localize in this.Localizes)
                    {
                        localize.Repaint(g, control);
                    }
                }
            }
        }

        public void Clear()
        {
            this.Localizes.Clear();            
            imageViewerManager.ReferenceLineManager.Clear();           
            RepaintAllLocalizers();
        }

        #region Mouse Events

        public bool flagMove = false;
        public bool flagRotate = false;

        public Point mouseDownPoint;
        public Vector3D mouseDownPoint3D;


        public bool MouseDown(DicomImageViewControl sender, MouseEventArgs e)
        {

            foreach (Localize localize in Localizes)
            {
                if (localize.Contain(e.Location, sender))
                {
                    Select(localize);
                }
            }
            if (e.Button == MouseButtons.Left)
            {
                this.flagMove = true;

                if (Localizes.Count() > 0)
                {
                    imageViewerManager.ReferenceLineManager.Refresh();
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                this.flagRotate = true;
                if (Localizes.Count() > 0)
                {
                    imageViewerManager.ReferenceLineManager.Refresh();
                }
            }

            mouseDownPoint = sender.convertToOrigin(e.Location);
            mouseDownPoint3D = sender.CurrentImagePlane.ConvertToPatient(sender.convertToOrigin(e.Location));

            return false;
        }

        public bool MouseMove(DicomImageViewControl sender, MouseEventArgs e)
        {

            if (this.flagMove == true)
            {
                Vector3D mouseMovePoint3D = sender.CurrentImagePlane.ConvertToPatient(sender.convertToOrigin(e.Location));
                if (Localizes.Count > 0)
                {
                    IPresentationImage targetImage = sender.CurrentDicomElement.PresentationImage;
                    DicomImagePlane targetImagePlane = DicomImagePlane.FromImage(targetImage);

                    foreach (Localize localize in Localizes)
                    {
                        if (localize.Selected)
                        {
                            localize.Move(sender.CurrentDicomElement, mouseMovePoint3D - mouseDownPoint3D);
                        }
                    }
                    RepaintAllLocalizers(sender);
                }

                mouseDownPoint3D = mouseMovePoint3D;

            }

            if (this.flagRotate == true)
            {
                Point mouseMovePoint = sender.convertToOrigin(e.Location);
                Vector3D mouseMovePoint3D = sender.CurrentImagePlane.ConvertToPatient(sender.convertToOrigin(e.Location));

                if (Localizes.Count > 0)
                {
                    IPresentationImage targetImage = sender.CurrentDicomElement.PresentationImage;
                    DicomImagePlane targetImagePlane = DicomImagePlane.FromImage(targetImage);
                    
                    Directions row = sender.CurrentDicomElement.ImageSop.Frames[1].ImageOrientationPatient.GetPrimaryRowDirection(false);
                    Directions col = sender.CurrentDicomElement.ImageSop.Frames[1].ImageOrientationPatient.GetPrimaryColumnDirection(false);

                    foreach (Localize localize in Localizes)
                    {
                        if (localize.Selected)
                        {
                            localize.Rotate(mouseMovePoint.X - mouseDownPoint.X, row, col);
                        }
                    }
                    RepaintAllLocalizers();
                }

                mouseDownPoint = mouseMovePoint;
                mouseDownPoint3D = mouseMovePoint3D;
            }

            return false;
        }

        public bool MouseUp(DicomImageViewControl sender, Point point)
        {
            this.flagMove = false;
            this.flagRotate = false;
            return false;
        }
        #endregion Mouse Events
    }
}
