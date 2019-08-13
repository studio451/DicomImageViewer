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
    public enum ButtonControlType { Simple, Switch };

    public partial class ButtonControl : UserControl
    {
        private Boolean swap = true;
        public Boolean Swap
        {
            get { return swap; }
            set
            {
                swap = value;
            }
        }

        private ButtonControlType type;
        public ButtonControlType Type
        {
            get { return type; }
            set
            {
                type = value;
            }
        }

        private Image imageSwitchOnOne;
        public Image ImageSwitchOnOne
        {
            get { return imageSwitchOnOne; }
            set
            {
                imageSwitchOnOne = value;
                this.BackgroundImage = imageSwitchOnOne;
            }
        }
        private Image imageSwitchOnTwo;
        public Image ImageSwitchOnTwo
        {
            get { return imageSwitchOnTwo; }
            set
            {
                imageSwitchOnTwo = value;
                this.BackgroundImage = imageSwitchOnTwo;
            }
        }

        private Boolean switchOn = false;
        public Boolean SwitchOn
        {
            get { return switchOn; }
            set
            {
                switchOn = value;
                if (switchOn)
                {
                    this.BackgroundImage = imageSwitchOnOne;
                }
                else
                {
                    this.BackgroundImage = imageSwitchOnTwo;
                }
                if (this.Parent != null)
                    this.Parent.Refresh();

            }
        }

        public ButtonControl()
        {
            InitializeComponent();
        }

        private void ButtonControl_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;
        }

        private void ButtonControl_MouseEnter(object sender, EventArgs e)
        {
            if (swap)
            {
                if (type == ButtonControlType.Switch)
                {
                    if (switchOn == false)
                    {
                        this.BackgroundImage = imageSwitchOnOne;
                    }
                }
                else
                {
                    this.BackgroundImage = imageSwitchOnOne;
                }
            }
        }

        private void ButtonControl_MouseLeave(object sender, EventArgs e)
        {
            if (swap)
            {
                if (type == ButtonControlType.Switch)
                {
                    if (switchOn == false)
                    {
                        this.BackgroundImage = imageSwitchOnTwo;
                    }
                }
                else
                {
                    this.BackgroundImage = imageSwitchOnTwo;
                }
            }

        }

        private void ButtonControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (type == ButtonControlType.Switch)
            {
                if (SwitchOn)
                {
                    SwitchOn = false;
                }
                else
                {
                    SwitchOn = true;
                }
            }            
        }        
    }
}
