using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using FastPin.Models;

namespace FastPin.ViewModels
{
    /// <summary>
    /// ViewModel for individual pinned items
    /// </summary>
    public class PinnedItemViewModel : ViewModelBase
    {
        private readonly PinnedItem _model;

        public PinnedItemViewModel(PinnedItem model)
        {
            _model = model;
            LoadTags();
        }

        public int Id => _model.Id;

        public ItemType Type => _model.Type;

        public string? TextContent
        {
            get => _model.TextContent;
            set
            {
                if (_model.TextContent != value)
                {
                    _model.TextContent = value;
                    _model.ModifiedDate = DateTime.Now;
                    OnPropertyChanged();
                }
            }
        }

        public byte[]? ImageData
        {
            get => _model.ImageData;
            set
            {
                if (_model.ImageData != value)
                {
                    _model.ImageData = value;
                    _model.ModifiedDate = DateTime.Now;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ImageSource));
                }
            }
        }

        public BitmapImage? ImageSource
        {
            get
            {
                if (_model.ImageData == null || _model.ImageData.Length == 0)
                    return null;

                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = new MemoryStream(_model.ImageData);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
                catch
                {
                    return null;
                }
            }
        }

        public string? FilePath
        {
            get => _model.FilePath;
            set
            {
                if (_model.FilePath != value)
                {
                    _model.FilePath = value;
                    _model.ModifiedDate = DateTime.Now;
                    OnPropertyChanged();
                }
            }
        }

        public string? FileName
        {
            get => _model.FileName;
            set
            {
                if (_model.FileName != value)
                {
                    _model.FileName = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCached
        {
            get => _model.IsCached;
            set
            {
                if (_model.IsCached != value)
                {
                    _model.IsCached = value;
                    OnPropertyChanged();
                    
                    // Trigger file caching through the parent ViewModel
                    // This will be handled in the MainWindow code-behind
                }
            }
        }

        public DateTime CreatedDate
        {
            get => _model.CreatedDate;
            set
            {
                _model.CreatedDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime ModifiedDate => _model.ModifiedDate;

        public string DisplayDate => _model.CreatedDate.ToString("MMM dd, yyyy");

        public ObservableCollection<string> Tags { get; } = new ObservableCollection<string>();

        public PinnedItem Model => _model;

        private void LoadTags()
        {
            Tags.Clear();
            foreach (var itemTag in _model.ItemTags)
            {
                Tags.Add(itemTag.Tag.Name);
            }
        }

        public void RefreshTags()
        {
            LoadTags();
            OnPropertyChanged(nameof(Tags));
        }
    }
}
