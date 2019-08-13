//Copyright © 2018 studio451.ru
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//E-mail: info@studio451.ru
//URL: https://studio451.ru/dicomimageviewer

using System;
using System.Drawing;
using System.Windows.Forms;

namespace DicomImageViewer
{
    public partial class FLayout : Form
    {

        public DicomImageViewer.Dicom.LayoutManager layoutManager;

        public int GridX = 1;
        public int GridY = 1;

        public int width = 27;
        public int height = 27;

        public int currentGridX = 1;
        public int currentGridY = 1;

        public int padding = 0;

        public FLayout(DicomImageViewer.Dicom.LayoutManager layoutManager)
        {
            this.layoutManager = layoutManager;
            
            this.GridX = this.layoutManager.GridX;
            this.GridY = this.layoutManager.GridY;

            this.currentGridX = this.GridX;
            this.currentGridY = this.GridY;

            InitializeComponent();

            width = this.ClientRectangle.Width / this.layoutManager.maxGridX - 2;
            height = this.ClientRectangle.Height / this.layoutManager.maxGridY - 2;
        }

        private void dvLayout_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.FillRectangle(Brushes.Black, 0, 0, this.Width, this.Height);

                        
            for (int i = 0; i < this.layoutManager.maxGridX; i++)
            {
                for (int j = 0; j < this.layoutManager.maxGridY; j++)
                {
                    g.DrawRectangle(new Pen(layoutManager.ImageViewerManager.BrushUnselectedBorder, 1), i * (width + 3), j * (height + 3), width, height);
                }
            }
            for (int i = 0; i < currentGridX; i++)
            {
                for (int j = 0; j < currentGridY; j++)
                {
                    g.FillRectangle(layoutManager.ImageViewerManager.BrushUnselectedBorder, i * (width + 3), j * (height + 3), width, height);
                    g.DrawRectangle(new Pen(layoutManager.ImageViewerManager.BrushSelectedBorder, 1), i * (width + 3), j * (height + 3), width, height);
                }
            }

            Font textFont = new Font("Tahoma", 10);
            g.DrawString(this.currentGridX.ToString() + "x" + this.currentGridY.ToString(), textFont, layoutManager.ImageViewerManager.BrushSelectedFont, new Point(10, height/2 - 7));
        }

        private void dvLayout_MouseMove(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < this.layoutManager.maxGridX; i++)
            {
                for (int j = 0; j < this.layoutManager.maxGridY; j++)
                {
                    Rectangle r = new Rectangle(i * (width + 3), j * (height + 3), width, height);
                    if (r.Contains(new Point(e.X, e.Y)))
                    {
                        this.currentGridX = i + 1;
                        this.currentGridY = j + 1;
                    }
                }
            }
            this.Invalidate();
        }

        private void dvLayout_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < this.layoutManager.maxGridX; i++)
                {
                    for (int j = 0; j < this.layoutManager.maxGridY; j++)
                    {
                        Rectangle r = new Rectangle(i * (width + 3), j * (height + 3), width, height);
                        if (r.Contains(new Point(e.X, e.Y)))
                        {
                            this.layoutManager.GridX = i + 1;
                            this.layoutManager.GridY = j + 1;
                            this.layoutManager.ChangeLayout();
                            this.Close();
                            return;
                        }
                    }
                }
            }
        }

        private void dvLayout_Deactivate(object sender, EventArgs e)
        {
            this.Close();
        }



    }
}
