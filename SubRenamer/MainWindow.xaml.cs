using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
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
        private GridViewColumn Column { get; }
        public ModelList ModelList { get; } = new ModelList();
        private bool _eatSushi;
        private bool _copySub;

        public bool CopySub
        {
            get => _copySub;
            set
            {
                _copySub = value;
                OnPropertyChanged();
            }
        }

        public bool EatSushi
        {
            get => _eatSushi;
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
                ModelList.AddOriginalMovie(selectFile.FileNames);
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
                ModelList.AddMovie(selectFile.FileNames);
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
                ModelList.AddSub(selectFile.FileNames);
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
            for (var i = 0; i < ModelList.Models.Count; i++)
            {
                if (EatSushi) controller?.SetMessage($"正在处理第{i + 1}个字幕");
                var model = ModelList.Models[i];
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
                            if (!CopySub) model.SubFiles[j].Delete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"第{i + 1}个字幕出现错误：{ex.Message}");
                }
                if (EatSushi) controller?.SetProgress((i + 1d)/ModelList.Models.Count);
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
            ModelList.Models.Clear();
        }

        private void BtnClearList_OnClick(object sender, RoutedEventArgs e)
        {
            ModelList.Models.Clear();
        }

        private void ListInfo_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
                ModelList.AddDropFiles(files, EatSushi);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
