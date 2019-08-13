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

namespace DicomImageViewer
{
    public class CommonModule
    {
        public static bool TEST_MODE = false;


        public static Color colorSelectedElement = Color.FromArgb(255, 255, 128);
        public static Color colorUnselectedElement = Color.FromArgb(240, 90, 90);

        public static void DrawPoint(Graphics g, Point p, Brush b)
        {
            g.DrawLine(new Pen(b), new Point(p.X - 10, p.Y), new Point(p.X + 10, p.Y));
            g.DrawLine(new Pen(b), new Point(p.X, p.Y - 10), new Point(p.X, p.Y + 10));
        }
        public static void DrawPoint(Graphics g, Point p)
        {
            g.DrawLine(Pens.Yellow, new Point(p.X - 10, p.Y), new Point(p.X + 10, p.Y));
            g.DrawLine(Pens.Yellow, new Point(p.X, p.Y - 10), new Point(p.X, p.Y + 10));
            g.DrawString(p.X.ToString() + ", " + p.Y.ToString(), new Font("Tahoma", 8), Brushes.Yellow, p);
        }
        public static void DrawText(Graphics g, Point p, String text)
        {
            g.DrawString(text, new Font("Tahoma", 8), Brushes.Yellow, p);
        }

        public static string Translit(string text)
        {
            string[] rusArray = new string[67] {"а","б","в","г","д","е","ё","ж","з","и","й","к","л", 
                "м","н","о","п","р","с","т","у","ф","х","ц","ч","ш","щ","ъ","ы","ь","э","ю","я","_",
                "А","Б","В","Г","Д","Е","Ё","Ж","З","И","Й","К","Л", 
                "М","Н","О","П","Р","С","Т","У","Ф","Х","Ц","Ч","Ш","Щ","Ъ","Ы","Ь","Э","Ю","Я"};

            string[] engArray = new string[67] {"a","b","v","g","d","e","e","zh","z","i","i","k","l",
                "m","n","o","p","r","s","t","u","f","kh","ts","ch","sh","shch","","y","","e","yu","ya","_",
                "A","B","V","G","D","E","E","Zh","Z","I","I","K","L",
                "M","N","O","P","R","S","T","U","F","Kh","Ts","Ch","Sh","Shch","","Y","","E","Yu","Ya"};

            string[] textArray = new string[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                textArray[i] = text[i].ToString();
            }
            string str = "";

            for (int j = 0; j < textArray.Length; j++)
            {
                for (int i = 0; i < rusArray.Length; i++)
                {
                    if (textArray[j] == rusArray[i]) str += engArray[i];
                }
            }
            return str;
        }
    }
}
