using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Annotator
{
    /// <summary>
    /// ROI 裁剪预览窗口
    /// </summary>
    public partial class RoiPreviewForm : Form
    {
        private List<Bitmap> _roiImages;
        private List<PictureBox> _pictureBoxes;
        private string _imageName;

        public RoiPreviewForm()
        {
            InitializeComponent();
            _roiImages = new List<Bitmap>();
            _pictureBoxes = new List<PictureBox>();
        }

        /// <summary>
        /// 显示 ROI 图像
        /// </summary>
        public void ShowRoiImages(List<Bitmap> images, string imageName)
        {
            _roiImages = images;
            _imageName = imageName;

            lblInfo.Text = $"当前图片: {imageName} | 共 {images.Count} 个 ROI";

            // 清空现有控件
            flowLayoutPanel.Controls.Clear();
            _pictureBoxes.Clear();

            // 为每个 ROI 创建显示面板
            for (int i = 0; i < images.Count; i++)
            {
                CreateRoiPanel(images[i], i);
            }
        }

        /// <summary>
        /// 创建单个 ROI 显示面板
        /// </summary>
        private void CreateRoiPanel(Bitmap image, int index)
        {
            // 创建容器面板
            Panel panel = new Panel
            {
                Width = 180,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5)
            };

            // 创建 PictureBox
            PictureBox picBox = new PictureBox
            {
                Width = 170,
                Height = 150,
                Left = 5,
                Top = 5,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = image,
                Cursor = Cursors.Hand,
                Tag = index
            };
            picBox.DoubleClick += PictureBox_DoubleClick;
            _pictureBoxes.Add(picBox);

            // 创建信息标签
            Label lblInfo = new Label
            {
                Width = 170,
                Height = 35,
                Left = 5,
                Top = 160,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = $"ROI {index + 1}\n{image.Width} x {image.Height}"
            };

            panel.Controls.Add(picBox);
            panel.Controls.Add(lblInfo);
            flowLayoutPanel.Controls.Add(panel);
        }

        /// <summary>
        /// 双击放大查看
        /// </summary>
        private void PictureBox_DoubleClick(object sender, EventArgs e)
        {
            PictureBox picBox = sender as PictureBox;
            if (picBox?.Image != null)
            {
                int index = (int)picBox.Tag;
                
                // 创建放大查看窗口
                Form zoomForm = new Form
                {
                    Text = $"ROI {index + 1} - 原始尺寸",
                    Width = Math.Min(picBox.Image.Width + 40, 800),
                    Height = Math.Min(picBox.Image.Height + 80, 600),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                PictureBox zoomPicBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = picBox.Image
                };

                zoomForm.Controls.Add(zoomPicBox);
                zoomForm.ShowDialog(this);
            }
        }

        /// <summary>
        /// 保存选中的 ROI
        /// </summary>
        private void btnSaveSelected_Click(object sender, EventArgs e)
        {
            // 获取选中的 PictureBox（这里简化为保存第一个）
            if (_pictureBoxes.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "PNG 图像|*.png|JPEG 图像|*.jpg|BMP 图像|*.bmp",
                    FileName = $"{Path.GetFileNameWithoutExtension(_imageName)}_ROI_1.png"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    _pictureBoxes[0].Image.Save(sfd.FileName);
                    MessageBox.Show("保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// 保存所有 ROI
        /// </summary>
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            if (_roiImages.Count == 0)
            {
                MessageBox.Show("没有可保存的 ROI 图像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                Description = "选择保存文件夹"
            };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string baseFileName = Path.GetFileNameWithoutExtension(_imageName);
                    
                    for (int i = 0; i < _roiImages.Count; i++)
                    {
                        string fileName = Path.Combine(fbd.SelectedPath, $"{baseFileName}_ROI_{i + 1}.png");
                        _roiImages[i].Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                    }

                    MessageBox.Show($"成功保存 {_roiImages.Count} 个 ROI 图像！", "提示", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败: {ex.Message}", "错误", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 窗口关闭时释放资源
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            
            // 注意：不要释放 _roiImages 中的图像，因为它们可能还在使用
            // 只清空引用
            _pictureBoxes.Clear();
        }
    }
}
