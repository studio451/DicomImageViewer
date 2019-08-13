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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Drawing.Drawing2D;
using System.Threading;

using DicomUtils;
using DicomImageViewer.Dicom;
using ClearCanvas.Dicom;
using ClearCanvas.ImageViewer.Mathematics;

namespace DicomImageViewer
{

    public partial class DicomImageViewControl : LayoutControl
    {
        #region Public fields

        /// <summary>
        /// Текущий DicomElement
        /// </summary>
        public DicomElement CurrentDicomElement;
                
        /// <summary>
        /// Список Dicom-элементов ассоциированных с контролом
        /// </summary>
        public List<DicomElement> DicomElements = new List<DicomElement>();

        /// <summary>
        /// Список линий пересечений с другими dcm-файлами, отображаемыми в данном контроле
        /// </summary>
        public List<ReferenceLine> ReferenceLines = new List<ReferenceLine>();

        /// <summary>
        /// Список путей всех dcm-файлов 
        /// </summary>
        public List<string> FilePaths
        {
            get
            {
                List<string> filePaths = new List<string>();

                foreach (DicomElement dicomElement in this.DicomElements)
                {
                    filePaths.Add(dicomElement.FilePath);
                }
                return filePaths;
                    
            }
        }

        public AnnotationManager AnnotationManager;
        public FilterManager FilterManager;
        #endregion

        #region Private fields
        private static SolidBrush currentBrushFont;
        private static SolidBrush currentBrushBorder;
        private static SolidBrush currentBrushFontOnBorder;

        private System.Timers.Timer timer;
        private Dicom.ImageViewerManager imageViewerManager;

        private Bitmap originalImage;
        private Bitmap filteredImage;

        private FreeHandAnnotation currentFreeHandAnnotation;
        private ArrowAnnotation currentArrowAnnotation;
        private RulerAnnotation currentRulerAnnotation;
        private SelectAnnotation currentSelectAnnotation;

        private int paddingTop = 25;
        private int paddingRight = 3;
        private int paddingBottom = 3;
        private int paddingLeft = 3;

        private int zoomx = 0;
        private int zoomy = 0;

        private int number = 0;
        #endregion

        private double pixelScale;

        private double realZoomFactor
        {
            get { return this.ZoomFactor / windowZoomFactor; }
            set
            {

                this.ZoomFactor = value * windowZoomFactor;


            }
        }
        private double windowZoomFactor;

        private bool zooming;
        private Point zoomStartPoint;
        private Point zoomEndPoint;
        private double zoomFactor;

        /// <summary>
        /// Коэффициент увеличения изображения
        /// </summary>
        public double ZoomFactor
        {
            get
            {
                if (zoomStartPoint.Y - zoomEndPoint.Y < -1 || zoomStartPoint.Y - zoomEndPoint.Y > 1)
                {
                    if (this.zoomFactor + (zoomStartPoint.Y - zoomEndPoint.Y) * 0.01 < 0.1 && this.zoomFactor + (zoomStartPoint.Y - zoomEndPoint.Y) * 0.01 < 8.0)
                    {
                        return 0.1;
                    }
                    if (this.zoomFactor + (zoomStartPoint.Y - zoomEndPoint.Y) * 0.01 > 9.0)
                    {
                        return 9.0;
                    }
                    return this.zoomFactor + (zoomStartPoint.Y - zoomEndPoint.Y) * 0.01;
                }
                else
                {
                    return this.zoomFactor;
                }
            }
            set
            {
                this.zoomFactor = value;
            }

        }


        private bool offseting;
        private Point offsetStartPoint;
        private Point offsetEndPoint;
        private Point offsetFactor;

        /// <summary>
        /// Коэффициент смещения изображения
        /// </summary>
        public Point OffsetFactor
        {
            get
            {
                return new Point(this.offsetFactor.X - this.offsetEndPoint.X + this.offsetStartPoint.X,
                 this.offsetFactor.Y - this.offsetEndPoint.Y + this.offsetStartPoint.Y);
            }
            set
            {
                this.offsetFactor = value;
            }

        }

        private bool arrowing;
        private bool freehanding;

        private ToolTip toolTip = new ToolTip();

        public DicomImageViewControl(Dicom.ImageViewerManager imageViewerManager)
        {
            InitializeComponent();

            this.imageViewerManager = imageViewerManager;

            this.filteredImage = new Bitmap(this.Width, this.Height);

            this.AllowDrop = true;
            this.Selected = false;
            this.ZoomFactor = 1;

            this.AnnotationManager = new AnnotationManager(convertToDestination, convertToOrigin);
            this.AnnotationManager.AnnotationAdded += new AnnotationManager.CollectionChanged(annotationManager_AnnotationAdded);
            this.AnnotationManager.AnnotationDeleted += new AnnotationManager.CollectionChanged(annotationManager_AnnotationDeleted);
            this.AnnotationManager.AnnotationCleared += new AnnotationManager.CollectionCleared(annotationManager_AnnotationCleared);

            this.FilterManager = new FilterManager(this);

            this.BorderStyle = BorderStyle.None;
            this.Padding = new Padding(0);

            this.closeButton.Visible = false;
            this.dicomButton.Visible = false;
            this.centerButton.Visible = false;
            this.invertButton.Visible = false;
            this.increaseButton.Visible = false;

            toolTip.SetToolTip(closeButton, "Close");
            toolTip.SetToolTip(dicomButton, "Show/hide DICOM properties");
            toolTip.SetToolTip(centerButton, "Center");
            toolTip.SetToolTip(invertButton, "Invert colors");
            toolTip.SetToolTip(increaseButton, "Expand window");

            this.BackColor = System.Drawing.SystemColors.ControlText;
            this.Location = new System.Drawing.Point(0, 0);
            this.OffsetFactor = new System.Drawing.Point(0, 0);
            this.TabIndex = 0;
            this.ZoomFactor = 1;

        }

      
        public void LoadImage()
        {
            if (this.CurrentDicomElement != null)
            {
                this.originalImage = this.CurrentDicomElement.Bitmap;

                if (this.InvokeRequired == true)
                {
                    this.Invoke((ThreadStart)delegate ()
                    {
                        this.closeButton.Visible = true;
                        this.dicomButton.Visible = true;
                        this.centerButton.Visible = true;
                        this.invertButton.Visible = true;
                        this.increaseButton.Visible = true;
                        SwitchIncreaseButton();
                        this.Invalidate();
                    });
                }
                else
                {
                    this.closeButton.Visible = true;
                    this.dicomButton.Visible = true;
                    this.centerButton.Visible = true;
                    this.invertButton.Visible = true;
                    this.increaseButton.Visible = true;
                    SwitchIncreaseButton();
                    this.Invalidate();
                }



                this.AnnotationManager.Number = this.number;
            }
            else
            {
                this.originalImage = new Bitmap(512, 512);
                SwitchIncreaseButton();
            }

        }

        ///<summary>
        ///Возвращает текущее изображение в исходное состояние
        ///</summary>
        public void ResetImage()
        {
            this.OffsetFactor = new Point();
            this.ZoomFactor = 1;

            this.Invalidate();
        }

        /// <summary>
        /// Очищает менеджеры (AnnotationManager,FilterManager) связанные с контролом 
        /// </summary>
        public void ClearManagers()
        {
            AnnotationManager.Clear();
            FilterManager.Clear();
        }
        /// <summary>
        ///Полностью очищает контрол от связанных dcm-данных 
        /// </summary>
        public void Clear()
        {
            this.ResetImage();                      

            this.number = 0;           

            this.CurrentDicomElement = null;
            this.originalImage = new Bitmap(512, 512);

            this.closeButton.Visible = false;
            this.dicomButton.Visible = false;
            this.centerButton.Visible = false;
            this.invertButton.Visible = false;
            this.increaseButton.Visible = false;

            this.DicomElements.Clear();
            this.ReferenceLines.Clear();

            this.ClearManagers();

            this.Invalidate();
        }

        #region DCM Player
        /// <summary>
        /// Показывает состояние проигрываетеля dcm-файлов
        /// </summary>
        /// <returns>true - если контрол в состоянии PLAY, false - если контрол в состоянии STOP</returns>
        public bool IsPlay()
        {
            if (this.timer != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Запуск проигрывателя dcm-файлов
        /// </summary>
        public void Play()
        {
            this.timer = new System.Timers.Timer();
            this.timer.Interval = 300;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(nextImageHandler);
            this.timer.Start();
        }

        /// <summary>
        /// Остановка проигрывателя dcm-файлов
        /// </summary>
        public void Stop()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
            }
            this.timer = null;
        }

        private void nextImageHandler(object sender, EventArgs e)
        {
            this.NextImage();
        }
        #endregion
        
        private bool selecting;
        private bool rulling;

        private bool textToolActive;
        private bool freehandToolActive;
        private bool arrowToolActive;
        private bool selectToolActive;
        private bool rulerToolActive;
        private bool zoomToolActive;
        private bool offsetToolActive;
        private bool brightnessToolActive;
        private bool localizeToolActive;

        #region StateChange
        internal void TextToolStateChanged(bool newState)
        {
            this.textToolActive = newState;
        }

        internal void FreehandToolStateChanged(bool newState)
        {
            this.freehandToolActive = newState;
        }

        internal void ArrowToolStateChanged(bool newState)
        {
            this.arrowToolActive = newState;
        }

        internal void OffsetToolStateChanged(bool newState)
        {
            this.offsetToolActive = newState;
        }

        internal void SelectToolStateChanged(bool newState)
        {
            this.selectToolActive = newState;
        }

        internal void RulerToolStateChanged(bool newState)
        {
            this.rulerToolActive = newState;

        }

        internal void ZoomToolStateChanged(bool newState)
        {
            this.zoomToolActive = newState;

        }

        internal void BrightnessToolStateChanged(bool newState)
        {
            this.brightnessToolActive = newState;

        }

        internal void LocalizeToolStateChanged(bool newState)
        {
            this.localizeToolActive = newState;

        }
        #endregion

        internal void NextImage()
        {
            if (this.CurrentDicomElement != null)
            {
                if (this.number + 1 < DicomElements.Count)
                {
                    this.number++;
                    this.CurrentDicomElement = DicomElements[this.number];

                    this.LoadImage();
                }
                else
                {
                    this.number = 0;
                    this.CurrentDicomElement = DicomElements[0];
                    this.LoadImage();
                }

                imageViewerManager.ReferenceLineManager.Refresh();

            }
        }

        internal void PreviousImage()
        {
            if (this.CurrentDicomElement != null)
            {
                if (this.number - 1 >= 0)
                {
                    this.number--;
                    this.CurrentDicomElement = DicomElements[this.number];
                    this.LoadImage();
                }
                else
                {
                    this.number = DicomElements.Count - 1;
                    this.CurrentDicomElement = DicomElements[this.number];
                    this.LoadImage();
                }

                imageViewerManager.ReferenceLineManager.Refresh();
            }

        }

        internal void ExportCurrent(string fileName, ImageFormat format)
        {
            if (this.CurrentDicomElement != null)
            {
                this.filteredImage.Save(fileName, format);
            }
        }

        private ImageFormat ToImageFormat(string extension)
        {
            if (extension.Equals(".jpg"))
            {
                return ImageFormat.Jpeg;
            }
            else if (extension.Equals(".png"))
            {
                return ImageFormat.Png;
            }
            else
            {
                return ImageFormat.Bmp;
            }

        }

        internal void DisableSelecting()
        {
            this.selecting = false;
        }

        void annotationManager_AnnotationAdded(Annotation annotation)
        {
        }

        void annotationManager_AnnotationDeleted(Annotation annotation)
        {
        }

        void annotationManager_AnnotationCleared()
        {
        }

        public string GetPropertyValue(FieldInfo fieldInfo)
        {
            if (this.CurrentDicomElement != null)
            {
                if ((uint)fieldInfo.GetValue(null) > this.CurrentDicomElement.DicomFile.DataSet.StartTagValue && (uint)fieldInfo.GetValue(null) < this.CurrentDicomElement.DicomFile.DataSet.EndTagValue)
                {
                    StringBuilder sb = new StringBuilder();

                    DicomAttribute da = this.CurrentDicomElement.DicomFile.DataSet[(uint)fieldInfo.GetValue(null)];

                    if (!da.IsEmpty)
                    {
                        da.Dump(sb, "", DicomDumpOptions.None);



                        return sb.ToString() + " >" + da.GetType() + " >" + da.GetValueType();
                    }
                }
            }
            return "";
        }

        internal bool IsDicomElementSet()
        {
            return this.CurrentDicomElement != null;
        }

        public Point convertToOrigin(Point point)
        {

            int width = this.Width - (this.paddingLeft + this.paddingRight);
            int height = this.Height - (this.paddingTop + this.paddingBottom);

            double wK1 = (double)this.originalImage.Width / width;
            double hK1 = (double)this.originalImage.Height / height;

            double wK2 = 1.0;
            double hK2 = 1.0;

            point.X = point.X + this.OffsetFactor.X;
            point.Y = point.Y + this.OffsetFactor.Y;

            zoomx = width / 2 + this.paddingLeft;
            zoomy = height / 2 + this.paddingTop;

            point.X = (int)((point.X - zoomx) / this.ZoomFactor) + zoomx;
            point.Y = (int)((point.Y - zoomy) / this.ZoomFactor) + zoomy;

            point.X = point.X - this.paddingLeft;
            point.Y = point.Y - this.paddingTop;


            if (this.originalImage.Width < this.originalImage.Height)
            {
                if (this.originalImage.Width / hK1 > width)
                {
                    hK2 = this.originalImage.Width / hK1 / width;

                }
                else
                {
                    wK2 = this.originalImage.Height / wK1 / height;
                }

                point.Y -= (int)(height - this.originalImage.Height / hK1 / hK2) / 2;
                point.X -= (int)(width - this.originalImage.Width / wK1 / wK2) / 2;

                if (this.originalImage.Width / hK1 > width)
                {
                    point.Y = (int)((double)point.Y * hK2);
                    point.X = (int)((double)point.X * hK2);
                }

                point.Y = (int)((double)point.Y * hK1);
                point.X = (int)((double)point.X * hK1);
            }
            else
            {
                if (this.originalImage.Height / wK1 > height)
                {
                    wK2 = this.originalImage.Height / wK1 / height;
                }
                else
                {
                    hK2 = this.originalImage.Width / hK1 / width;
                }
                point.Y -= (int)(height - this.originalImage.Height / hK1 / hK2) / 2;
                point.X -= (int)(width - this.originalImage.Width / wK1 / wK2) / 2;

                if (this.originalImage.Height / wK1 > height)
                {
                    point.Y = (int)((double)point.Y * wK2);
                    point.X = (int)((double)point.X * wK2);
                }

                point.Y = (int)((double)point.Y * wK1);
                point.X = (int)((double)point.X * wK1);
            }

            return point;
        }
        public Point convertToDestination(Point point)
        {


            int width = this.Width - (this.paddingLeft + this.paddingRight);
            int height = this.Height - (this.paddingTop + this.paddingBottom);

            double wK1 = (double)this.originalImage.Width / width;
            double hK1 = (double)this.originalImage.Height / height;

            double wK2 = 1.0;
            double hK2 = 1.0;

            if (this.originalImage.Width < this.originalImage.Height)
            {
                point.Y = (int)((double)point.Y / hK1);
                point.X = (int)((double)point.X / hK1);

                if (this.originalImage.Width / hK1 > width)
                {
                    hK2 = this.originalImage.Width / hK1 / width;
                    point.Y = (int)((double)point.Y / hK2);
                    point.X = (int)((double)point.X / hK2);
                }
                else
                {
                    wK2 = this.originalImage.Height / wK1 / height;
                }
            }
            else
            {
                point.Y = (int)((double)point.Y / wK1);
                point.X = (int)((double)point.X / wK1);

                if (this.originalImage.Height / wK1 > height)
                {
                    wK2 = this.originalImage.Height / wK1 / height;
                    point.Y = (int)((double)point.Y / wK2);
                    point.X = (int)((double)point.X / wK2);
                }
                else
                {
                    hK2 = this.originalImage.Width / hK1 / width;
                }
            }
            point.X += (int)(width - this.originalImage.Width / wK1 / wK2) / 2;
            point.Y += (int)(height - this.originalImage.Height / hK1 / hK2) / 2;
            windowZoomFactor = hK1 * hK2;


            point.X = point.X + this.paddingLeft;
            point.Y = point.Y + this.paddingTop;

            zoomx = width / 2 + this.paddingLeft;
            zoomy = height / 2 + this.paddingTop;

            point.X = (int)((point.X - zoomx) * this.ZoomFactor) + zoomx;
            point.Y = (int)((point.Y - zoomy) * this.ZoomFactor) + zoomy;

            point.X = point.X - this.OffsetFactor.X;
            point.Y = point.Y - this.OffsetFactor.Y;

            return point;
        }


        private DicomImagePlane _currentImagePlane = null;
        public DicomImagePlane CurrentImagePlane
        {
            get
            {
                if (_currentImagePlane != null)
                {
                    return _currentImagePlane;
                }
                else
                {
                    if (CurrentDicomElement != null)
                    {
                        _currentImagePlane = DicomImagePlane.FromImage(CurrentDicomElement.PresentationImage);
                    }
                    return _currentImagePlane;
                }
            }
        }

        public Point Convert3DTo2DDestination(Vector3D point3D)
        {
            Vector3D _p = this.CurrentImagePlane.ConvertToImagePlane(point3D);
            PointF _image_p = this.CurrentImagePlane.ConvertToImage(new PointF(_p.X, _p.Y));
            Point __p = new Point((int)_image_p.X, (int)_image_p.Y);
            return this.convertToDestination(__p);
        }


        private int selectFontSize()
        {
            if (this.Width < 300)
            {
                return 7;
            }
            if (this.Width < 500)
            {
                return 8;
            }
            return 9;
        }

        #region DicomImageView Paint 

        private void DicomImageView_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.FillRectangle(Brushes.Black, 0, 0, this.Width, this.Height);

            if (this.Selected == true)
            {
                currentBrushFont = imageViewerManager.BrushSelectedFont;
                currentBrushBorder = imageViewerManager.BrushSelectedBorder;
                currentBrushFontOnBorder = imageViewerManager.BrushSelectedFontOnBorder;
            }
            else
            {
                currentBrushFont = imageViewerManager.BrushUnselectedFont;
                currentBrushFontOnBorder = imageViewerManager.BrushUnselectedFontOnBorder;
                currentBrushBorder = imageViewerManager.BrushUnselectedBorder;
            }

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (this.CurrentDicomElement != null)
            {
                DrawImageToScreen(g);

                int textX1 = this.paddingLeft + 2;
                int textX2 = this.Width - this.paddingRight - 2;
                int textY1 = this.paddingTop + 2;
                int textY2 = this.Height - this.paddingBottom - 2;

                int textH = 15;
                Font captionFont = new Font("Tahoma", 9);
                Font captionFontBold = new Font("Tahoma", 9, FontStyle.Bold);

                Font textFont = new Font("Tahoma", this.selectFontSize());




                //DICOM DATA
                if (dicomButton.SwitchOn)
                {


                    //Левый верхний угол
                    g.DrawString(CurrentDicomElement.InstanceNumber.ToString(), textFont, currentBrushFont, new Point(textX1, textY1 + 0 * textH));
                    g.DrawString(CurrentDicomElement.ImageSop.StudyDescription, textFont, currentBrushFont, new Point(textX1, textY1 + 1 * textH));
                    g.DrawString(CurrentDicomElement.ImageSop.ProtocolName, textFont, currentBrushFont, new Point(textX1, textY1 + 2 * textH));
                    g.DrawString(CurrentDicomElement.ImageSop.Frames[1].SliceThickness.ToString("F01") + "mm", textFont, currentBrushFont, new Point(textX1, textY1 + 3 * textH));
                    g.DrawString(CurrentDicomElement.ImageSop.Frames[1].SliceLocation.ToString("F01") + "mm", textFont, currentBrushFont, new Point(textX1, textY1 + 4 * textH));

                    //Нижний левый угол
                    //g.DrawString("", textFont, CurrentBrushFont, new Point(textX1, textY2 - 2 * textH));

                    //Превью локализатора (нижний левый угол)
                    //g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 60)), new Rectangle(5, this.Height - 45, 40, 40));
                    //g.DrawLine(new Pen(Color.Gray), new Point(5, this.Height - 45), new Point(45, this.Height - 45 + 40));
                    //g.DrawLine(new Pen(Color.Gray), new Point(5, 40 + this.Height - 45), new Point(45, 0 + this.Height - 45));

                    //Нижний правый угол
                    //g.DrawString("", textFont, CurrentBrushFont, new Point(textX2 - TextRenderer.MeasureText("", textFont).Width, textY2 - 4 * textH));

                    //Верхний правый угол
                    g.DrawString(CurrentDicomElement.ImageSop.PatientsName.LastName + " " + CurrentDicomElement.ImageSop.PatientsName.FirstName, textFont, currentBrushFont,
                                           new Point(textX2 - TextRenderer.MeasureText(CurrentDicomElement.ImageSop.PatientsName.LastName + " " + CurrentDicomElement.ImageSop.PatientsName.FirstName, textFont).Width, textY1 + 0 * textH));
                    g.DrawString(CurrentDicomElement.ImageSop.PatientsBirthDate, textFont, currentBrushFont,
                                           new Point(textX2 - TextRenderer.MeasureText(CurrentDicomElement.ImageSop.PatientsBirthDate, textFont).Width, textY1 + 1 * textH));
                    g.DrawString(CurrentDicomElement.ImageSop.InstitutionName, textFont, currentBrushFont,
                        new Point(textX2 - TextRenderer.MeasureText(CurrentDicomElement.ImageSop.InstitutionName, textFont).Width, textY1 + 2 * textH));
                    g.DrawString(CurrentDicomElement.ImageSop.Frames[1].AcquisitionDate, textFont, currentBrushFont,
                                            new Point(textX2 - TextRenderer.MeasureText(CurrentDicomElement.ImageSop.Frames[1].AcquisitionDate, textFont).Width, textY1 + 3 * textH));


                }
                //END DICOM DATA

                imageViewerManager.LocalizeManager.RepaintLocalizers(g, this);
                AnnotationManager.RepaintAnnotations(g, this.pixelScale);                
                this.DrawScaleToScreen(g);
                this.DrawReferenceLinesToScreen(g);


                //DRAW BORDER
                g.SmoothingMode = SmoothingMode.None;
                Pen pen = new Pen(currentBrushBorder, 2);
                pen.Alignment = PenAlignment.Inset;

                g.DrawRectangle(pen, 0, 0, this.Width, this.Height);
                g.FillRectangle(currentBrushBorder, new Rectangle(0, 0, this.Width, this.paddingTop));
                g.SmoothingMode = SmoothingMode.AntiAlias;
                //END DRAW BORDER


                //HEADER
                g.DrawString(CurrentDicomElement.ImageSop.PatientsName.ToString() + " - " + CurrentDicomElement.ImageSop.Frames[1].AcquisitionDate.ToString(), captionFontBold, currentBrushFontOnBorder, new Point(textX1, (this.paddingTop - TextRenderer.MeasureText(CurrentDicomElement.ImageSop.PatientsName.ToString(), captionFontBold).Height) / 2));

                //Темный прямоугольник в правом верхнем углу
                g.SmoothingMode = SmoothingMode.None;
                g.FillRectangle(new SolidBrush(Color.FromArgb(30, 30, 30)), new Rectangle(this.Width - 83, 4, 81, 19));
                g.SmoothingMode = SmoothingMode.AntiAlias;
                

                //Ориентация пациента
                this.DrawImageOrientationPatient(g);
                

                if (CommonModule.TEST_MODE)
                {
                    g.DrawString("Font size:" + this.selectFontSize().ToString(), new Font("Tahoma", 8), Brushes.Yellow, this.paddingLeft + 2, 100);
                    g.DrawString("W:" + this.Width.ToString(), new Font("Tahoma", 8), Brushes.Yellow, this.paddingLeft + 2, 110);

                    g.DrawString("Pixel col:" + this.CurrentDicomElement.ImageSop.Frames[1].PixelSpacing.Column.ToString(), new Font("Tahoma", 8), Brushes.Yellow, this.paddingLeft + 2, 190);
                    g.DrawString("Pixel row:" + this.CurrentDicomElement.ImageSop.Frames[1].PixelSpacing.Row.ToString(), new Font("Tahoma", 8), Brushes.Yellow, this.paddingLeft + 2, 200);
                    g.DrawString("windowZF:" + windowZoomFactor.ToString("F03"), new Font("Tahoma", 8), Brushes.Yellow, this.paddingLeft + 2, 230);
                    g.DrawString("Path:" + this.CurrentDicomElement.FilePath, new Font("Tahoma", 8), Brushes.Yellow, this.paddingLeft + 2, 250);

                    g.DrawString((this.realZoomFactor * 100).ToString("F01") + "%", textFont, currentBrushFont,
                    new Point(textX2 - TextRenderer.MeasureText((this.realZoomFactor * 100).ToString("F01") + "%", textFont).Width, textY2 - 3 * textH));
                }

            }
            else
            {
                //DRAW BORDER
                g.SmoothingMode = SmoothingMode.None;
                Pen pen = new Pen(currentBrushBorder, 2);
                pen.Alignment = PenAlignment.Inset;

                g.DrawRectangle(pen, 0, 0, this.Width, this.Height);
                //END DRAW BORDER
            }
        }

        #endregion

        #region Draw Methods

        private void DrawScaleToScreen(Graphics g)
        {
            Pen scalePenWhite = new Pen(currentBrushFont, 1);
            Pen scalePenBlack = new Pen(Brushes.Black, 1);
            int widthCM = 1;
            if (this.CurrentDicomElement.ImageSop.Frames[1].PixelSpacing.Column != 0)
            {
                this.pixelScale = (1 / this.CurrentDicomElement.ImageSop.Frames[1].PixelSpacing.Column) * this.realZoomFactor;
                widthCM = (int)(10 * pixelScale);
            }
            if (widthCM < 1) widthCM = 1;

            GraphicsPath path = new GraphicsPath();

            Rectangle screen = new Rectangle(this.paddingLeft, this.paddingTop,
                this.Width - (this.paddingLeft + this.paddingRight), this.Height - (this.paddingTop + this.paddingBottom));
            Point centerScreenPoint = new Point(screen.Width / 2 + paddingLeft, screen.Height / 2 + paddingTop);

            for (int i = -5; i <= 5; i++)
            {
                path.StartFigure();

                Point point1 = new Point(centerScreenPoint.X + (int)(i * widthCM), this.paddingTop + 4);

                int height = 4;
                if (i == -5 || i == 5)
                {
                    height = 10;
                }
                if (i == 0)
                {
                    height = 15;
                }
                Point point2 = new Point(centerScreenPoint.X + (int)(i * widthCM), this.paddingTop + 4 + height);

                path.AddLine(point1, point2);
                path.CloseFigure();
            }

            g.DrawPath(scalePenWhite, path);

            System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
            m.Translate(1, 1);
            path.Transform(m);
            g.DrawPath(scalePenBlack, path);

            int heightCM = 1;
            if (this.CurrentDicomElement.ImageSop.Frames[1].PixelSpacing.Row != 0)
            {
                heightCM = (int)((10 / this.CurrentDicomElement.ImageSop.Frames[1].PixelSpacing.Row) * this.realZoomFactor);
            }
            if (heightCM < 1) heightCM = 1;

            path = new GraphicsPath();
            for (int i = -5; i <= 5; i++)
            {
                path.StartFigure();
                int width = 4;
                if (i == -5 || i == 5)
                {
                    width = 10;
                }
                if (i == 0)
                {
                    width = 15;
                }
                Point point1 = new Point(this.Width - this.paddingRight - 4 - width, centerScreenPoint.Y + (int)(i * heightCM));
                Point point2 = new Point(this.Width - this.paddingRight - 4, centerScreenPoint.Y + (int)(i * heightCM));

                path.AddLine(point1, point2);
                path.CloseFigure();
            }

            g.DrawPath(scalePenWhite, path);
            path.Transform(m);
            g.DrawPath(scalePenBlack, path);

            int d = 50;
            g.DrawString("1 sm", new Font("Tahoma", 8), currentBrushFont, new Point(this.Width - this.paddingRight - d - widthCM / 2 - 13, this.Height - 19));

            scalePenWhite.StartCap = LineCap.DiamondAnchor;
            scalePenWhite.EndCap = LineCap.DiamondAnchor;
            g.DrawLine(scalePenWhite, new Point(this.Width - this.paddingRight - d, this.Height - 20), new Point(this.Width - this.paddingRight - d - widthCM, this.Height - 20));

            scalePenBlack.StartCap = LineCap.DiamondAnchor;
            scalePenBlack.EndCap = LineCap.DiamondAnchor;
            g.DrawLine(scalePenBlack, new Point(this.Width - this.paddingRight - d + 2, this.Height - 17), new Point(this.Width - this.paddingRight - d - widthCM + 2, this.Height - 17));

            if (CommonModule.TEST_MODE)
            {
                g.DrawString("1 sm = " + ((int)((10 / this.CurrentDicomElement.ImageSop.Frames[1].PixelSpacing.Column))).ToString() + "px (1:1)", new Font("Tahoma", 8), Brushes.Yellow, new Point(this.paddingLeft + 2, 300));
                g.DrawString("1 sm = " + widthCM.ToString() + "px", new Font("Tahoma", 8), Brushes.Yellow, new Point(this.paddingLeft + 2, 310));
                Pen pen = new Pen(Brushes.Yellow, 1);
                pen.StartCap = LineCap.DiamondAnchor;
                pen.EndCap = LineCap.DiamondAnchor;
                g.DrawLine(pen, new Point(this.paddingLeft + 5, 330), new Point(this.paddingLeft + 5 + widthCM, 330));
            }
        }

        private void DrawImageToScreen(Graphics g)
        {
            GraphicsUnit units = GraphicsUnit.Pixel;

            Point p1 = new Point(0, 0);
            Point p2 = new Point(this.originalImage.Width, this.originalImage.Height);

            p1 = convertToDestination(p1);
            p2 = convertToDestination(p2);

            Rectangle screen = new Rectangle(this.paddingLeft, this.paddingTop,
                this.Width - (this.paddingLeft + this.paddingRight), this.Height - (this.paddingTop + this.paddingBottom));
            Point centerScreenPoint = new Point(screen.Width / 2 + paddingLeft, screen.Height / 2 + paddingTop);
            Rectangle destination = new Rectangle(p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
            Point centerDestinationPoint = new Point(destination.Width / 2 + p1.X, destination.Height / 2 + p1.Y);

            //IMAGE ATTRIBUTES 
            ImageAttributes imageAttr = new ImageAttributes();
            imageAttr.SetGamma(1.0F);
            //END IMAGE ATTRIBUTES

            FilterManager.ApplyFilters(ref this.filteredImage, this.originalImage);


            try
            {
                checked
                {
                    g.DrawImage(
                        this.filteredImage,
                        destination,
                        0, 0,
                        this.originalImage.Width, this.originalImage.Height,
                        units,
                        imageAttr);
                }
            }
            catch (Exception ex)
            {
                g.DrawString(
                    ex.ToString(),
                    new Font("Tahoma", 8),
                    Brushes.Black,
                    new PointF(0, 0));
            }

            if (centerButton.SwitchOn == true)
            {
                CommonModule.DrawPoint(g, centerDestinationPoint, imageViewerManager.BrushSelectedFont);
            }

            if (CommonModule.TEST_MODE)
            {
                g.DrawRectangle(new Pen(Brushes.Yellow, 1), destination);
                g.DrawRectangle(new Pen(Brushes.Yellow, 1), screen);

                CommonModule.DrawPoint(g, centerDestinationPoint);
                CommonModule.DrawPoint(g, centerScreenPoint);

                if (this.zoomToolActive)
                {
                    CommonModule.DrawPoint(g, zoomStartPoint);
                    CommonModule.DrawPoint(g, zoomEndPoint);
                    g.DrawString(this.ZoomFactor.ToString(), new Font("Tahoma", 8), Brushes.Yellow, new Point(zoomEndPoint.X, zoomEndPoint.Y - 20));
                }

                g.DrawString("W: " + this.originalImage.Width + "px", new Font("Tahoma", 8), Brushes.Yellow, new Point(this.paddingLeft + 2, 130));
                g.DrawString("H: " + this.originalImage.Height + "px", new Font("Tahoma", 8), Brushes.Yellow, new Point(this.paddingLeft + 2, 140));

                g.DrawString("gridX: " + this.GridX, new Font("Tahoma", 8), Brushes.Yellow, new Point(this.paddingLeft + 2, 150));
                g.DrawString("gridY: " + this.GridY, new Font("Tahoma", 8), Brushes.Yellow, new Point(this.paddingLeft + 2, 160));
                g.DrawString("Offset: " + this.Offset, new Font("Tahoma", 8), Brushes.Yellow, new Point(this.paddingLeft + 2, 170));
            }
        }

        private void DrawReferenceLinesToScreen(Graphics g)
        {
            foreach (ReferenceLine line in this.ReferenceLines)
            {
                Point p1 = new Point((int)line.StartPoint.X, (int)line.StartPoint.Y);
                Point p2 = new Point((int)line.EndPoint.X, (int)line.EndPoint.Y);

                p1 = convertToDestination(p1);
                p2 = convertToDestination(p2);

                g.DrawLine(new Pen(Brushes.Yellow, 1), p1, p2);

                g.DrawString(line.Label, new Font("Tahoma", 8), Brushes.Yellow, p2);
            }

        }

        private void DrawImageOrientationPatient(Graphics g)
        {
            //Ориентация пациента относительно изображения
            //None = 0,
            //Left = 1,
            //Right = -1,
            //Posterior = 2, //Спина
            //Anterior = -2, //Перед
            //Head = 3
            //Foot = -3,

            //Право
            g.DrawString(this.CurrentDicomElement.ImageSop.Frames[1].ImageOrientationPatient.GetPrimaryRowDirection(false).ToString().Substring(0, 1), new Font("Tahoma", 8), Brushes.Yellow, this.Width - this.paddingRight - 30, this.Height / 2 + 4);
            //Лево
            g.DrawString(this.CurrentDicomElement.ImageSop.Frames[1].ImageOrientationPatient.GetPrimaryRowDirection(true).ToString().Substring(0, 1), new Font("Tahoma", 8), Brushes.Yellow, this.paddingLeft + 2, this.Height / 2 + 4);
            //Верх
            g.DrawString(this.CurrentDicomElement.ImageSop.Frames[1].ImageOrientationPatient.GetPrimaryColumnDirection(true).ToString().Substring(0, 1), new Font("Tahoma", 8), Brushes.Yellow, this.Width / 2 - 5, this.paddingTop + 20);
            //Низ
            g.DrawString(this.CurrentDicomElement.ImageSop.Frames[1].ImageOrientationPatient.GetPrimaryColumnDirection(false).ToString().Substring(0, 1), new Font("Tahoma", 8), Brushes.Yellow, this.Width / 2 - 5, this.Height - this.paddingBottom - 14);

        }
        #endregion

        #region DicomImageView MouseEvents
        private void DicomImageView_MouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();
            this.imageViewerManager.CurrentDicomImageViewControl = this;
            if (e.Button == MouseButtons.Right && CurrentDicomElement != null)
            {
                if (imageViewerManager.LocalizeManager.MouseDown((DicomImageViewControl)sender, e))
                {
                    return;
                }
            }

            if (e.Button == MouseButtons.Left && CurrentDicomElement != null)
            {
                if (imageViewerManager.LocalizeManager.MouseDown((DicomImageViewControl)sender, e))
                {
                    return;
                }

                if (this.AnnotationManager.MouseDown(e.Location))
                {
                    return;
                }



                if (this.textToolActive)
                {
                    this.imageViewerManager.annotateTextForm.Clear();
                    this.imageViewerManager.annotateTextForm.Location = new Point(this.imageViewerManager.LayoutControl.Left + this.Left + e.X, this.imageViewerManager.LayoutControl.Top + this.Top + e.Y);
                    this.imageViewerManager.annotateTextForm.ShowDialog();

                }

                if (this.freehandToolActive)
                {
                    freehanding = true;
                    if (currentFreeHandAnnotation == null)
                    {
                        currentFreeHandAnnotation = new FreeHandAnnotation();
                        currentFreeHandAnnotation.Color = this.imageViewerManager.CurrentColor;
                        currentFreeHandAnnotation.Width = this.imageViewerManager.CurrentBrushSize;
                        AnnotationManager.AddAnnotation(currentFreeHandAnnotation);
                    }
                }
                if (this.selectToolActive)
                {
                    selecting = true;
                    if (currentSelectAnnotation == null)
                    {
                        currentSelectAnnotation = new SelectAnnotation();
                        currentSelectAnnotation.Color = this.imageViewerManager.CurrentColor;
                        currentSelectAnnotation.StartPoint = this.convertToOrigin(e.Location);
                        currentSelectAnnotation.EndPoint = new Point(currentSelectAnnotation.StartPoint.X, currentSelectAnnotation.StartPoint.Y - 15);
                        AnnotationManager.AddAnnotation(currentSelectAnnotation);
                    }

                }
                if (this.rulerToolActive)
                {
                    rulling = true;
                    if (currentRulerAnnotation == null)
                    {
                        currentRulerAnnotation = new RulerAnnotation();
                        currentRulerAnnotation.Color = this.imageViewerManager.CurrentColor;
                        currentRulerAnnotation.StartPoint = this.convertToOrigin(e.Location);
                        currentRulerAnnotation.EndPoint = new Point(currentRulerAnnotation.StartPoint.X, currentRulerAnnotation.StartPoint.Y - 15);
                        AnnotationManager.AddAnnotation(currentRulerAnnotation);
                    }
                    return;
                }
                if (this.zoomToolActive)
                {
                    zooming = true;
                    this.zoomStartPoint = this.zoomEndPoint = e.Location;
                    return;

                }
                if (this.offsetToolActive)
                {
                    offseting = true;
                    this.offsetStartPoint = this.offsetEndPoint = e.Location;
                    return;
                }
                if (this.arrowToolActive)
                {
                    arrowing = true;
                    if (currentArrowAnnotation == null)
                    {
                        currentArrowAnnotation = new ArrowAnnotation();
                        currentArrowAnnotation.Color = this.imageViewerManager.CurrentColor;
                        currentArrowAnnotation.Width = this.imageViewerManager.CurrentBrushSize;
                        currentArrowAnnotation.StartPoint = this.convertToOrigin(e.Location);
                        currentArrowAnnotation.EndPoint = new Point(currentArrowAnnotation.StartPoint.X, currentArrowAnnotation.StartPoint.Y - 15);
                        AnnotationManager.AddAnnotation(currentArrowAnnotation);
                    }
                    return;
                }
            }
        }

        private void DicomImageView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && CurrentDicomElement != null)
            {
                if (imageViewerManager.LocalizeManager.MouseMove((DicomImageViewControl)sender, e))
                {
                    this.Invalidate();
                    return;
                }
            }

            if (e.Button == MouseButtons.Left && CurrentDicomElement != null)
            {
                if (imageViewerManager.LocalizeManager.MouseMove((DicomImageViewControl)sender, e))
                {
                    this.Invalidate();
                    return;
                }

                if (this.AnnotationManager.MouseMove(e.Location))
                {
                    this.Invalidate();
                    return;
                }



                if (this.freehanding)
                {
                    Point originPoint = this.convertToOrigin(e.Location);
                    currentFreeHandAnnotation.AddPoint(originPoint.X, originPoint.Y);

                }

                if (this.selecting)
                {
                    currentSelectAnnotation.EndPoint = this.convertToOrigin(e.Location);
                }

                if (this.rulling)
                {
                    currentRulerAnnotation.EndPoint = this.convertToOrigin(e.Location);
                }

                if (this.offseting)
                {
                    this.offsetEndPoint = e.Location;
                }

                if (this.zooming)
                {
                    this.zoomEndPoint = e.Location;
                }

                if (this.arrowing)
                {
                    currentArrowAnnotation.EndPoint = this.convertToOrigin(e.Location);
                }

                this.Invalidate();
            }
        }

        private void DicomImageView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && CurrentDicomElement != null)
            {
                imageViewerManager.LocalizeManager.MouseUp((DicomImageViewControl)sender, this.convertToOrigin(e.Location));
            }


            if (e.Button == MouseButtons.Left && CurrentDicomElement != null)
            {
                this.AnnotationManager.MouseUp(this.convertToOrigin(e.Location));

                imageViewerManager.LocalizeManager.MouseUp((DicomImageViewControl)sender, this.convertToOrigin(e.Location));


                if (this.textToolActive)
                {

                }
                if (this.freehandToolActive && currentFreeHandAnnotation != null)
                {
                    currentFreeHandAnnotation.Close();
                    currentFreeHandAnnotation = null;
                    freehanding = false;

                }
                if (this.arrowToolActive && currentArrowAnnotation != null)
                {
                    currentArrowAnnotation = null;
                    arrowing = false;
                }
                if (this.rulerToolActive && currentRulerAnnotation != null)
                {
                    currentRulerAnnotation = null;
                    rulling = false;
                }
                if (this.selectToolActive && currentSelectAnnotation != null)
                {
                    currentSelectAnnotation = null;
                    selecting = false;
                }


                offseting = false;
                this.offsetFactor = this.OffsetFactor;
                this.offsetStartPoint = this.offsetEndPoint = new Point();

                zooming = false;
                this.zoomFactor = this.ZoomFactor;
                this.zoomEndPoint = this.zoomStartPoint;

                this.Invalidate();
            }
        }

        private void DicomImageView_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.imageViewerManager.CurrentDicomImageViewControl = this ;
            if (e.Delta > 0)
            {
                NextImage();
            }
            else
            {
                PreviousImage();
            }
        }

        #endregion

        #region DicomImageView Buttons
        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Clear();
        }
        bool flagIncrease = false;
        private void increaseButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (flagIncrease == false)
            {
                if (imageViewerManager.LayoutManager.GridX != 1 && imageViewerManager.LayoutManager.GridY != 1)
                {
                    imageViewerManager.LayoutManager.Offset = this.Offset;
                    imageViewerManager.LayoutManager.GridX = 1;
                    imageViewerManager.LayoutManager.GridY = 1;
                    imageViewerManager.LayoutManager.ChangeLayout();
                }
                flagIncrease = true;
            }
            else
            {
                if (imageViewerManager.LayoutManager.GridX != imageViewerManager.LayoutManager.oldGridX ||
                    imageViewerManager.LayoutManager.GridY != imageViewerManager.LayoutManager.oldGridY)
                {
                    imageViewerManager.LayoutManager.GridX = imageViewerManager.LayoutManager.oldGridX;
                    imageViewerManager.LayoutManager.GridY = imageViewerManager.LayoutManager.oldGridY;
                    imageViewerManager.LayoutManager.ChangeLayout();
                }
                flagIncrease = false;
            }
            SwitchIncreaseButton();
        }
        private void centerButton_MouseDown(object sender, MouseEventArgs e)
        {
            this.OffsetFactor = new Point();
            this.Invalidate();
        }
        bool flagInvert = true;
        private void invertButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (flagInvert == true)
            {
                this.FilterManager.AddFilter("Invert");
                flagInvert = false;
            }
            else
            {
                this.FilterManager.DeleteAllFiltersByName("Invert");
                flagInvert = true;
            }
        }
        public void SwitchIncreaseButton()
        {


            if ((imageViewerManager.LayoutManager.oldGridX == 1 && imageViewerManager.LayoutManager.oldGridY == 1) && (imageViewerManager.LayoutManager.GridX == 1 && imageViewerManager.LayoutManager.GridY == 1))
            {
                increaseButton.Visible = false;
            }
            else
            {
                if (this.CurrentDicomElement != null)
                {
                    increaseButton.Visible = true;
                }
            }
        }
        #endregion

        #region DicomImageViewControl KeyEvents
        public override void KeyDown(KeyEventArgs e)
        {
            if (this.AnnotationManager.KeyDown(e.KeyCode))
            {
                this.Invalidate();
                return;
            }
        }
        #endregion



        ///<summary>Обработка события DragDrop - открываем файлы *.dcm</summary>
        private void DicomImageView_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);

            List<string> directories = new List<string>();
            List<string> files = new List<string>();

            foreach (string fileName in fileNames)
            {
                FileInfo fi = new FileInfo(fileName);

                if (fi.Exists)
                {
                    if (fi.Extension.ToLower() == ".dcm")
                    {
                        files.Add(fi.FullName);
                    }
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(fileName);
                    if (di.Exists)
                    {
                        directories.Add(di.FullName);
                    }
                }
            }
            
            //Считаем что передали папку с серией, берем первую
            if (directories.Count > 0)
            {
                this.imageViewerManager.CurrentDicomImageViewControl = this;
                this.imageViewerManager.LoadSeries(directories[0]);
                return;
            }
            //Считаем что передали dcm-файлы, берем все файлы
            if (files.Count > 0)
            {
                this.imageViewerManager.CurrentDicomImageViewControl = this;
                this.imageViewerManager.LoadDcmFiles(files);
                return;
            }

        }

        ///<summary>Обработка события DragEnter - проверяем есть ли файлы *.dcm</summary>
        private void DicomImageView_DragEnter(object sender, DragEventArgs e)
        {
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);

            foreach (string fileName in fileNames)
            {
                FileInfo fi = new FileInfo(fileName);

                if (fi.Exists)
                {
                    e.Effect = DragDropEffects.All;
                    return;
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(fileName);
                    if (di.Exists)
                    {
                        e.Effect = DragDropEffects.All;
                        return;
                    }
                }
            }

            e.Effect = DragDropEffects.None;
        }
    }    
}
