using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using SubRenamer.Annotations;

namespace SubRenamer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public ObservableCollection<Model> ModelList { get; } = new ObservableCollection<Model>();
        private GridViewColumn Column { get; }

        private bool _eatSushi;
        private bool _copySub;

        public bool CopySub
        {
            get { return _copySub; }
            set
            {
                _copySub = value;
                OnPropertyChanged();
            }
        }

        public bool EatSushi
        {
            get { return _eatSushi; }
            set
            {
                _eatSushi = value;
                if (_eatSushi)
                {
                    GridView.Columns.Insert(0, Column);
                }
                else
                {
                    GridView.Columns.RemoveAt(0);
                }
                OnPropertyChanged();
            }
        }

        public void AddOriginalMovie(IEnumerable<string> files)
        {
            var fileList = files.OrderBy(t => t);
            var i = 0;
            foreach (var selectFileFileName in fileList)
            {
                if (i == ModelList.Count)
                {
                    ModelList.Add(new Model
                    {
                        OriginalMovieFile = new FileInfo(selectFileFileName)
                    });
                }
                else
                {
                    ModelList[i].OriginalMovieFile = new FileInfo(selectFileFileName);
                }
                i++;
            }
            while (i < ModelList.Count) ModelList.RemoveAt(i);
        }

        public void AddMovie(IEnumerable<string> files)
        {
            var fileList = files.OrderBy(t => t);
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
            while (i < ModelList.Count) ModelList.RemoveAt(i);
        }

        public void AddSub(IEnumerable<string> files)
        {
            var fileList = files.Select(t => new
            {
                file = new FileInfo(t),
                name = new FileInfo(t).Name
            }).Select(t => new
            {
                t.file,
                t.name,
                nameOnly = t.name.Substring(0,
                    t.name.Substring(t.name.Length - 15).IndexOf(".", StringComparison.Ordinal) + (t.name.Length - 15))
            }).OrderBy(t => t.nameOnly).ToList();

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
            i++;
            while (i < ModelList.Count) ModelList.RemoveAt(i);
        }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            Column = GridView.Columns[0];
            GridView.Columns.RemoveAt(0);
        }

        private void BtnSelectOriginalMovie_OnClick(object sender, RoutedEventArgs e)
        {
            var selectFile = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "视频文件(*.mp4;*.mkv)|*.mp4;*.mkv|所有文件 (*.*)|*.*"
            };
            if (selectFile.ShowDialog() == true)
            {
                AddOriginalMovie(selectFile.FileNames);
            }
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
                AddMovie(selectFile.FileNames);
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
                AddSub(selectFile.FileNames);
            }
        }

        private async void BtnRename_OnClick(object sender, RoutedEventArgs e)
        {
            ProgressDialogController controller = null;
            if (EatSushi)
            {
                controller = await this.ShowProgressAsync("正在处理", "正在处理第1个字幕");
                controller.Minimum = 0;
                controller.Maximum = 1;
            }
            var sb = new StringBuilder();
            for (var i = 0; i < ModelList.Count; i++)
            {
                if (EatSushi) controller?.SetMessage($"正在处理第{i + 1}个字幕");
                var model = ModelList[i];
                try
                {
                    model.GenerateRenameSubFiles(CopySub);
                    for (var j = 0; j < model.SubFiles.Count; j++)
                    {
                        if (!EatSushi)
                        {
                            if (CopySub) model.SubFiles[j].CopyTo(model.RenamedSubFiles[j].FullName);
                            else model.SubFiles[j].MoveTo(model.RenamedSubFiles[j].FullName);
                        }
                        else
                        {
                            var process = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = Path.Combine("Sushi", "sushi.exe"),
                                    Arguments = $"--src \"{model.OriginalMovieFile.FullName}\" " +
                                                $"--dst \"{model.MovieFile.FullName}\" " +
                                                $"--script \"{model.SubFiles[j].FullName}\" " +
                                                $"-o \"{model.RenamedSubFiles[j].FullName}\"",
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                }
                            };
                            process.Start();
                            await Task.Run(() => process.WaitForExit());
                        }
                        if (!CopySub) model.SubFiles[j].Delete();
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"第{i + 1}个字幕出现错误：{ex.Message}");
                }
                if (EatSushi) controller?.SetProgress((i + 1d)/ModelList.Count);
            }
            if (EatSushi && controller != null) await controller.CloseAsync();
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

        private void ListInfo_OnDrop(object sender, DragEventArgs e)
        {
            var originalMovieList = new List<string>();
            var movieList = new List<string>();
            var subList = new List<string>();
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null) return;
            foreach (var file in files)
            {
                var extension = new FileInfo(file).Extension;
                switch (extension)
                {
                    case ".mp4":
                    case ".mkv":
                        if (!EatSushi || movieList.Count == 0)
                        {
                            movieList.Add(file);
                        }
                        else if (Utils.TestSimilarity(new FileInfo(movieList[0]).Name, new FileInfo(file).Name) > 30)
                        {
                            movieList.Add(file);
                        }
                        else
                        {
                            originalMovieList.Add(file);
                        }
                        break;
                    case ".ass":
                    case ".ssa":
                    case ".srt":
                        subList.Add(file);
                        break;
                }
            }
            if (subList.Count > 0) AddSub(subList);
            if (EatSushi)
            {
                if (ModelList.Count > 0 && !string.IsNullOrWhiteSpace(ModelList[0].SubFileName))
                {
                    if (movieList.Count > 0)
                    {
                        if (Utils.TestSimilarity(ModelList[0].SubFiles[0].Name, new FileInfo(movieList[0]).Name) > 30)
                        {
                            var tempList = movieList;
                            movieList = originalMovieList;
                            originalMovieList = tempList;
                        }
                    }
                }
            }
            if (originalMovieList.Count > 0) AddOriginalMovie(originalMovieList);
            if (movieList.Count > 0) AddMovie(movieList);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
