using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace SubRenamer
{
    public class Model : INotifyPropertyChanged
    {
        private FileInfo _movieFile;

        public FileInfo MovieFile
        {
            get { return _movieFile; }
            set
            {
                _movieFile = value;
                OnPropertyChanged("MovieFileName");
                try
                {
                    GenerateRenameSubFiles();
                }
                catch
                {
                    // ignored
                }
            }
        }

        public string MovieFileName => MovieFile?.Name;
        public ObservableCollection<FileInfo> SubFiles { get; set; } = new ObservableCollection<FileInfo>();
        public string SubFileName => string.Join(Environment.NewLine, SubFiles.Select(t => t.Name));
        public ObservableCollection<FileInfo> RenamedSubFiles { get; set; } = new ObservableCollection<FileInfo>();
        public string RenamedSubFileName => string.Join(Environment.NewLine, RenamedSubFiles.Select(t => t.Name));

        public event PropertyChangedEventHandler PropertyChanged;

        public Model()
        {
            SubFiles.CollectionChanged += (e, o) =>
            {
                OnPropertyChanged("SubFileName");
                try
                {
                    GenerateRenameSubFiles();
                }
                catch
                {
                    // ignored
                }
            };
            RenamedSubFiles.CollectionChanged += (e, o) => OnPropertyChanged("RenamedSubFileName");
        }

        public virtual void GenerateRenameSubFiles(bool copyToMovieLocation = false)
        {
            if (MovieFile == null || !MovieFile.Exists) throw new FileNotFoundException("视频文件不存在");
            if (SubFiles.Count == 0 || SubFiles.Any(t => !t.Exists)) throw new FileNotFoundException("字幕文件不存在");
            RenamedSubFiles.Clear();
            var fileName = MovieFileName.Substring(0, MovieFileName.LastIndexOf(".", StringComparison.Ordinal));
            foreach (var subFileInfo in SubFiles)
            {
                var extension = subFileInfo.Name.Substring(subFileInfo.Name.IndexOf(".", StringComparison.Ordinal));
                if (copyToMovieLocation)
                {
                    if (MovieFile.Directory != null)
                    {
                        var path = Path.Combine(MovieFile.Directory.FullName, fileName + extension);
                        RenamedSubFiles.Add(new FileInfo(path));
                    }
                    else
                    {
                        throw new FileNotFoundException("视频文件不存在", MovieFile.FullName);
                    }
                }
                else
                {
                    if (subFileInfo.Directory != null)
                    {
                        var path = Path.Combine(subFileInfo.Directory.FullName, fileName + extension);
                        RenamedSubFiles.Add(new FileInfo(path));
                    }
                    else
                    {
                        throw new FileNotFoundException("字幕文件不存在", subFileInfo.FullName);
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
