using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
            get => _movieFile;
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
        private FileInfo _originalMovieFile;

        public FileInfo OriginalMovieFile
        {
            get => _originalMovieFile;
            set
            {
                _originalMovieFile = value;
                OnPropertyChanged("OriginalMovieFileName");
            }
        }

        public string MovieFileName => MovieFile?.Name;
        public string OriginalMovieFileName => OriginalMovieFile?.Name;
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
                switch (o.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        try
                        {
                            GenerateRenameSubFiles(o.NewStartingIndex);
                        }
                        catch
                        {
                            // ignored
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        RenamedSubFiles.Clear();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
            RenamedSubFiles.CollectionChanged += (e, o) => OnPropertyChanged("RenamedSubFileName");
        }

        public virtual void GenerateRenameSubFiles(int index, bool copyToMovieLocation = false)
        {
            var subFileInfo = SubFiles[index];
            var fileName = MovieFileName.Substring(0, MovieFileName.LastIndexOf(".", StringComparison.Ordinal));
            var extension = subFileInfo.Name.Substring(
                subFileInfo.Name.Substring(subFileInfo.Name.Length - 15 >= 0 ? subFileInfo.Name.Length - 15 : 0)
                    .IndexOf(".", StringComparison.Ordinal) +
                (subFileInfo.Name.Length - 15 >= 0 ? subFileInfo.Name.Length - 15 : 0));
            if (copyToMovieLocation)
            {
                if (MovieFile.Directory != null)
                {
                    var path = Path.Combine(MovieFile.Directory.FullName, fileName + extension);
                    RenamedSubFiles.Insert(index, new FileInfo(path));
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
                    RenamedSubFiles.Insert(index, new FileInfo(path));
                }
                else
                {
                    throw new FileNotFoundException("字幕文件不存在", subFileInfo.FullName);
                }
            }
        }

        public virtual void GenerateRenameSubFiles(bool copyToMovieLocation = false)
        {
            RenamedSubFiles.Clear();
            for (var i = 0; i < SubFiles.Count; i++)
            {
                GenerateRenameSubFiles(i, copyToMovieLocation);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
