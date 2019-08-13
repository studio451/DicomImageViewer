//Copyright © 2018 studio451.ru
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//E-mail: info@studio451.ru
//URL: https://studio451.ru/dicomimageviewer

using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace DicomImageViewer.Dicom
{
    public delegate Point convertToDestinationDelegate(Point point);
    public delegate Point convertToOriginDelegate(Point point);
    
    public class AnnotationManager
    {
        public Dictionary<int,HashSet<Annotation>> AnnotationsArray = new Dictionary<int,HashSet<Annotation>>();
        public HashSet<Annotation> Annotations = new HashSet<Annotation>();
        public event CollectionChanged AnnotationAdded;
        public event CollectionChanged AnnotationDeleted;
        public event CollectionCleared AnnotationCleared;

        protected convertToDestinationDelegate ConvertToDestination;
        protected convertToOriginDelegate ConvertToOrigin;

        public AnnotationManager(convertToDestinationDelegate ConvertToDestination,
        convertToOriginDelegate ConvertToOrigin)
        {
            this.ConvertToDestination = ConvertToDestination;
            this.ConvertToOrigin = ConvertToOrigin;
        }

        public bool flagMouseMove = false;

        public Point offset = new Point();

        public int number = 0;
        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        public void AddAnnotation(Annotation annotation)
        {
            if (annotation != null)
            {
                if (!AnnotationsArray.ContainsKey(Number))
                {
                    AnnotationsArray[Number] = new HashSet<Annotation>();
                }
                AnnotationsArray[Number].Add(annotation);                

                if (this.AnnotationAdded != null)
                    this.AnnotationAdded(annotation);
            }
        }

        public void RepaintAnnotations(Graphics g,double PixelScale)
        {
                if (AnnotationsArray.ContainsKey(Number))
                {
                    foreach (Annotation annotation in AnnotationsArray[Number])
                    {
                        annotation.Repaint(g, this.ConvertToDestination, PixelScale);
                    }
                }            
        }

        public void DeleteLastAnnotation()
        {
            if (AnnotationsArray.ContainsKey(Number))
            {
                Annotation toRemove = AnnotationsArray[Number].LastOrDefault();
                if (toRemove != null)
                {
                    AnnotationsArray[Number].Remove(toRemove);
                    if (this.AnnotationDeleted != null)
                        this.AnnotationDeleted(toRemove);
                }
            }
        }

        public Annotation GetLastAnnotation()
        {
            if (AnnotationsArray.ContainsKey(Number))
            {
                return AnnotationsArray[Number].LastOrDefault();
            }
            return null;
        }

        public Annotation GetSelectedAnnotation()
        {
            if (AnnotationsArray.ContainsKey(Number))
            {
                foreach (Annotation annotation in AnnotationsArray[Number])
                {
                    if (annotation.Selected)
                    {
                        return annotation;
                    }
                }
            }
            return null;
        }        

        public void Clear()
        {

                if (AnnotationsArray.ContainsKey(Number))
                {
                    AnnotationsArray[Number].Clear();
                    if (this.AnnotationCleared != null)
                        this.AnnotationCleared();
                }
            
        }

        internal void DeleteAnnotation(Annotation annotation, bool fromAllFrames = false)
        {
            if (fromAllFrames)
            {
                foreach (var item in AnnotationsArray)
                {
                    item.Value.Remove(annotation);
                }
            }
            else
            {
                if (AnnotationsArray.ContainsKey(Number))
                {
                    AnnotationsArray[Number].Remove(annotation);
                    if (this.AnnotationDeleted != null)
                        this.AnnotationDeleted(annotation);
                }
            }
        }        

        public delegate void CollectionChanged(Annotation annotation);
        public delegate void CollectionCleared();

        public void Select(Annotation annotationSelected)
        {
            if (AnnotationsArray.ContainsKey(Number))
            {
                foreach (Annotation annotation in AnnotationsArray[Number])
                {
                    if (annotationSelected == annotation)
                    {
                        annotation.Selected = true;
                    }
                    else
                    {
                        annotation.Selected = false;
                    }
                }
            }
        }
        public bool MouseDown(Point point)
        {   
                if (AnnotationsArray.ContainsKey(Number))
                {
                    foreach (Annotation annotation in AnnotationsArray[Number])
                    {
                        if (annotation.Contain(point))
                        {
                            this.Select(annotation);
                            this.flagMouseMove = true;

                            Point originPoint = this.ConvertToOrigin(point);

                            offset.X = annotation.StartPoint.X - originPoint.X;
                            offset.Y = annotation.StartPoint.Y - originPoint.Y;

                            return true;
                        }
                    }
                }
                return false;
        }
        public bool MouseMove(Point point)
        {
            
            if (this.flagMouseMove == true)
            {
                if (AnnotationsArray.ContainsKey(Number))
                {
                    foreach (Annotation annotation in AnnotationsArray[Number])
                    {
                        if (annotation.Selected)
                        {
                            annotation.MouseMove(this.ConvertToOrigin(point),offset);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool MouseUp(Point point)
        {
            if (this.flagMouseMove == true)
            {
                if (AnnotationsArray.ContainsKey(Number))
                {
                    foreach (Annotation annotation in AnnotationsArray[Number])
                    {
                        if (annotation.Selected)
                        {
                            annotation.MouseUp(this.ConvertToOrigin(point));
                        }
                    }
                }
            }
            this.flagMouseMove = false;

            return false;
        }

        public bool KeyDown(Keys key)
        {

            switch (key)
            {
                case Keys.Delete:
                    {
                        Annotation deletedAnnotation = this.GetSelectedAnnotation();
                        if (deletedAnnotation != null)
                        {
                                this.DeleteAnnotation(deletedAnnotation);
                        }
                        return true;
                    }
            }
           
            return false;
        }

    }
}
