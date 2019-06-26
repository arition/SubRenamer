using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SubRenamer
{
    public class ModelList 
    {
        public ObservableCollection<Model> Models { get; } = new ObservableCollection<Model>();

        public void AddOriginalMovie(IEnumerable<string> files)
        {
            var fileList = files.OrderBy(t => t);
            var i = 0;
            foreach (var selectFileFileName in fileList)
            {
                if (i == Models.Count)
                {
                    Models.Add(new Model
                    {
                        OriginalMovieFile = new FileInfo(selectFileFileName)
                    });
                }
                else
                {
                    Models[i].OriginalMovieFile = new FileInfo(selectFileFileName);
                }
                i++;
            }
            while (i < Models.Count) Models.RemoveAt(i);
        }

        public void AddMovie(IEnumerable<string> files)
        {
            var fileList = files.OrderBy(t => t);
            var i = 0;
            foreach (var selectFileFileName in fileList)
            {
                if (i == Models.Count)
                {
                    Models.Add(new Model
                    {
                        MovieFile = new FileInfo(selectFileFileName)
                    });
                }
                else
                {
                    Models[i].MovieFile = new FileInfo(selectFileFileName);
                }
                i++;
            }
            while (i < Models.Count) Models.RemoveAt(i);
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
                    t.name.Substring(t.name.Length - 15 >= 0 ? t.name.Length - 15 : 0)
                        .IndexOf(".", StringComparison.Ordinal) + (t.name.Length - 15 >= 0 ? t.name.Length - 15 : 0))
            }).OrderBy(t => t.nameOnly).ToList();

            var i = -1;
            var lastSubNameOnly = "";

            foreach (var selectFileFileName in fileList)
            {
                if (lastSubNameOnly != selectFileFileName.nameOnly)
                {
                    i++;
                    if (i < Models.Count)
                    {
                        Models[i].SubFiles.Clear();
                    }
                }
                if (i == Models.Count)
                {
                    var model = new Model();
                    model.SubFiles.Add(selectFileFileName.file);
                    Models.Add(model);
                }
                else
                {
                    Models[i].SubFiles.Add(selectFileFileName.file);
                }
                lastSubNameOnly = selectFileFileName.nameOnly;
            }
            i++;
            while (i < Models.Count) Models.RemoveAt(i);
        }

        public void AddDropFiles(IEnumerable<string> files, bool eatSushi)
        {
            var originalMovieList = new List<string>();
            var movieList = new List<string>();
            var subList = new List<string>();
            foreach (var file in files)
            {
                var extension = new FileInfo(file).Extension.ToLower();
                switch (extension)
                {
                    case ".mp4":
                    case ".mkv":
                    case ".m2ts":
                        if (!eatSushi || movieList.Count == 0)
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
            if (eatSushi)
            {
                if (Models.Count > 0 && !string.IsNullOrWhiteSpace(Models[0].SubFileName))
                {
                    if (movieList.Count > 0)
                    {
                        if (Utils.TestSimilarity(Models[0].SubFiles[0].Name, new FileInfo(movieList[0]).Name) > 30)
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
    }
}
