namespace SudokuGUI
{
    partial class SudokuCell
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
            this.cellTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // cellTextBox
            // 
            this.cellTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cellTextBox.Location = new System.Drawing.Point(0, 0);
            this.cellTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.cellTextBox.Multiline = true;
            this.cellTextBox.Name = "cellTextBox";
            this.cellTextBox.Size = new System.Drawing.Size(150, 150);
            this.cellTextBox.TabIndex = 0;
            this.cellTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // SudokuCell
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cellTextBox);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "SudokuCell";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox cellTextBox;
    }
}
