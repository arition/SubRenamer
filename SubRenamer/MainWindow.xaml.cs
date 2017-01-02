using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace SubRenamer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public ObservableCollection<Model> ModelList { get; } = new ObservableCollection<Model>();
        public bool CopySub { get; set; }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        } 

        private void BtnSelectMovie_OnClick(object sender, RoutedEventArgs e)
        {
            var selectFile = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "视频文件(*.mp4;*.mkv)|*.mp4;*.mkv|所有文件 (*.*)|*.*"
            };
            if (selectFile.ShowDialog() == true)
            {
                var fileList = selectFile.FileNames.OrderBy(t => t);
                var i = 0;
                foreach (var selectFileFileName in fileList)
                {
                    if (i == ModelList.Count)
                    {
                        ModelList.Add(new Model
                        {
                            MovieFile = new FileInfo(selectFileFileName)
                        });
                    }
                    else
                    {
                        ModelList[i].MovieFile = new FileInfo(selectFileFileName);
                    }
                    i++;
                }
            }
        }

        private void BtnSelectSub_OnClick(object sender, RoutedEventArgs e)
        {
            var selectFile = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "字幕文件(*.ass;*.ssa;*.srt)|*.ass;*.ssa;*.srt|所有文件 (*.*)|*.*"
            };
            if (selectFile.ShowDialog() == true)
            {
                var fileList = selectFile.FileNames.Select(t => new
                {
                    file = new FileInfo(t),
                    name = new FileInfo(t).Name
                }).Select(t => new
                {
                    t.file,
                    t.name,
                    nameOnly = t.name.Substring(0, t.name.IndexOf(".", StringComparison.Ordinal))
                }).OrderBy(t=>t.nameOnly).ToList();

                var i = -1;
                var lastSubNameOnly = "";
                
                foreach (var selectFileFileName in fileList)
                {
                    if (lastSubNameOnly != selectFileFileName.nameOnly)
                    {
                        i++;
                        if (i < ModelList.Count)
                        {
                            ModelList[i].SubFiles.Clear();
                        }
                    }
                    if (i == ModelList.Count)
                    {
                        var model = new Model();
                        model.SubFiles.Add(selectFileFileName.file);
                        ModelList.Add(model);
                    }
                    else
                    {
                        ModelList[i].SubFiles.Add(selectFileFileName.file); 
                    }
                    lastSubNameOnly = selectFileFileName.nameOnly;
                }
            }
        }

        private async void BtnRename_OnClick(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < ModelList.Count; i++)
            {
                var model = ModelList[i];
                try
                {
                    model.GenerateRenameSubFiles(CopySub);
                    for (var j = 0; j < model.SubFiles.Count; j++)
                    {
                        if (CopySub) model.SubFiles[j].CopyTo(model.RenamedSubFiles[j].FullName);
                        else model.SubFiles[j].MoveTo(model.RenamedSubFiles[j].FullName);
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"第{i + 1}个字幕出现错误：{ex.Message}");
                }
            }
            var message = sb.ToString();
            if (string.IsNullOrWhiteSpace(message))
            {
                await this.ShowMessageAsync("成功", "重命名成功");
            }
            else
            {
                await this.ShowMessageAsync("错误", message);
            }
            ModelList.Clear();
        }

        private void BtnClearList_OnClick(object sender, RoutedEventArgs e)
        {
            ModelList.Clear();
        }
    }
}
