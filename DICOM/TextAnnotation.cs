//Copyright © 2018 studio451.ru
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//E-mail: info@studio451.ru
//URL: https://studio451.ru/dicomimageviewer

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DicomImageViewer.Dicom
{
    class TextAnnotation : Annotation
    {

        private string text;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        private Font font;

        public Font Font
        {
            get { return font; }
            set { font = value; }
        }

        public TextAnnotation(string text, Point point, Font font)
        {
            this.text = text;
            
            this.font = font;

            this.StartPoint.X = point.X;
            this.StartPoint.Y = point.Y;
        }

        public override void Draw(Graphics g, convertToDestinationDelegate convertToDestination,double PixelScale)
        {

            Point point = convertToDestination(StartPoint);

            TextRenderer.DrawText(g, text, font, point, Color);

            path = new GraphicsPath();
            path.StartFigure();
            path.AddRectangle(new Rectangle(point, TextRenderer.MeasureText(text, font)));
            path.CloseFigure();
        }

        public override void MouseMove(Point point, Point offset)
        {
            this.StartPoint.X = point.X + offset.X;
            this.StartPoint.Y = point.Y + offset.Y;
        }

        public override void MouseUp(Point point)
        {

        }

        public override string ToString()
        {
            return this.Text;
        }
    }
}
