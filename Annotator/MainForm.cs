using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using DZ_ROISelector;

namespace Annotator
{
    public partial class MainForm : Form
    {
        #region Private Variables

        private string _imageFolder = null;
        private int _currenIndex = -1;

        private List<string> _extentions;
        private List<string> _files;

        private AnnotationList _annList;
        private ROISelector _roiSelector;

        #endregion Private Variables

        #region Constructor

        public MainForm()
        {
            _extentions = GetImageFileExtensions();
            InitializeComponent();
            
            // 初始化 ROISelector 控件
            InitializeROISelector();
            
            if (Directory.Exists(Properties.Settings.Default.ImageFolder))
            {
                ImageFolder = Properties.Settings.Default.ImageFolder;
            }
            else
            {
                ImageFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }
        }

        #endregion Constructor

        #region ROISelector Initialization

        private void InitializeROISelector()
        {
            // 创建 ROISelector 控件
            _roiSelector = new ROISelector();
            _roiSelector.Location = picBox.Location;
            _roiSelector.Size = picBox.Size;
            _roiSelector.Anchor = picBox.Anchor;
            _roiSelector.BackColor = Color.FromArgb(64, 64, 64);
            _roiSelector.TabIndex = picBox.TabIndex;
            
            // 注册默认的 ROI 类型
            _roiSelector.RegisterRoiType("Default", Properties.Settings.Default.RectangleColor, "默认标注");
            
            // 设置为配置模式
            _roiSelector.Mode = DZ_ROISelector.Enums.ControlMode.Configuration;
            
            // 订阅事件
            _roiSelector.RoiCreated += RoiSelector_RoiCreated;
            _roiSelector.RoiModified += RoiSelector_RoiModified;
            _roiSelector.RoiDeleted += RoiSelector_RoiDeleted;
            _roiSelector.RoiSelected += RoiSelector_RoiSelected;
            
            // 移除旧的 picBox，添加新的 ROISelector
            this.Controls.Remove(picBox);
            this.Controls.Add(_roiSelector);
            _roiSelector.BringToFront();
            
            // 启动创建模式
            _roiSelector.StartCreating("Default");
        }

        #endregion ROISelector Initialization

        #region ROISelector Events

        private void RoiSelector_RoiCreated(object sender, DZ_ROISelector.EventArgs.RoiCreatedEventArgs e)
        {
            // ROI 创建时自动保存
            SaveImageRectangles(CurrenIndex);
            
            // 继续创建下一个
            _roiSelector.StartCreating("Default");
        }

        private void RoiSelector_RoiModified(object sender, DZ_ROISelector.EventArgs.RoiModifiedEventArgs e)
        {
            // ROI 修改时自动保存
            SaveImageRectangles(CurrenIndex);
        }

        private void RoiSelector_RoiDeleted(object sender, DZ_ROISelector.EventArgs.RoiDeletedEventArgs e)
        {
            // ROI 删除时自动保存
            SaveImageRectangles(CurrenIndex);
        }

        private void RoiSelector_RoiSelected(object sender, DZ_ROISelector.EventArgs.RoiSelectedEventArgs e)
        {
            // 可以在这里处理 ROI 选择事件
        }

        #endregion ROISelector Events

        #region Form properties

        public string ImageFolder
        {
            get { return _imageFolder; }
            set
            {
                _imageFolder = value;
                if (!String.IsNullOrWhiteSpace(_imageFolder) && Directory.Exists(_imageFolder))
                {
                    LoadFileList();
                    LoadAnnotations();
                    CurrenIndex = 0;
                }
                Properties.Settings.Default.ImageFolder = _imageFolder;
                txtImageFolder.Text = _imageFolder;
            }
        }

        public int CurrenIndex
        {
            get
            {
                return _currenIndex;
            }

            set
            {
                if (value >= 0 && value != _currenIndex)
                {
                    SaveImageRectangles(_currenIndex);
                    _currenIndex = value;
                    LoadFile(_currenIndex);
                    LoadImageRectangles(_currenIndex);
                }
            }
        }

        #endregion Form properties

        #region Save and Load

        private void LoadFileList()
        {
            if (_files == null)
                _files = new List<string>();

            _files.Clear();

            if (!string.IsNullOrWhiteSpace(ImageFolder) && System.IO.Directory.Exists(ImageFolder))
            {
                string[] f = System.IO.Directory.GetFiles(ImageFolder);

                foreach (string file in f)
                {
                    string ext = Path.GetExtension(file).ToLowerInvariant();

                    if (_extentions.Contains(ext))
                    {
                        _files.Add(Path.GetFileName(file));
                    }
                }
                _files.Sort();
            }
        }

        private void LoadAnnotations()
        {
            if (Directory.Exists(ImageFolder))
            {
                string annXml = Path.Combine(ImageFolder, "Annotations.xml");
                if (File.Exists(annXml))
                {
                    _annList = AnnotationList.FromFile(annXml);
                }
                else
                {
                    _annList = new AnnotationList();
                }
                if (_files != null)
                {
                    foreach (string file in _files)
                    {
                        if (!_annList.ContainsKey(file))
                        {
                            _annList.Add(file);
                        }
                    }
                }
            }
        }

        private void SaveAnnotations(string fileName)
        {
            SaveImageRectangles(CurrenIndex);
            if (Directory.Exists(ImageFolder))
            {
                if (_annList == null)
                    _annList = new AnnotationList();
                _annList.Save(fileName);
            }
        }

        private void SaveImageRectangles(int index)
        {
            if (_files != null && index >= 0 && index < _files.Count && _roiSelector != null)
            {
                string file = _files[index];
                if (_annList != null)
                {
                    // 从 ROISelector 获取所有 ROI
                    var roiList = _roiSelector.GetAllRois();
                    
                    // 转换为 BRectangle 列表（使用原始图像坐标）
                    List<BRectangle> rectangles = new List<BRectangle>();
                    foreach (var roi in roiList)
                    {
                        // 使用原始图像坐标,转换为 int
                        rectangles.Add(new BRectangle(
                            (int)roi.OriginalRect.X,
                            (int)roi.OriginalRect.Y,
                            (int)roi.OriginalRect.Width,
                            (int)roi.OriginalRect.Height
                        ));
                    }
                    
                    _annList.CheckInRectangles(file, rectangles, 0, 0, 1.0f);
                }
            }
        }

        private void LoadImageRectangles(int index)
        {
            if (_files != null && index >= 0 && index < _files.Count && _roiSelector != null)
            {
                string file = _files[index];
                if (_annList != null)
                {
                    // 获取保存的矩形（原始图像坐标）
                    var rectangles = _annList.CheckoutRectangles(file, 0, 0, 1.0f);
                    
                    // 清除现有 ROI
                    _roiSelector.ClearRois();
                    
                    // 添加到 ROISelector（使用原始图像坐标）
                    if (rectangles != null)
                    {
                        foreach (var rect in rectangles)
                        {
                            _roiSelector.AddRoi("Default", new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
                        }
                    }
                    
                    // 重新启动创建模式
                    _roiSelector.StartCreating("Default");
                }
            }
        }

        private void LoadFile(int index)
        {
            if (_files != null && _files.Count > 0)
            {
                string fullPath = System.IO.Path.Combine(ImageFolder, _files[index]);
                if (System.IO.File.Exists(fullPath))
                {
                    Image loadedImage = Image.FromFile(fullPath);
                    _roiSelector.SourceImage = loadedImage;
                }
            }
        }

        private static List<string> GetImageFileExtensions()
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

            List<string> extensions = new List<string>();

            foreach (var enc in encoders)
            {
                extensions.AddRange(enc.FilenameExtension.ToLowerInvariant().Replace("*", "").Split(';'));
            }
            return extensions;
        }

        private void ExportAnnotations(string fileName)
        {
            TextWriter tw = new StreamWriter(fileName);
            string format = Properties.Settings.Default.RectangleLineFormat;
            foreach (var ann in _annList)
            {
                tw.WriteLine(ann.Key);
                tw.WriteLine(ann.Value.Count);

                foreach (var rec in ann.Value)
                    tw.WriteLine(string.Format(format, rec.X, rec.Y, rec.Width, rec.Height));
            }
            tw.Close();
        }

        #endregion Save and Load

        #region Form Events

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveAnnotations(Path.Combine(ImageFolder, "Annotations.xml"));
            Properties.Settings.Default.Save();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (FBD.ShowDialog() == DialogResult.OK)
            {
                ImageFolder = FBD.SelectedPath;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (CurrenIndex < _files.Count - 1)
                CurrenIndex++;
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (CurrenIndex > 0)
                CurrenIndex--;
        }

        private void tsSave_Click(object sender, EventArgs e)
        {
            SaveAnnotations(Path.Combine(ImageFolder, "Annotations.xml"));
        }

        private void tsSaveAs_Click(object sender, EventArgs e)
        {
            SFD.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            SFD.FileName = Path.Combine(ImageFolder, "Annotations.xml");
            if (SFD.ShowDialog() == DialogResult.OK)
            {
                SaveAnnotations(SFD.FileName);
            }
        }

        private void tsExport_Click(object sender, EventArgs e)
        {
            SFD.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            SFD.InitialDirectory = ImageFolder;
            if (SFD.ShowDialog() == DialogResult.OK)
            {
                ExportAnnotations(SFD.FileName);
            }
        }

        private void tsOptions_Click(object sender, EventArgs e)
        {
            OptionsForm ofrm = new OptionsForm();
            ofrm.Show(this);
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Delete 键由 ROISelector 内部处理
            // 这里可以添加其他快捷键
        }

        #endregion Form Events
    }
}
