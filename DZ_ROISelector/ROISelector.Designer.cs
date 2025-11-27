namespace DZ_ROISelector
{
    partial class ROISelector
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ROISelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Name = "ROISelector";
            this.Size = new System.Drawing.Size(640, 480);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ROISelector_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ROISelector_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ROISelector_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ROISelector_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ROISelector_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
