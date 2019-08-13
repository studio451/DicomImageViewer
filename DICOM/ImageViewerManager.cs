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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using ClearCanvas.Dicom;
using DicomUtils;
using System.IO;
using System.Reflection;
using DicomImageViewer.Properties;

namespace DicomImageViewer.Dicom
{
    public class ImageViewerManager
    {
        private DicomImageViewControl currentDicomImageViewControl;

        private System.ComponentModel.BackgroundWorker bgrwExport;
        private System.ComponentModel.BackgroundWorker bgrwLoadSeries;
        private System.ComponentModel.BackgroundWorker bgrwLoadStudy;

        public SolidBrush BrushUnselectedBorder = new SolidBrush(Color.FromArgb(100, 100, 100));
        public SolidBrush BrushSelectedBorder = new SolidBrush(Color.FromArgb(210, 210, 210));
        public SolidBrush BrushUnselectedFont = new SolidBrush(Color.FromArgb(100, 100, 100));
        public SolidBrush BrushSelectedFont = new SolidBrush(Color.FromArgb(210, 210, 210));
        public SolidBrush BrushUnselectedFontOnBorder = new SolidBrush(Color.FromArgb(210, 210, 210));
        public SolidBrush BrushSelectedFontOnBorder = new SolidBrush(Color.FromArgb(100, 100, 100));

        public ExportManager ExportManager;
        public LayoutManager LayoutManager;
        public ReferenceLineManager ReferenceLineManager;
        public LocalizeManager LocalizeManager;

        public Color CurrentColor = Color.OrangeRed;
        public int CurrentBrushSize = 3;

        public FAnnotateText annotateTextForm;
        public FDicomProperties dicomPropertiesForm;

        /// <summary>Элемент(ContainerControl) для обработки и событий клавиатуры
        public ContainerControl ContainerControl;        
        /// <summary>Элемент(Control) в которой размещается сетка
        public Control LayoutControl;


        /*EVENTS*/
        /// <summary>
        /// Событие onProgressInit, возникает в начале длительного процесса, 
        /// например загрузка файлов обследования и т.д., можно использовать для установки мин. и макс. значений ProgressBar
        /// </summary>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        public delegate void ProgressInitDelegate(int minimum, int maximum);
        public event ProgressInitDelegate onProgressInit;

        /// <summary>
        /// Событие onProgressValueChanged, возникает при изменении кол-ва выполненных операций текущего длительного процесса,
        /// например загрузка файлов обследования и т.д., можно использовать для установки значения value для ProgressBar
        /// </summary>
        /// <param name="value"></param>
        public delegate void ProgressValueChangedDelegate(int value);
        public event ProgressValueChangedDelegate onProgressValueChanged;

        /// <summary>
        /// Событие onProcessDescriptionChanged, возникает при изменении описания текущего длительного процесса,
        /// например загрузка файлов обследования и т.д.
        /// </summary>
        /// <param name="text">Текст для вывода</param>
        public delegate void ProcessDescriptionChangedDelegate(string text);
        public event ProcessDescriptionChangedDelegate onProcessDescriptionChanged;

        /// <summary>
        /// Событие возникает после загрузки серии
        /// </summary>
        /// <param name="currentDicomImageViewControl"></param>
        public delegate void SeriesLoadedDelegate(DicomImageViewControl currentDicomImageViewControl);
        public event SeriesLoadedDelegate onSeriesLoaded;

        public delegate void StudyLoadedDelegate();
        public event StudyLoadedDelegate onStudyLoaded;
        /*END EVENTS*/

        public System.Windows.Forms.FolderBrowserDialog folderBrowserDialog2;
        public System.Windows.Forms.SaveFileDialog saveFileDialogExportCurrent;
        public System.Windows.Forms.ColorDialog colorDialog1;

        /// <summary>Создает объект ImageViewerManager
		/// <param name="containerControl">Элемент(тип ContainerControl) для обработки и событий клавиатуры</param>
        /// <param name="layoutControl">Элемент(тип Control) в которой размещается сетка</param>
        public ImageViewerManager(ContainerControl containerControl, Control layoutControl)
        {
            this.ContainerControl = containerControl;
            this.LayoutControl = layoutControl;

            this.folderBrowserDialog2 = new System.Windows.Forms.FolderBrowserDialog();
            this.saveFileDialogExportCurrent = new System.Windows.Forms.SaveFileDialog();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();

            this.annotateTextForm = new FAnnotateText();
            this.dicomPropertiesForm = new FDicomProperties();

            this.saveFileDialogExportCurrent.Filter = "jpg (*.jpg)|*.jpg|bmp (*.bmp)|*.bmp|png (*.png)|*.png";

            this.LayoutManager = new DicomImageViewer.Dicom.LayoutManager(LayoutControl, this.CreateLayoutControl, this);

            this.LayoutControl.SizeChanged += new System.EventHandler(this.layoutPanel_SizeChanged);

            this.LayoutControl.DragEnter += new DragEventHandler(this.layoutPanel_DragEnter);
            
            this.ContainerControl.KeyDown += new KeyEventHandler(this.container_KeyDown);

            this.LayoutManager.ChangeLayout();

            bgrwExport_Initialize();
            bgrwLoadSeries_Initialize();
            bgrwLoadStudy_Initialize();

            this.CurrentDicomImageViewControl = (DicomImageViewControl)this.LayoutManager.GetLayoutControl(1, 1);

            string path = DriveInfo.GetDrives().First(x => x.DriveType == DriveType.Fixed).Name;
            if (string.IsNullOrEmpty(Settings.Default.ExportPath))
            {
                Settings.Default.ExportPath = System.IO.Path.Combine(path, "export");
            }
            if (string.IsNullOrEmpty(Settings.Default.PublishPath))
            {
                Settings.Default.PublishPath = System.IO.Path.Combine(path, "publish");
            }

            this.ReferenceLineManager = new ReferenceLineManager(this);

            this.LocalizeManager = new LocalizeManager(this);

        }

        /// <summary>
        /// Возвращает список DicomImageViewControl с загруженными dcm-файлами
        /// </summary>
        /// <returns>List<DicomImageViewControl></returns>
        public List<DicomImageViewControl> LoadedDicomImageViewControls
        {
            get
            {
                List<DicomImageViewControl> dicomImageViewControls = new List<DicomImageViewControl>();
                foreach (LayoutControl control in this.LayoutManager.layoutControls)
                {
                    if (((DicomImageViewControl)control).CurrentDicomElement != null)
                    {
                        dicomImageViewControls.Add((DicomImageViewControl)control);
                    }
                }
                return dicomImageViewControls;
            }
        }

        /// <summary>Загрузка серии</summary>
        /// <param name="path">Путь к папке серии (Структура: папка серии\dcm-файл)</param>
        public void LoadSeries(string path)
        {
            DirectoryInfo directoryinfo = new DirectoryInfo(path);

            List<string> filePaths = new List<string>();
            foreach (FileInfo file in directoryinfo.GetFiles("*.dcm"))
            {
                filePaths.Add(file.FullName);
            }
            bgrwLoadSeries_Run(filePaths);
        }

        /// <summary>Загрузка dcm-файлов</summary>
		/// <param name="filePaths">Строковый массив, в каждом элементе массива путь к каждому dcm-файлу</param>
        public void LoadDcmFiles(List<string> filePaths)
        {
            bgrwLoadSeries_Run(filePaths);
        }

        /// <summary>Загрузка dcm-файла</summary>
		/// <param name="filePath">Путь к dcm-файлу</param>
        public void LoadDcmFile(string filePath)
        {
            List<string> filePaths = new List<string>();
            filePaths.Add(filePath);
            bgrwLoadSeries_Run(filePaths);
        }

        /// <summary>Загрузка обследования</summary>
		/// <param name="path">Путь к папке обследования (Структура: папка обследования\папка серии\dcm-файл)</param>
        public void LoadStudy(string path)
        {
            bgrwLoadStudy_Run(path);
        }

        /// <summary>
        /// Перерисовка
        /// </summary>
        public void Invalidate()
        {
            this.LayoutControl.Invalidate();
            this.CurrentDicomImageViewControl.Invalidate();
        }

        #region Layout
        void layoutPanel_SizeChanged(object sender, System.EventArgs e)
        {
            if (this.LayoutManager != null)
            {
                this.LayoutManager.Resize();
            }
        }

        void layoutPanel_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            throw new NotImplementedException();
        }


        void container_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.T)
            {
                if (CommonModule.TEST_MODE == true)
                {
                    CommonModule.TEST_MODE = false;
                }
                else
                {
                    CommonModule.TEST_MODE = true;
                }
                this.ContainerControl.Refresh();

            }

            LayoutManager.KeyDown(e);

            e.Handled = false;
        }

        private LayoutControl CreateLayoutControl()
        {
            DicomImageViewControl dicomImageViewControl = new DicomImageViewControl(this);
            
            return dicomImageViewControl;
        }
        #endregion
        
        #region BackgroundWorkers
        #region InitializeBackgroundWorker
        private void bgrwExport_Initialize()
        {
            bgrwExport = new System.ComponentModel.BackgroundWorker();
            bgrwExport.WorkerReportsProgress = true;
            bgrwExport.WorkerSupportsCancellation = true;
            bgrwExport.DoWork +=
                new DoWorkEventHandler(bgrwExport_DoWork);
            bgrwExport.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            bgrwExport_RunWorkerCompleted);
            bgrwExport.ProgressChanged +=
                new ProgressChangedEventHandler(
            bgrwExport_ProgressChanged);
        }
        private void bgrwLoadSeries_Initialize()
        {
            bgrwLoadSeries = new System.ComponentModel.BackgroundWorker();
            bgrwLoadSeries.WorkerReportsProgress = true;
            bgrwLoadSeries.DoWork +=
                new DoWorkEventHandler(bgrwLoadSeries_DoWork);
            bgrwLoadSeries.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            bgrwLoadSeries_RunWorkerCompleted);
            bgrwLoadSeries.ProgressChanged +=
                new ProgressChangedEventHandler(
            bgrwLoadSeries_ProgressChanged);
        }
        private void bgrwLoadStudy_Initialize()
        {
            bgrwLoadStudy = new System.ComponentModel.BackgroundWorker();

            bgrwLoadStudy.WorkerReportsProgress = true;
            bgrwLoadStudy.WorkerSupportsCancellation = true;
            bgrwLoadStudy.DoWork +=
                new DoWorkEventHandler(bgrwLoadStudy_DoWork);
            bgrwLoadStudy.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            bgrwLoadStudy_RunWorkerCompleted);
            bgrwLoadStudy.ProgressChanged +=
                new ProgressChangedEventHandler(
            bgrwLoadStudy_ProgressChanged);
        }        
        #endregion
        #region BackgroundWorkerRun
        private void bgrwExport_Run(string type, DirectoryInfo folder)
        {
            if (!bgrwExport.IsBusy)
            {
                onProcessDescriptionChanged?.Invoke("Export started...");

                onProgressInit?.Invoke(0, folder.GetDirectories().Count());

                ArgForBgrwExport arg = new ArgForBgrwExport();
                arg.type = type;
                arg.folder = folder;

                bgrwExport.RunWorkerAsync(arg);
            }
        }
        private void bgrwLoadSeries_Run(List<string> filePaths)
        {
            if (!bgrwLoadSeries.IsBusy)
            {
                this.CurrentDicomImageViewControl.Clear();
                onProcessDescriptionChanged?.Invoke("Load dcm-files started...");
                onProgressInit?.Invoke(0, filePaths.Count());
                bgrwLoadSeries.RunWorkerAsync(filePaths);
            }
        }
        private void bgrwLoadStudy_Run(string pathStudy)
        {
            if (!bgrwLoadStudy.IsBusy)
            {
                DirectoryInfo[] dirs = new DirectoryInfo(pathStudy).GetDirectories();
                
                this.LayoutManager.ChangeLayout(dirs.Count());               
               
                DirectoryInfo directoryinfo = new DirectoryInfo(pathStudy);

                onProcessDescriptionChanged?.Invoke("Load study started...");
                onProgressInit?.Invoke(0, directoryinfo.GetDirectories().Count());
                this.CurrentDicomImageViewControl.Clear();
                bgrwLoadStudy.RunWorkerAsync(pathStudy);
            }
        }
        public struct ArgForBgrwConvertSUR
        {
            public string path;
            public DicomTypes.RequiredMrtData data;
            public Dictionary<uint, string> additionalData;
        }
        #endregion

        public struct ArgForBgrwExport
        {
            public string type;
            public DirectoryInfo folder;
        }
        #region BackgroundWorkerDoWork
        private void bgrwExport_DoWork(object sender, DoWorkEventArgs e)
        {
            ArgForBgrwExport argForBgrwExport = (ArgForBgrwExport)e.Argument;
            BackgroundWorker worker = sender as BackgroundWorker;
            ExportManager = new ExportManager(argForBgrwExport.folder);
            string type = argForBgrwExport.type;

            if (type == "bitmap")
            {
                ExportManager.doBitmapExporting(worker);
            }
            if (type == "video")
            {
                ExportManager.doVideoExporting(worker);
            }
            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
        }
        private void bgrwLoadSeries_DoWork(object sender, DoWorkEventArgs e)
        {

            List<string> filePaths = (List<string>)e.Argument;

            int count = 0;
            foreach (string filePath in filePaths)
            {
                count++;
                DicomElement currentDicomElement = new DicomElement(filePath);
                this.CurrentDicomImageViewControl.DicomElements.Add(currentDicomElement);
                this.CurrentDicomImageViewControl.DicomElements.Sort();
                bgrwLoadSeries.ReportProgress(count);
            }

            this.CurrentDicomImageViewControl.CurrentDicomElement = this.CurrentDicomImageViewControl.DicomElements[0];
        }
        private void bgrwLoadStudy_DoWork(object sender, DoWorkEventArgs e)
        {
            DirectoryInfo[] dirs = new DirectoryInfo(e.Argument.ToString()).GetDirectories();

            int count = 0;

            foreach (DirectoryInfo di in dirs)
            {
                DicomImageViewControl control = (DicomImageViewControl)this.LayoutManager.layoutControls[count];

                List<string> filePaths = new List<string>();
                foreach (FileInfo file in di.GetFiles("*.dcm"))
                {
                    DicomElement currentDicomElement = new DicomElement(file.FullName);
                    control.DicomElements.Add(currentDicomElement);
                    control.DicomElements.Sort();
                    control.CurrentDicomElement = ((DicomImageViewControl)control).DicomElements[0];
                }
                control.LoadImage();

                bgrwLoadStudy.ReportProgress(count);
                count++;
            }

        }        
        #endregion
        #region BackgroundWorkerCompleted
        private void bgrwExport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                onProcessDescriptionChanged?.Invoke("Export canceled");
            }

            runWorkerCompleted(!e.Cancelled);
        }
        private void bgrwLoadSeries_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled)
            {
                onProcessDescriptionChanged?.Invoke("Load dcm-files canceled");
            }
            this.CurrentDicomImageViewControl.LoadImage();
            this.CurrentDicomImageViewControl.Focus();
            runWorkerCompleted(!e.Cancelled);

            onSeriesLoaded?.Invoke(CurrentDicomImageViewControl);
        }
        private void bgrwLoadStudy_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                onProcessDescriptionChanged?.Invoke("Load study canceled");
            }

            this.CurrentDicomImageViewControl.Focus();
            runWorkerCompleted(!e.Cancelled);

            onStudyLoaded?.Invoke();
        }
        private void runWorkerCompleted(bool completed)
        {
            if (completed)
            {
                onProcessDescriptionChanged?.Invoke("Ready");
            }


            this.Invalidate();
        }
        #endregion
        #region BackgroundWorkerProgressChanged
        private void bgrwExport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            onProcessDescriptionChanged?.Invoke("Processing... " + e.ProgressPercentage);
            onProgressValueChanged?.Invoke(e.ProgressPercentage);
        }
        private void bgrwLoadSeries_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            onProcessDescriptionChanged?.Invoke("Processing... " + e.ProgressPercentage);
            onProgressValueChanged?.Invoke(e.ProgressPercentage);
        }
        private void bgrwLoadStudy_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            onProcessDescriptionChanged?.Invoke("Processing... " + e.ProgressPercentage);
            onProgressValueChanged?.Invoke(e.ProgressPercentage);
        }
        private void bgrwConvertSUR_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            FileInfo file = (FileInfo)e.UserState;
            onProcessDescriptionChanged?.Invoke("Processing... " + file.Name);
            onProgressValueChanged?.Invoke(e.ProgressPercentage);
        }

        #endregion
        #region BackgroundHelpers

        private int сountFiles(DirectoryInfo directory, int cnt)
        {
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                cnt += сountFiles(dir, 0);
            }
            cnt += directory.GetFiles().Count();
            return cnt;

        }
        #endregion
        #endregion

        /// <summary>
        /// Текущий выбранный элемент в сетке
        /// </summary>
        public DicomImageViewControl CurrentDicomImageViewControl
        {
            get { return this.currentDicomImageViewControl; }
            set
            {
                if (this.currentDicomImageViewControl != null)
                {
                    if (this.currentDicomImageViewControl != value)
                    {
                        this.currentDicomImageViewControl.DisableSelecting();
                        this.currentDicomImageViewControl.Selected = false;
                        this.currentDicomImageViewControl.Stop();
                        this.currentDicomImageViewControl.Refresh();

                        this.currentDicomImageViewControl = value;
                        this.currentDicomImageViewControl.Selected = true;
                        this.Invalidate();
                    }
                }else
                {
                    this.currentDicomImageViewControl = value;
                    this.currentDicomImageViewControl.Selected = true;
                    this.Invalidate();
                }
            }
        }
       

        public void ExportCurrent(string fileName, ImageFormat format)
        {
            this.CurrentDicomImageViewControl.ExportCurrent(fileName, format);
        }
        public void ExportStudyToBitmap(string path)
        {
            DirectoryInfo folder = new DirectoryInfo(path);
            bgrwExport_Run("bitmap", folder);
        }
        public void ExportStudyToVideo(string path)
        {
            DirectoryInfo folder = new DirectoryInfo(path);
            bgrwExport_Run("video", folder);
        }
        public void StopExporting()
        {
            bgrwExport.CancelAsync();
        }
        public void ShowProperties()
        {
            CustomClass properties = new CustomClass();
            
            IEnumerable<FieldInfo> fields = typeof(DicomTags).GetFields().OrderBy(field => field.Name);
            int nonZeroFieldCount = 0;
            Console.WriteLine(CurrentDicomImageViewControl.Name);
            foreach (FieldInfo fi in fields)
            {
                String propertyValue = this.CurrentDicomImageViewControl.GetPropertyValue(fi);

                if (propertyValue != null && propertyValue.Length != 0)
                {
                    nonZeroFieldCount++;
                    properties.Add(new CustomProperty(fi.Name, propertyValue, true, true));
                }
            }

            if (CommonModule.TEST_MODE)
            {
                FDicomProperties dicomPropertiesFormNotModal = new FDicomProperties();
                dicomPropertiesFormNotModal.setMyProperties(properties);
                dicomPropertiesFormNotModal.Show();
            }
            else
            {
                dicomPropertiesForm.setMyProperties(properties);
                dicomPropertiesForm.ShowDialog();
            }
        }

        /// <summary>
        /// Включение/выключение инструментов
        /// </summary>
        /// <param name="toolName">Имя инструмента</param>
        public void ToolSwitchOn(string toolName = "freehand")
        {

            if (toolName == "layout")
            {
                FLayout dicomLayout = new FLayout(this.LayoutManager);
                dicomLayout.ShowDialog(this.ContainerControl);
                return;
            }
            if (toolName == "play")
            {
                this.CurrentDicomImageViewControl.Play();
                return;
            }
            if (toolName == "stop")
            {
                this.CurrentDicomImageViewControl.Stop();
                return;
            }
            if (toolName == "reset")
            {
                this.CurrentDicomImageViewControl.ClearManagers();
                this.CurrentDicomImageViewControl.ResetImage();
                return;
            }
            if (toolName == "localize")
            {
                LocalizeManager.CreateLocalize(this.CurrentDicomImageViewControl);
            }

            foreach (DicomImageViewControl control in this.LayoutManager.layoutControls)
            {
                control.FreehandToolStateChanged(false);
                control.TextToolStateChanged(false);
                control.ArrowToolStateChanged(false);
                control.SelectToolStateChanged(false);
                control.RulerToolStateChanged(false);
                control.ZoomToolStateChanged(false);
                control.OffsetToolStateChanged(false);
                control.BrightnessToolStateChanged(false);
                control.LocalizeToolStateChanged(false);

                if (toolName == "none")
                {
                    control.Refresh();
                    return;
                }
                if (toolName == "freehand")
                {
                    control.FreehandToolStateChanged(true);
                }
                if (toolName == "text")
                {
                    control.TextToolStateChanged(true);
                }
                if (toolName == "arrow")
                {
                    control.ArrowToolStateChanged(true);
                }
                if (toolName == "select")
                {
                    control.SelectToolStateChanged(true);
                }
                if (toolName == "ruler")
                {
                    control.RulerToolStateChanged(true);
                }
                if (toolName == "zoom")
                {
                    control.ZoomToolStateChanged(true);
                }
                if (toolName == "offset")
                {
                    control.OffsetToolStateChanged(true);
                }
                if (toolName == "brightness")
                {
                    control.BrightnessToolStateChanged(true);
                }

                control.Refresh();
            }

        }

        #region Test
        public void TestOn()
        {
            CommonModule.TEST_MODE = true;
        }
        public void TestOff()
        {
            CommonModule.TEST_MODE = false;
        }
        #endregion
    }
}

