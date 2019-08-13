//Copyright © 2018 studio451.ru
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//E-mail: info@studio451.ru
//URL: https://studio451.ru/dicomimageviewer

using ClearCanvas.Dicom;
using DicomImageViewer.Dicom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using DicomImageViewer.Properties;

namespace DicomImageViewer
{
    public partial class FTest : Form
    {
        public DicomImageViewer.Dicom.ImageViewerManager ImageViewerManager;

        private FAnnotateText annotateTextForm;

        public FTest()
        {
            InitializeComponent();
        }

        private void FTest_Load(object sender, EventArgs e)
        {
            annotateTextForm = new FAnnotateText();

            //Инициализация ImageViewerManager
            ImageViewerManager = new DicomImageViewer.Dicom.ImageViewerManager(this, this.layoutPanel);
            ImageViewerManager.onProcessDescriptionChanged += imageViewerManager_ProcessDescriptionChanged;
            ImageViewerManager.onSeriesLoaded += ImageViewerManager_onSeriesLoaded;
            ImageViewerManager.onStudyLoaded += ImageViewerManager_onStudyLoaded; 
            //Конец инициализации

            toolStripPenWidth.Text = ImageViewerManager.CurrentBrushSize.ToString();
        }

        private void ImageViewerManager_onStudyLoaded()
        {
            generateTreeViewNodesForStudy();
        }

        private void ImageViewerManager_onSeriesLoaded(DicomImageViewControl currentDicomImageViewControl)
        {
            generateTreeViewNodesForStudy();
        }

        private void generateTreeViewNodesForStudy()
        {
            treeViewDicomStudies.Nodes.Clear();
            foreach (DicomImageViewControl dicomImageViewControl in ImageViewerManager.LoadedDicomImageViewControls)
            {
                TreeNode seriesNode = new TreeNode();

                seriesNode.Text = dicomImageViewControl.CurrentDicomElement.ImageSop.SeriesDescription;

                seriesNode.Tag = dicomImageViewControl.FilePaths;

                imageListDicomStudies.Images.Add(dicomImageViewControl.CurrentDicomElement.Bitmap);
                seriesNode.ImageIndex = imageListDicomStudies.Images.Count - 1;
                seriesNode.SelectedImageIndex = imageListDicomStudies.Images.Count - 1;
                treeViewDicomStudies.Nodes.Add(seriesNode);
            }            
        }

        public void imageViewerManager_ProcessDescriptionChanged(string text)
        {
            this.labelStatus.Text = text;
        }

        private void OpenStudyDcmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeViewDicomStudies.Nodes.Clear();
            folderBrowserDialogLoadStudy.Description = "Select study folder";
            if (folderBrowserDialogLoadStudy.ShowDialog() == DialogResult.OK)
            {
                ImageViewerManager.LoadStudy(folderBrowserDialogLoadStudy.SelectedPath);               
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(1);
        }

        private void showPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CustomClass properties = new CustomClass();


            IEnumerable<FieldInfo> fields = typeof(DicomTags).GetFields();
            int nonZeroFieldCount = 0;
            foreach (FieldInfo fi in fields)
            {
                String propertyValue = ImageViewerManager.CurrentDicomImageViewControl.GetPropertyValue(fi);

                if (propertyValue != null && propertyValue.Length != 0)
                {
                    nonZeroFieldCount++;
                    properties.Add(new CustomProperty(fi.Name, propertyValue, true, true));
                }
            }
            
            FDicomProperties dicomPropertiesFormNotModal = new FDicomProperties();
            dicomPropertiesFormNotModal.setMyProperties(properties);
            dicomPropertiesFormNotModal.Show();
        }

        private void exportCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(Settings.Default.ExportPath);
            if (!di.Exists)
            {
                di.Create();
            }
            ImageViewerManager.ExportCurrent(di.FullName + "\\" + Guid.NewGuid().ToString() + ".jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        private void exportBitmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialogLoadStudy.Description = "Select study folder";
            if (folderBrowserDialogLoadStudy.ShowDialog() == DialogResult.OK)
            {
                ImageViewerManager.ExportStudyToBitmap(folderBrowserDialogLoadStudy.SelectedPath);
            }
        }

        private void exportVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialogLoadStudy.Description = "Select study folder";
            if (folderBrowserDialogLoadStudy.ShowDialog() == DialogResult.OK)
            {
                ImageViewerManager.ExportStudyToVideo(folderBrowserDialogLoadStudy.SelectedPath);
            }
        }

        private void stopExportingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageViewerManager.StopExporting();
        }

        private void toolStripButtonNone_Click(object sender, EventArgs e)
        {
            ImageViewerManager.ToolSwitchOn("none");
        }
        private void toolStripButtonOffset_Click(object sender, EventArgs e)
        {
            ImageViewerManager.ToolSwitchOn("offset");
        }

        private void toolStripButtonArrow_Click(object sender, EventArgs e)
        {
            ImageViewerManager.ToolSwitchOn("arrow");
        }

        private void toolStripButtonFreehand_Click(object sender, EventArgs e)
        {
            ImageViewerManager.ToolSwitchOn("freehand");
        }

        private void toolStripButtonText_Click(object sender, EventArgs e)
        {
            ImageViewerManager.ToolSwitchOn("text");
        }

        private void toolStripButtonSelect_Click(object sender, EventArgs e)
        {
            ImageViewerManager.ToolSwitchOn("select");
        }

        private void toolStripButtonRuler_Click(object sender, EventArgs e)
        {
            ImageViewerManager.ToolSwitchOn("ruler");
        }

        private void toolStripButtonZoom_Click(object sender, EventArgs e)
        {
            ImageViewerManager.ToolSwitchOn("zoom");
        }

        private void toolStripButtonLayout_Click(object sender, EventArgs e)
        {
            ImageViewerManager.ToolSwitchOn("layout");
        }

        private void toolStripButtonShowProperties_Click(object sender, EventArgs e)
        {
            CustomClass properties = new CustomClass();

            IEnumerable<FieldInfo> fields = typeof(DicomTags).GetFields();
            int nonZeroFieldCount = 0;
            foreach (FieldInfo fi in fields)
            {
                String propertyValue = ImageViewerManager.CurrentDicomImageViewControl.GetPropertyValue(fi);

                if (propertyValue != null && propertyValue.Length != 0)
                {
                    nonZeroFieldCount++;
                    properties.Add(new CustomProperty(fi.Name, propertyValue, true, true));
                }
            }

            FDicomProperties dicomPropertiesFormNotModal = new FDicomProperties();
            dicomPropertiesFormNotModal.setMyProperties(properties);
            dicomPropertiesFormNotModal.Show();
        }

        private void toolStripButtonPlay_Click(object sender, EventArgs e)
        {
            ImageViewerManager.ToolSwitchOn("play");
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            ImageViewerManager.ToolSwitchOn("stop");
        }

        private void toolStripButtonOpenStudyDcm_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialogLoadStudy.ShowDialog() == DialogResult.OK)
            {
                ImageViewerManager.LoadStudy(folderBrowserDialogLoadStudy.SelectedPath);
            }
        }

        private void toolStripButtonPickColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = ImageViewerManager.CurrentColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                ImageViewerManager.CurrentColor = colorDialog1.Color;
            }
        }

        private void toolStripPenWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = this.toolStripPenWidth.SelectedIndex;
            ImageViewerManager.CurrentBrushSize = int.Parse(this.toolStripPenWidth.Items[selectedIndex] as string);
        }

        private void toolStripButtonAddLocalize_Click(object sender, EventArgs e)
        {
            ImageViewerManager.ToolSwitchOn("localize");
        }

        private void toolStripTextBoxSliceNumber_KeyUp(object sender, KeyEventArgs e)
        {
            ImageViewerManager.LocalizeManager.DefaultSliceNumber = int.Parse(toolStripTextBoxSliceNumber.Text);
        }

        private void toolStripTextBoxSliceWidth_KeyUp(object sender, KeyEventArgs e)
        {
            ImageViewerManager.LocalizeManager.DefaultSliceWidth = int.Parse(toolStripTextBoxSliceWidth.Text);
        }
        private void toolStripTextBoxSliceSize_KeyUp(object sender, KeyEventArgs e)
        {
            ImageViewerManager.LocalizeManager.DefaultSliceSize = int.Parse(toolStripTextBoxSliceSize.Text);
        }
        private void toolStripButtonFilterManager_Click(object sender, EventArgs e)
        {
            FFilterManager dicomLayout = new FFilterManager(ImageViewerManager.CurrentDicomImageViewControl.FilterManager);
            dicomLayout.ShowDialog(this);
        }

        private void toolStripButtonReset_Click(object sender, EventArgs e)
        {
            ImageViewerManager.CurrentDicomImageViewControl.ClearManagers();
            ImageViewerManager.CurrentDicomImageViewControl.ResetImage();
        }

        private void toolStripButtonClearLocalizes_Click(object sender, EventArgs e)
        {
            ImageViewerManager.LocalizeManager.Clear();
        }

        private void loadSeriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialogLoadSeries.Description = "Select series folder";
            if (folderBrowserDialogLoadSeries.ShowDialog() == DialogResult.OK)
            {
                ImageViewerManager.LoadSeries(folderBrowserDialogLoadSeries.SelectedPath);
            }
        }

        private void loadDcmFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogLoadDcmFiles.ShowDialog() == DialogResult.OK)
            {
                ImageViewerManager.LoadDcmFiles(openFileDialogLoadDcmFiles.FileNames.ToList<string>());
            }
        }

        private void loadDcmFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogLoadDcmFile.ShowDialog() == DialogResult.OK)
            {
                ImageViewerManager.LoadDcmFile(openFileDialogLoadDcmFile.FileName);
            }
        }

        private void treeViewDicomStudies_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                treeViewDicomStudies.SelectedNode = treeViewDicomStudies.GetNodeAt(e.X, e.Y);

                if (treeViewDicomStudies.SelectedNode != null)
                {
                    ImageViewerManager.LoadDcmFiles((List<string>)treeViewDicomStudies.SelectedNode.Tag);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FAbout fFAbout = new FAbout();
            fFAbout.Show();
        }
    }
}

