namespace DicomImageViewer
{
    partial class ButtonControl
    {
        /// <summary> 
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ButtonControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(142)))), ((int)(((byte)(223)))));
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DoubleBuffered = true;
            this.Name = "ButtonControl";
            this.Size = new System.Drawing.Size(14, 14);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ButtonControl_KeyDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ButtonControl_MouseUp);
            this.MouseEnter += new System.EventHandler(this.ButtonControl_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.ButtonControl_MouseLeave);
            this.ResumeLayout(false);

        }

        #endregion

    }
}
