//Copyright © 2018 studio451.ru
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//E-mail: info@studio451.ru
//URL: https://studio451.ru/dicomimageviewer

using System.Collections.Generic;
using System.Windows.Forms;

namespace DicomImageViewer.Dicom
{

    public delegate LayoutControl CreateLayoutControl();

    public class LayoutManager
    {
        public ImageViewerManager ImageViewerManager;

        public int Offset = 0;

        public int maxGridX = 5;
        public int maxGridY = 5;

        int padding = 0;
        int margin = 1;

        int gridX = 2;
        int gridY = 2;

        public int oldGridX = 2;
        public int oldGridY = 2;

        float gridW = 0;
        float gridH = 0;

        public int GridX
        {
            set
            {
                oldGridX = gridX;
                gridX = value;
            }
            get { return gridX; }
        }

        public int GridY
        {
            set
            {
                oldGridY = gridY;
                gridY = value;
            }
            get { return gridY; }
        }

        public int GridCount
        {
            get { return gridX*gridY; }
        }

        public List<LayoutControl> layoutControls;
        public Control container;

        private CreateLayoutControl createLayoutControl;

        public LayoutManager(Control container, CreateLayoutControl createLayoutControl, ImageViewerManager imageViewerManager)
        {
            this.ImageViewerManager = imageViewerManager;

            this.layoutControls = new List<LayoutControl>();

            this.container = container;

            this.createLayoutControl = createLayoutControl;
        }

        public LayoutControl GetLayoutControl(int gridX, int gridY)
        {
            foreach (LayoutControl control in this.layoutControls)
            {
                if (control.GridX == gridX && control.GridY == gridY)
                {
                    return control;
                }
            }
            return null;
        }

        /// <summary>
        /// Изменяет сетку раскладки
        /// </summary>
        /// <param name="max">
        /// 0 - исходя из текущих GridX и GridY
        /// от 0 до 1 сетка 1х1
        /// от 1 до 4 сетка 2х2
        /// от 4 до 9 сетка 3х3
        /// от 9 до 16 сетка 4х4
        /// от 16 сетка 5х5
        /// </param>
        public void ChangeLayout(int max = 0)
        {
            if (max > 0 && max <= 1) { this.GridX = 1; this.GridY = 1;}
            if (max > 1 && max <= 4) { this.GridX = 2; this.GridY = 2;}
            if (max > 4 && max <= 9) { this.GridX = 3; this.GridY = 3;}
            if (max > 9 && max <= 16) { this.GridX = 4; this.GridY = 4;}
            if (max > 16) { this.GridX = 5; this.GridY = 5; }

            if (this.GridX != 1 && this.GridY != 1)
            {
                this.Offset = 0;
            }
            this.container.SuspendLayout();
            this.container.Controls.Clear();


            int gX = 1;
            int gY = 1;

            for (int i = 0; i < this.GridX * this.GridY; i++)
            {
                if (this.layoutControls.Count <= i)
                {
                    this.layoutControls.Add(this.createLayoutControl());
                }

                this.layoutControls[i + Offset].GridX = gX;
                this.layoutControls[i + Offset].GridY = gY;
                this.layoutControls[i + Offset].Offset = i;

                gX++;

                if (gX > GridX)
                {
                    gX = 1;
                    gY++;
                }

                this.container.Controls.Add(layoutControls[i + Offset]);
                ((DicomImageViewControl)layoutControls[i + Offset]).SwitchIncreaseButton();
            }
            this.container.ResumeLayout();


            Resize();
        }

        /// <summary>
        /// Изменение размеров Layout
        /// </summary>
        public void Resize()
        {
            this.gridW = (float)(this.container.Width - 2 * padding) / (float)this.GridX - 2 * margin;
            this.gridH = (float)(this.container.Height - 2 * padding) / (float)this.GridY - 2 * margin;

            int gX = 1;
            int gY = 1;

            for (int i = 0; i < this.GridX * this.GridY; i++)
            {
                this.layoutControls[i + Offset].Width = (int)this.gridW;
                this.layoutControls[i + Offset].Height = (int)this.gridH;

                this.layoutControls[i + Offset].Left = (int)(this.gridW * (gX - 1) + margin * (2 * gX - 1) + padding);
                this.layoutControls[i + Offset].Top = (int)(this.gridH * (gY - 1) + margin * (2 * gY - 1) + padding);

                gX++;

                if (gX > GridX)
                {
                    gX = 1;
                    gY++;
                }
                this.layoutControls[i + Offset].Invalidate();
            }
        }

        public void KeyDown(KeyEventArgs e)
        {
            foreach (LayoutControl control in this.layoutControls)
            {
                if (control.Selected)
                {
                    control.KeyDown(e);
                }
            }
        }
    }
}
