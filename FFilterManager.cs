//Copyright © 2018 studio451.ru
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//E-mail: info@studio451.ru
//URL: https://studio451.ru/dicomimageviewer

using System;
using System.Windows.Forms;
using DicomImageViewer.Dicom;

namespace DicomImageViewer
{
    public partial class FFilterManager : Form
    {

        public DicomImageViewer.Dicom.FilterManager FilterManager;

        private void refreshLbFilters(int selectedIndex = -1)
        {
            //Получаем список примененных фильтров
            int currentIndex = lbFilters.SelectedIndex;

            if(selectedIndex != -1)
            {
                currentIndex = selectedIndex;
            }  

            lbFilters.Items.Clear();
            foreach (System.Collections.DictionaryEntry item in this.FilterManager.Filters)
            {
                lbFilters.Items.Add((Filter)item.Value);
            }


            if (currentIndex >= lbFilters.Items.Count)
            {
                currentIndex = lbFilters.Items.Count - 1;
            }
            if (currentIndex < -1)
            {
                currentIndex = -1;
            }

            if (lbFilters.Items.Count > 0)
            {                
                lbFilters.SelectedIndex = currentIndex;
            }
            else
            {
                tbSelectedFilterName.Tag = "";
                tbSelectedFilterName.Text = "";
                tbSelectedFilterValue.Text = "";
            }

        }
        public FFilterManager(DicomImageViewer.Dicom.FilterManager filterManager)
        {
            InitializeComponent();

            this.FilterManager = filterManager;

            //Получаем список существующих фильтров
            foreach (var item in this.FilterManager.FiltersNameList)
            {
                cbFiltersList.Items.Add(item);
            }
            if (cbFiltersList.Items.Count > 0)
            {
                cbFiltersList.SelectedIndex = 0;
            }
            refreshLbFilters();
        }

        private void bAddFilter_Click(object sender, EventArgs e)
        {
            //Добавляем фильтр и базовое значение
            FilterManager.AddFilter(cbFiltersList.Text, Convert.ToDouble(tbValue.Text));
            refreshLbFilters();
        }

        private void bDeleteAllFilters_Click(object sender, EventArgs e)
        {
            //Удалить все фильтры
            FilterManager.DeleteAllFilters();
            refreshLbFilters();
        }

        private void bDeleteFilter_Click(object sender, EventArgs e)
        {
            //Удаляем конкретный фильтр
            if (lbFilters.SelectedIndex != -1)
            {
                FilterManager.DeleteFilter(((Filter)lbFilters.Items[lbFilters.SelectedIndex]).Id);
                refreshLbFilters();
                
            }
        }

        private void lbFilters_SelectedIndexChanged(object sender, EventArgs e)
        {
            var filter = FilterManager.GetFilter(((Filter)lbFilters.Items[lbFilters.SelectedIndex]).Id);
            if(filter != null)
            {
                tbSelectedFilterName.Tag = filter.Id;
                tbSelectedFilterName.Text = filter.Name;
                tbSelectedFilterValue.Text = Convert.ToString(filter.Value);
            }

        }

        private void bSaveSelectedFilter_Click(object sender, EventArgs e)
        {
            var filter = FilterManager.GetFilter(tbSelectedFilterName.Tag.ToString());
            filter.Value = Convert.ToDouble(tbSelectedFilterValue.Text);
            refreshLbFilters();
        }

        private void bMoveUp_Click(object sender, EventArgs e)
        {
            //Перемещаем вверх фильтр
            if (lbFilters.SelectedIndex != -1)
            {
                FilterManager.MoveFilter(((Filter)lbFilters.Items[lbFilters.SelectedIndex]).Id, lbFilters.SelectedIndex - 1);
                refreshLbFilters(lbFilters.SelectedIndex - 1);
            }
        }

        private void bMoveDown_Click(object sender, EventArgs e)
        {
            //Перемещаем вниз фильтр
            //Перемещаем вверх фильтр
            if (lbFilters.SelectedIndex != -1)
            {
                FilterManager.MoveFilter(((Filter)lbFilters.Items[lbFilters.SelectedIndex]).Id, lbFilters.SelectedIndex + 1);
                refreshLbFilters(lbFilters.SelectedIndex + 1);
            }
        }
    }
}
