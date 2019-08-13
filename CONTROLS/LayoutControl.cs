using System;
using System.Windows.Forms;

namespace DicomImageViewer
{
    public class LayoutControl : UserControl
    {
        public int GridX;
        public int GridY;
        public int Offset;

        private Boolean selected;
        public Boolean Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        public virtual new void KeyDown(KeyEventArgs e)
        {
            return;
        }

    }
}
