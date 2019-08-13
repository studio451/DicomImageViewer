namespace DicomImageViewer
{
    partial class DicomImageViewControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DicomImageViewControl));
            this.invertButton = new DicomImageViewer.ButtonControl();
            this.centerButton = new DicomImageViewer.ButtonControl();
            this.dicomButton = new DicomImageViewer.ButtonControl();
            this.closeButton = new DicomImageViewer.ButtonControl();
            this.increaseButton = new DicomImageViewer.ButtonControl();
            this.SuspendLayout();
            // 
            // invertButton
            // 
            this.invertButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.invertButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(142)))), ((int)(((byte)(223)))));
            this.invertButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("invertButton.BackgroundImage")));
            this.invertButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.invertButton.ImageSwitchOnOne = ((System.Drawing.Image)(resources.GetObject("invertButton.ImageSwitchOnOne")));
            this.invertButton.ImageSwitchOnTwo = ((System.Drawing.Image)(resources.GetObject("invertButton.ImageSwitchOnTwo")));
            this.invertButton.Location = new System.Drawing.Point(171, 7);
            this.invertButton.Name = "invertButton";
            this.invertButton.Size = new System.Drawing.Size(13, 13);
            this.invertButton.Swap = true;
            this.invertButton.SwitchOn = false;
            this.invertButton.TabIndex = 5;
            this.invertButton.Type = DicomImageViewer.ButtonControlType.Switch;
            this.invertButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.invertButton_MouseDown);
            // 
            // centerButton
            // 
            this.centerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.centerButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(142)))), ((int)(((byte)(223)))));
            this.centerButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("centerButton.BackgroundImage")));
            this.centerButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.centerButton.ImageSwitchOnOne = ((System.Drawing.Image)(resources.GetObject("centerButton.ImageSwitchOnOne")));
            this.centerButton.ImageSwitchOnTwo = ((System.Drawing.Image)(resources.GetObject("centerButton.ImageSwitchOnTwo")));
            this.centerButton.Location = new System.Drawing.Point(191, 6);
            this.centerButton.Name = "centerButton";
            this.centerButton.Size = new System.Drawing.Size(14, 14);
            this.centerButton.Swap = true;
            this.centerButton.SwitchOn = false;
            this.centerButton.TabIndex = 6;
            this.centerButton.Type = DicomImageViewer.ButtonControlType.Simple;
            this.centerButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.centerButton_MouseDown);
            // 
            // dicomButton
            // 
            this.dicomButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dicomButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(142)))), ((int)(((byte)(223)))));
            this.dicomButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("dicomButton.BackgroundImage")));
            this.dicomButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.dicomButton.ImageSwitchOnOne = ((System.Drawing.Image)(resources.GetObject("dicomButton.ImageSwitchOnOne")));
            this.dicomButton.ImageSwitchOnTwo = ((System.Drawing.Image)(resources.GetObject("dicomButton.ImageSwitchOnTwo")));
            this.dicomButton.Location = new System.Drawing.Point(212, 7);
            this.dicomButton.Name = "dicomButton";
            this.dicomButton.Size = new System.Drawing.Size(12, 12);
            this.dicomButton.Swap = true;
            this.dicomButton.SwitchOn = true;
            this.dicomButton.TabIndex = 7;
            this.dicomButton.Type = DicomImageViewer.ButtonControlType.Switch;
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(142)))), ((int)(((byte)(223)))));
            this.closeButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("closeButton.BackgroundImage")));
            this.closeButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.closeButton.ImageSwitchOnOne = ((System.Drawing.Image)(resources.GetObject("closeButton.ImageSwitchOnOne")));
            this.closeButton.ImageSwitchOnTwo = ((System.Drawing.Image)(resources.GetObject("closeButton.ImageSwitchOnTwo")));
            this.closeButton.Location = new System.Drawing.Point(231, 7);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(12, 12);
            this.closeButton.Swap = true;
            this.closeButton.SwitchOn = false;
            this.closeButton.TabIndex = 8;
            this.closeButton.Type = DicomImageViewer.ButtonControlType.Simple;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // increaseButton
            // 
            this.increaseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.increaseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(142)))), ((int)(((byte)(223)))));
            this.increaseButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("increaseButton.BackgroundImage")));
            this.increaseButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.increaseButton.ImageSwitchOnOne = ((System.Drawing.Image)(resources.GetObject("increaseButton.ImageSwitchOnOne")));
            this.increaseButton.ImageSwitchOnTwo = ((System.Drawing.Image)(resources.GetObject("increaseButton.ImageSwitchOnTwo")));
            this.increaseButton.Location = new System.Drawing.Point(225, 225);
            this.increaseButton.Name = "increaseButton";
            this.increaseButton.Size = new System.Drawing.Size(18, 18);
            this.increaseButton.Swap = false;
            this.increaseButton.SwitchOn = true;
            this.increaseButton.TabIndex = 9;
            this.increaseButton.Type = DicomImageViewer.ButtonControlType.Switch;
            this.increaseButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.increaseButton_MouseDown);
            // 
            // DicomImageViewControl
            // 
            this.AllowDrop = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.increaseButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.dicomButton);
            this.Controls.Add(this.centerButton);
            this.Controls.Add(this.invertButton);
            this.DoubleBuffered = true;
            this.Name = "DicomImageViewControl";
            this.Size = new System.Drawing.Size(250, 250);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.DicomImageView_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.DicomImageView_DragEnter);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.DicomImageView_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DicomImageView_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DicomImageView_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DicomImageView_MouseUp);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DicomImageView_MouseWheel);
            this.ResumeLayout(false);

        }



        #endregion

        private ButtonControl invertButton;
        private ButtonControl centerButton;
        private ButtonControl dicomButton;
        private ButtonControl closeButton;
        private ButtonControl increaseButton;
    }
}
