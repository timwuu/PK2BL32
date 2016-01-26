namespace PICkit2V2
{
    partial class DialogUnitSelect
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
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSelectUnit = new System.Windows.Forms.Button();
            this.listBoxUnits = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(190, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Please select a PICkit 2 to use:";
            // 
            // buttonSelectUnit
            // 
            this.buttonSelectUnit.Enabled = false;
            this.buttonSelectUnit.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSelectUnit.Location = new System.Drawing.Point(74, 144);
            this.buttonSelectUnit.Name = "buttonSelectUnit";
            this.buttonSelectUnit.Size = new System.Drawing.Size(80, 26);
            this.buttonSelectUnit.TabIndex = 2;
            this.buttonSelectUnit.Text = "Select";
            this.buttonSelectUnit.UseVisualStyleBackColor = true;
            this.buttonSelectUnit.Click += new System.EventHandler(this.buttonSelectUnit_Click);
            // 
            // listBoxUnits
            // 
            this.listBoxUnits.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxUnits.FormattingEnabled = true;
            this.listBoxUnits.ItemHeight = 15;
            this.listBoxUnits.Location = new System.Drawing.Point(14, 64);
            this.listBoxUnits.Name = "listBoxUnits";
            this.listBoxUnits.Size = new System.Drawing.Size(199, 64);
            this.listBoxUnits.TabIndex = 4;
            this.listBoxUnits.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxUnits_MouseDoubleClick);
            this.listBoxUnits.SelectedIndexChanged += new System.EventHandler(this.listBoxUnits_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(11, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(122, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Unit#            UnitID";
            // 
            // DialogUnitSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(227, 193);
            this.ControlBox = false;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listBoxUnits);
            this.Controls.Add(this.buttonSelectUnit);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DialogUnitSelect";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select PICkit 2 Unit";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSelectUnit;
        private System.Windows.Forms.ListBox listBoxUnits;
        private System.Windows.Forms.Label label2;
    }
}