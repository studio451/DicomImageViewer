namespace DicomImageViewer
{
    partial class FFilterManager
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FFilterManager));
            this.cbFiltersList = new System.Windows.Forms.ComboBox();
            this.bAddFilter = new System.Windows.Forms.Button();
            this.lbFilters = new System.Windows.Forms.ListBox();
            this.bDeleteFilter = new System.Windows.Forms.Button();
            this.bDeleteAllFilters = new System.Windows.Forms.Button();
            this.tbValue = new System.Windows.Forms.TextBox();
            this.tbSelectedFilterValue = new System.Windows.Forms.TextBox();
            this.bSaveSelectedFilter = new System.Windows.Forms.Button();
            this.tbSelectedFilterName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.bMoveUp = new System.Windows.Forms.Button();
            this.bMoveDown = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbFiltersList
            // 
            this.cbFiltersList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbFiltersList.FormattingEnabled = true;
            this.cbFiltersList.Location = new System.Drawing.Point(9, 25);
            this.cbFiltersList.Name = "cbFiltersList";
            this.cbFiltersList.Size = new System.Drawing.Size(432, 21);
            this.cbFiltersList.TabIndex = 0;
            // 
            // bAddFilter
            // 
            this.bAddFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bAddFilter.Location = new System.Drawing.Point(510, 23);
            this.bAddFilter.Name = "bAddFilter";
            this.bAddFilter.Size = new System.Drawing.Size(81, 23);
            this.bAddFilter.TabIndex = 1;
            this.bAddFilter.Text = "Add";
            this.bAddFilter.UseVisualStyleBackColor = true;
            this.bAddFilter.Click += new System.EventHandler(this.bAddFilter_Click);
            // 
            // lbFilters
            // 
            this.lbFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbFilters.FormattingEnabled = true;
            this.lbFilters.HorizontalScrollbar = true;
            this.lbFilters.Location = new System.Drawing.Point(9, 136);
            this.lbFilters.Name = "lbFilters";
            this.lbFilters.Size = new System.Drawing.Size(490, 342);
            this.lbFilters.TabIndex = 2;
            this.lbFilters.SelectedIndexChanged += new System.EventHandler(this.lbFilters_SelectedIndexChanged);
            // 
            // bDeleteFilter
            // 
            this.bDeleteFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bDeleteFilter.Location = new System.Drawing.Point(510, 427);
            this.bDeleteFilter.Name = "bDeleteFilter";
            this.bDeleteFilter.Size = new System.Drawing.Size(81, 23);
            this.bDeleteFilter.TabIndex = 3;
            this.bDeleteFilter.Text = "Delete";
            this.bDeleteFilter.UseVisualStyleBackColor = true;
            this.bDeleteFilter.Click += new System.EventHandler(this.bDeleteFilter_Click);
            // 
            // bDeleteAllFilters
            // 
            this.bDeleteAllFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bDeleteAllFilters.Location = new System.Drawing.Point(510, 456);
            this.bDeleteAllFilters.Name = "bDeleteAllFilters";
            this.bDeleteAllFilters.Size = new System.Drawing.Size(81, 23);
            this.bDeleteAllFilters.TabIndex = 4;
            this.bDeleteAllFilters.Text = "Delete all";
            this.bDeleteAllFilters.UseVisualStyleBackColor = true;
            this.bDeleteAllFilters.Click += new System.EventHandler(this.bDeleteAllFilters_Click);
            // 
            // tbValue
            // 
            this.tbValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbValue.Location = new System.Drawing.Point(447, 25);
            this.tbValue.Name = "tbValue";
            this.tbValue.Size = new System.Drawing.Size(52, 20);
            this.tbValue.TabIndex = 5;
            this.tbValue.Text = "0,2";
            // 
            // tbSelectedFilterValue
            // 
            this.tbSelectedFilterValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSelectedFilterValue.Location = new System.Drawing.Point(447, 84);
            this.tbSelectedFilterValue.Name = "tbSelectedFilterValue";
            this.tbSelectedFilterValue.Size = new System.Drawing.Size(52, 20);
            this.tbSelectedFilterValue.TabIndex = 6;
            // 
            // bSaveSelectedFilter
            // 
            this.bSaveSelectedFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bSaveSelectedFilter.Location = new System.Drawing.Point(510, 82);
            this.bSaveSelectedFilter.Name = "bSaveSelectedFilter";
            this.bSaveSelectedFilter.Size = new System.Drawing.Size(81, 23);
            this.bSaveSelectedFilter.TabIndex = 7;
            this.bSaveSelectedFilter.Text = "Apply";
            this.bSaveSelectedFilter.UseVisualStyleBackColor = true;
            this.bSaveSelectedFilter.Click += new System.EventHandler(this.bSaveSelectedFilter_Click);
            // 
            // tbSelectedFilterName
            // 
            this.tbSelectedFilterName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSelectedFilterName.Location = new System.Drawing.Point(9, 84);
            this.tbSelectedFilterName.Name = "tbSelectedFilterName";
            this.tbSelectedFilterName.ReadOnly = true;
            this.tbSelectedFilterName.Size = new System.Drawing.Size(432, 20);
            this.tbSelectedFilterName.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Add filter:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Edit selected filter:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Filters:";
            // 
            // bMoveUp
            // 
            this.bMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bMoveUp.Location = new System.Drawing.Point(510, 135);
            this.bMoveUp.Name = "bMoveUp";
            this.bMoveUp.Size = new System.Drawing.Size(81, 23);
            this.bMoveUp.TabIndex = 12;
            this.bMoveUp.Text = "Move up";
            this.bMoveUp.UseVisualStyleBackColor = true;
            this.bMoveUp.Click += new System.EventHandler(this.bMoveUp_Click);
            // 
            // bMoveDown
            // 
            this.bMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bMoveDown.Location = new System.Drawing.Point(510, 164);
            this.bMoveDown.Name = "bMoveDown";
            this.bMoveDown.Size = new System.Drawing.Size(81, 23);
            this.bMoveDown.TabIndex = 13;
            this.bMoveDown.Text = "Move down";
            this.bMoveDown.UseVisualStyleBackColor = true;
            this.bMoveDown.Click += new System.EventHandler(this.bMoveDown_Click);
            // 
            // FFilterManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 493);
            this.Controls.Add(this.bMoveDown);
            this.Controls.Add(this.bMoveUp);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbSelectedFilterName);
            this.Controls.Add(this.bSaveSelectedFilter);
            this.Controls.Add(this.tbSelectedFilterValue);
            this.Controls.Add(this.tbValue);
            this.Controls.Add(this.bDeleteAllFilters);
            this.Controls.Add(this.bDeleteFilter);
            this.Controls.Add(this.lbFilters);
            this.Controls.Add(this.bAddFilter);
            this.Controls.Add(this.cbFiltersList);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(260, 300);
            this.Name = "FFilterManager";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Filters";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox cbFiltersList;
        private System.Windows.Forms.Button bAddFilter;
        private System.Windows.Forms.ListBox lbFilters;
        private System.Windows.Forms.Button bDeleteFilter;
        private System.Windows.Forms.Button bDeleteAllFilters;
        private System.Windows.Forms.TextBox tbValue;
        private System.Windows.Forms.TextBox tbSelectedFilterValue;
        private System.Windows.Forms.Button bSaveSelectedFilter;
        private System.Windows.Forms.TextBox tbSelectedFilterName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button bMoveUp;
        private System.Windows.Forms.Button bMoveDown;
    }
}