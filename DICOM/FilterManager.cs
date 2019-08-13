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
using System.Windows.Forms;
using System.Drawing.Imaging;
using AForge.Imaging.Filters;

namespace DicomImageViewer.Dicom
{
    public class FilterManager
    {
        private Control control = null;
        private Dictionary<string, string> filtersNameList = new Dictionary<string, string>();
        private System.Collections.Specialized.OrderedDictionary filters = new System.Collections.Specialized.OrderedDictionary();

        public FilterManager(Control control)
        {
            this.control = control;

            filtersNameList.Add("Invert", "Invert");
            filtersNameList.Add("Brightness", "Brightness");
            filtersNameList.Add("Contrast", "Contrast");
            filtersNameList.Add("Sharpen", "Sharpen");
            filtersNameList.Add("Blur", "Blur");
            filtersNameList.Add("Edges", "Edges");
            filtersNameList.Add("GaussianBlur", "GaussianBlur");
            filtersNameList.Add("GaussianSharpen", "GaussianSharpen");
            filtersNameList.Add("Mean", "Mean");
            filtersNameList.Add("Median", "Median");
            filtersNameList.Add("Mirror", "Mirror");
            filtersNameList.Add("Pixellate", "Pixellate");
            filtersNameList.Add("Morph", "Morph");
            filtersNameList.Add("HomogenityEdgeDetector", "HomogenityEdgeDetector");
            filtersNameList.Add("CannyEdgeDetector", "CannyEdgeDetector");
            filtersNameList.Add("SobelEdgeDetector", "SobelEdgeDetector");
            filtersNameList.Add("StereoAnaglyph", "StereoAnaglyph");
            filtersNameList.Add("Threshold", "Threshold");
        }

        /// <summary>
        /// Установить значение фильтра
        /// </summary>
        /// <param name="name">Имя фильтра</param>
        /// <param name="value">Значение для инициализации</param>
        /// <returns>Возвращает фильтр</returns>
        public static IInPlaceFilter SetFilter(string name, double value)
        {
            if (name == "Invert") { return new Invert(); }
            if (name == "Brightness") { return new BrightnessCorrection(value); }
            if (name == "Contrast") { return new ContrastCorrection(value); }
            if (name == "Sharpen") { return new Sharpen(); }
            if (name == "Blur") { return new Blur(); }
            if (name == "Edges") { return new Edges(); }
            if (name == "GaussianBlur") { return new GaussianBlur(); }
            if (name == "GaussianSharpen") { return new GaussianSharpen(); }
            if (name == "Mean") { return new Mean(); }
            if (name == "Median") { return new Median((int)value); }
            if (name == "Mirror")
            {
                if ((int)value == 1)
                    return new Mirror(true, false);
                if ((int)value == 2)
                    return new Mirror(true, true);
                if ((int)value == 3)
                    return new Mirror(false, true);
                return new Mirror(false, false);
            }
            if (name == "Pixellate") { return new Pixellate((int)value); }
            if (name == "CannyEdgeDetector") { return new CannyEdgeDetector(); }
            if (name == "HomogenityEdgeDetector") { return new HomogenityEdgeDetector(); }
            if (name == "Morph") { return new Morph(); }
            if (name == "SobelEdgeDetector") { return new SobelEdgeDetector(); }
            if (name == "StereoAnaglyph") { return new StereoAnaglyph(); }
            if (name == "Threshold") { return new Threshold(); }

            return null;
        }

        /// <summary>
        /// Добавить фильтр
        /// </summary>
        /// <param name="name">Имя фильтра</param>
        /// <param name="value">Значение для инициализации</param>
        public void AddFilter(string name, double value = 0)
        {
            if (!filtersNameList.ContainsKey(name))
            {
                throw new Exception("Filter not found!");
            }

            string id = Guid.NewGuid().ToString();
            filters[id] = new Filter(this, id, name, value, FilterManager.SetFilter(name, value));
            Invalidate();
        }

        /// <summary>
        /// Получить фильтр с заданным id
        /// </summary>
        /// <param name="id">id фильтра</param>
        /// <returns>Возвращает фильтр</returns>
        public Filter GetFilter(string id)
        {
            return (Filter)filters[id];
        }

        /// <summary>
        /// Удалить фильтр с заданным id
        /// </summary>
        /// <param name="id">id фильтра</param>
        public void DeleteFilter(string id)
        {
            filters.Remove(id);
            Invalidate();
        }

        /// <summary>
        /// Удалить все фильтры с заданным именем
        /// </summary>
        /// <param name="name">Имя фильтра</param>
        public void DeleteAllFiltersByName(string name)
        {
            foreach (System.Collections.DictionaryEntry f in filters)
            {
                if (((Filter)f.Value).Name == name)
                {
                    filters.Remove(f.Key);
                }
            }
            Invalidate();
        }

        /// <summary>
        /// Удалить все фильтры
        /// </summary>
        public void DeleteAllFilters()
        {
            filters.Clear();
            Invalidate();
        }

        /// <summary>
        /// Переместить фильтр с заданным id  в коллекции в указанную позицию
        /// </summary>
        /// <param name="id">id фильтра</param>
        /// <param name="index">Позиция в списке</param>
        public void MoveFilter(string id, int index)
        {
            if (index >= 0 && index < filters.Values.Count)
            {
                Filter f = (Filter)filters[id];
                filters.Remove(id);
                filters.Insert(index, f.Id, f);
                Invalidate();
            }
        }

        /// <summary>
        /// Текущие фильтры
        /// </summary>
        public System.Collections.Specialized.OrderedDictionary Filters
        {
            get { return filters; }
        }

        /// <summary>
        /// Список названий всех доступных для применения фильтров
        /// </summary>
        public List<string> FiltersNameList
        {
            get
            {
                List<string> list = new List<string>();
                foreach (var item in filtersNameList)
                {
                    list.Add(item.Value);
                }
                return list;
            }
        }

        /// <summary>
        /// Последовательно применить фильтры к изображению
        /// </summary>
        /// <param name="resultBitmap">Bitmap с наложенными фильтрами</param>
        /// <param name="originBitmap">Исходный bitmap</param>
        internal void ApplyFilters(ref System.Drawing.Bitmap resultBitmap, System.Drawing.Bitmap originBitmap)
        {
            resultBitmap = new System.Drawing.Bitmap(originBitmap);
            if (filters.Count > 0)
            {
                if (resultBitmap.PixelFormat != PixelFormat.Format24bppRgb)
                {
                    resultBitmap = AForge.Imaging.Image.Clone(resultBitmap, PixelFormat.Format24bppRgb);
                }

                foreach (System.Collections.DictionaryEntry item in filters)
                {
                    ((Filter)item.Value).ApplyFilter(ref resultBitmap);
                }
            }
        }

        public void Invalidate()
        {
            control.Invalidate();
        }

        /// <summary>
        /// Очистить коллекцию с фильтрами
        /// </summary>
        public void Clear()
        {
            filters.Clear();
        }
    }
}
