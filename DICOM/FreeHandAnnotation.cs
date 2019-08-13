//Copyright © 2018 studio451.ru
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//E-mail: info@studio451.ru
//URL: https://studio451.ru/dicomimageviewer

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DicomImageViewer.Dicom
{
    class FreeHandAnnotation : Annotation
    {

        List<Point> points = new List<Point>();

        private bool closed = false;

        public FreeHandAnnotation()
        {
            this.Width = 1;
        }

        public int Width;

        public override void Draw(Graphics g, convertToDestinationDelegate convertToDestination, double PixelScale)
        {

            path = new GraphicsPath();
            path.StartFigure();

            if (points.Count > 2)
            {
                this.StartPoint = points[0];
                for (int i = 0; i < points.Count - 1; i++)
                {
                    path.AddLine(convertToDestination(points[i]), convertToDestination(points[i + 1]));
                }
            }
            if (closed)
            {
                path.CloseFigure();
            }

            g.DrawPath(new Pen(new SolidBrush(this.Color), this.Width), path);
        }

        public void Close()
        {
            closed = true;
        }

        public void AddPoint(int x, int y)
        {
            points.Add(new Point(x, y));

        }

        public override void MouseMove(Point point, Point offset)
        {

            if (points.Count > 2)
            {
                Point offsetEnd = new Point();
                offsetEnd.X = this.StartPoint.X - point.X - offset.X;
                offsetEnd.Y = this.StartPoint.Y - point.Y - offset.Y;

                this.StartPoint.X = point.X + offset.X;
                this.StartPoint.Y = point.Y + offset.Y;

                points[0] = this.StartPoint;

                for (int i = 1; i < points.Count; i++)
                {
                    Point currentPoint = points[i];
                    currentPoint.X -= offsetEnd.X;
                    currentPoint.Y -= offsetEnd.Y;

                    points[i] = currentPoint;

                }
            }
        }

        public override void MouseUp(Point point)
        {
        }
    }
}
