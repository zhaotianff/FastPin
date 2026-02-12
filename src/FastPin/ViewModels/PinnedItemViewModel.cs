using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private BitmapImage? _cachedImageSource;
        private bool _isLoadingImage;

        public PinnedItemViewModel(PinnedItem model)
        {
            _model = model;
            LoadTags();
            
            // Pre-load image asynchronously if available
            if (_model.ImageData != null && _model.ImageData.Length > 0)
            {
                _ = LoadImageAsync();
            }
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

        public string? RichTextContent
        {
            get => _model.RichTextContent;
            set
            {
                if (_model.RichTextContent != value)
                {
                    _model.RichTextContent = value;
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
                    _cachedImageSource = null; // Clear cache when data changes
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ImageSource));
                    
                    // Load image asynchronously to avoid blocking UI
                    if (value != null && value.Length > 0)
                    {
                        _ = LoadImageAsync();
                    }
                }
            }
        }

        public BitmapImage? ImageSource
        {
            get
            {
                if (_model.ImageData == null || _model.ImageData.Length == 0)
                    return null;

                // Return cached image if available
                if (_cachedImageSource != null)
                    return _cachedImageSource;

                // If not cached and not currently loading, start async load
                if (!_isLoadingImage)
                {
                    _ = LoadImageAsync();
                }

                return null; // Will be set via property change notification when loaded
            }
        }

        private async Task LoadImageAsync()
        {
            if (_isLoadingImage || _model.ImageData == null || _model.ImageData.Length == 0)
                return;

            _isLoadingImage = true;

            try
            {
                // Clone the data to avoid holding references during async operation
                var imageDataCopy = (byte[])_model.ImageData.Clone();

                // Decode image on background thread to avoid blocking UI
                var bitmap = await Task.Run(() =>
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = new MemoryStream(imageDataCopy);
                    
                    // For large images, decode at reduced size to improve memory usage and performance
                    // Max width of 2000px should be sufficient for most displays while reducing load time
                    if (imageDataCopy.Length > 1024 * 1024) // If larger than 1MB
                    {
                        bmp.DecodePixelWidth = 2000;
                    }
                    
                    bmp.EndInit();
                    bmp.Freeze(); // Freeze to make it cross-thread accessible
                    return bmp;
                });

                _cachedImageSource = bitmap;
                OnPropertyChanged(nameof(ImageSource));
            }
            catch
            {
                // Silently fail for corrupted image data - keep null to avoid breaking UI
                _cachedImageSource = null;
            }
            finally
            {
                _isLoadingImage = false;
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

        public ObservableCollection<TagViewModel> Tags { get; } = new ObservableCollection<TagViewModel>();

        public PinnedItem Model => _model;

        public int? ImageWidth => _model.ImageWidth;

        public int? ImageHeight => _model.ImageHeight;

        public long? FileSize => _model.FileSize;

        public ItemSource Source => _model.Source;
        
        public string? SourceApplication => _model.SourceApplication;
        
        public string SourceDisplayText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SourceApplication))
                    return string.Empty;
                
                return $"From {SourceApplication}";
            }
        }

        public string FileSizeFormatted
        {
            get
            {
                if (!FileSize.HasValue)
                    return string.Empty;

                double size = FileSize.Value;
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                int order = 0;
                while (size >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    size = size / 1024;
                }
                return $"{size:0.##} {sizes[order]}";
            }
        }

        public string ImageDimensionsFormatted
        {
            get
            {
                if (ImageWidth.HasValue && ImageHeight.HasValue)
                    return $"{ImageWidth}x{ImageHeight}";
                return string.Empty;
            }
        }

        public string ImageMetadata
        {
            get
            {
                if (ImageWidth.HasValue && ImageHeight.HasValue && FileSize.HasValue)
                    return $"{ImageWidth}x{ImageHeight} • {FileSizeFormatted}";
                else if (ImageWidth.HasValue && ImageHeight.HasValue)
                    return $"{ImageWidth}x{ImageHeight}";
                else if (FileSize.HasValue)
                    return FileSizeFormatted;
                return string.Empty;
            }
        }

        private void LoadTags()
        {
            Tags.Clear();
            foreach (var itemTag in _model.ItemTags)
            {
                Tags.Add(new TagViewModel(itemTag.Tag));
            }
        }

        public void RefreshTags()
        {
            LoadTags();
            OnPropertyChanged(nameof(Tags));
        }
    }
}
