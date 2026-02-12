using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FastPin.Commands;
using FastPin.Data;
using FastPin.Models;
using FastPin.Services;
using Microsoft.EntityFrameworkCore;

namespace FastPin.ViewModels
{
    /// <summary>
    /// Main ViewModel for the FastPin application
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly FastPinDbContext _dbContext;
        private readonly ClipboardMonitorService _clipboardMonitor;
        private readonly HotkeyService _hotkeyService;
        private string _searchText = string.Empty;
        private string _newTagName = string.Empty;
        private string _previewNewTagName = string.Empty;
        private PinnedItemViewModel? _selectedItem;
        private bool _groupByDate = true;
        private ItemType? _selectedItemType = null;
        private DateTime? _selectedDate = null;
        private string? _selectedTag = null;
        private string _currentLanguage = "en-US";
        
        // Supported image file extensions for clipboard conversion
        private static readonly string[] SupportedImageExtensions = 
            { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".ico", ".webp" };
        
        // Clipboard preview data
        private string? _clipboardPreviewText;
        private string? _clipboardPreviewRichText;
        private byte[]? _clipboardPreviewImage;
        private BitmapImage? _clipboardPreviewImageSource;
        private string? _clipboardPreviewFilePath;
        private ItemType? _clipboardPreviewType;

        public MainViewModel()
        {
            _dbContext = new FastPinDbContext();
            DatabaseInitializer.Initialize(_dbContext);

            _clipboardMonitor = new ClipboardMonitorService();
            _clipboardMonitor.ClipboardChanged += OnClipboardChanged;

            _hotkeyService = new HotkeyService();
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;

            // Initialize commands
            PinTextCommand = new RelayCommand(PinText);
            PinImageCommand = new RelayCommand(PinImage);
            PinFileCommand = new RelayCommand(PinFile);
            DeleteItemCommand = new RelayCommand<PinnedItemViewModel>(DeleteItem, item => item != null);
            AddTagCommand = new RelayCommand(AddTag, () => !string.IsNullOrWhiteSpace(NewTagName));
            RemoveTagCommand = new RelayCommand<string>(RemoveTag);
            OpenTagManagementCommand = new RelayCommand(OpenTagManagement);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            ToggleGroupingCommand = new RelayCommand(ToggleGrouping);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            ChangeLanguageCommand = new RelayCommand<string>(ChangeLanguage);
            PinClipboardCommand = new RelayCommand(PinClipboard);
            DiscardClipboardCommand = new RelayCommand(DiscardClipboard);
            CopyItemCommand = new RelayCommand<PinnedItemViewModel>(CopyItem);
            ViewImageCommand = new RelayCommand<PinnedItemViewModel>(ViewImage, item => item != null && item.Type == ItemType.Image);
            AddPreviewTagCommand = new RelayCommand(AddPreviewTag, () => !string.IsNullOrWhiteSpace(PreviewNewTagName));
            RemovePreviewTagCommand = new RelayCommand<string>(RemovePreviewTag);
            ShowAddTagPopupCommand = new RelayCommand<PinnedItemViewModel>(ShowAddTagPopup);
            SelectExistingTagCommand = new RelayCommand<string>(SelectExistingTag);

            LoadItems();
            LoadAllTags();
        }

        public ObservableCollection<PinnedItemViewModel> Items { get; } = new ObservableCollection<PinnedItemViewModel>();
        public ObservableCollection<ItemGroup> GroupedItems { get; } = new ObservableCollection<ItemGroup>();
        public ObservableCollection<string> AllTags { get; } = new ObservableCollection<string>();
        public ObservableCollection<TagViewModel> PreviewItemTags { get; } = new ObservableCollection<TagViewModel>();

        public bool GroupByDate
        {
            get => _groupByDate;
            set
            {
                if (SetProperty(ref _groupByDate, value))
                {
                    LoadItems();
                }
            }
        }

        public PinnedItemViewModel? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    LoadItems();
                }
            }
        }

        public string NewTagName
        {
            get => _newTagName;
            set => SetProperty(ref _newTagName, value);
        }

        public string PreviewNewTagName
        {
            get => _previewNewTagName;
            set => SetProperty(ref _previewNewTagName, value);
        }

        public ItemType? SelectedItemType
        {
            get => _selectedItemType;
            set
            {
                if (SetProperty(ref _selectedItemType, value))
                {
                    LoadItems();
                }
            }
        }

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    LoadItems();
                }
            }
        }

        public string? SelectedTag
        {
            get => _selectedTag;
            set
            {
                if (SetProperty(ref _selectedTag, value))
                {
                    LoadItems();
                }
            }
        }

        public ICommand PinTextCommand { get; }
        public ICommand PinImageCommand { get; }
        public ICommand PinFileCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand AddTagCommand { get; }
        public ICommand RemoveTagCommand { get; }
        public ICommand OpenTagManagementCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand ToggleGroupingCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand ChangeLanguageCommand { get; }
        public ICommand PinClipboardCommand { get; }
        public ICommand DiscardClipboardCommand { get; }
        public ICommand CopyItemCommand { get; }
        public ICommand ViewImageCommand { get; }
        public ICommand AddPreviewTagCommand { get; }
        public ICommand RemovePreviewTagCommand { get; }
        public ICommand ShowAddTagPopupCommand { get; }
        public ICommand SelectExistingTagCommand { get; }

        public string? ClipboardPreviewText => _clipboardPreviewText;
        public ItemType? ClipboardPreviewType => _clipboardPreviewType;

        public BitmapImage? ClipboardPreviewImageSource => _clipboardPreviewImageSource;

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (SetProperty(ref _currentLanguage, value))
                {
                    ChangeLanguage(value);
                }
            }
        }

        private void ChangeLanguage(string? cultureName)
        {
            if (string.IsNullOrEmpty(cultureName))
                return;

            FastPin.Resources.LocalizationService.SetCulture(cultureName);
            
            // Reload items to update date group labels
            LoadItems();
        }

        private void OnClipboardChanged(object? sender, EventArgs e)
        {
            try
            {
                // Store clipboard data for preview instead of auto-pinning
                if (Clipboard.ContainsText())
                {
                    _clipboardPreviewText = Clipboard.GetText();
                    
                    // Check if clipboard contains rich text format
                    var dataObject = Clipboard.GetDataObject();
                    if (dataObject != null && dataObject.GetDataPresent(DataFormats.Rtf))
                    {
                        _clipboardPreviewRichText = dataObject.GetData(DataFormats.Rtf) as string;
                    }
                    else
                    {
                        _clipboardPreviewRichText = null;
                    }
                    
                    _clipboardPreviewType = ItemType.Text;
                    _clipboardPreviewImage = null;
                    _clipboardPreviewImageSource = null;
                    _clipboardPreviewFilePath = null;
                }
                else if (Clipboard.ContainsImage())
                {
                    var image = Clipboard.GetImage();
                    if (image != null)
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(image));
                        using (var stream = new MemoryStream())
                        {
                            encoder.Save(stream);
                            _clipboardPreviewImage = stream.ToArray();
                        }
                        
                        // Create cached BitmapImage for preview
                        try
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = new MemoryStream(_clipboardPreviewImage);
                            bitmap.EndInit();
                            bitmap.Freeze();
                            _clipboardPreviewImageSource = bitmap;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error creating image preview: {ex.Message}");
                            _clipboardPreviewImageSource = null;
                        }
                        
                        _clipboardPreviewType = ItemType.Image;
                        _clipboardPreviewText = null;
                        _clipboardPreviewFilePath = null;
                    }
                }
                else if (Clipboard.ContainsFileDropList())
                {
                    var files = Clipboard.GetFileDropList();
                    if (files != null && files.Count > 0)
                    {
                        var filePath = files[0]?.ToString();
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            // Check if the file is an image
                            var extension = Path.GetExtension(filePath).ToLowerInvariant();
                            
                            if (SupportedImageExtensions.Contains(extension) && File.Exists(filePath))
                            {
                                // Handle as image content rather than file
                                try
                                {
                                    var bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.UriSource = new Uri(filePath);
                                    bitmap.EndInit();
                                    bitmap.Freeze();
                                    
                                    // Convert to byte array
                                    var encoder = new PngBitmapEncoder();
                                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                                    using (var stream = new MemoryStream())
                                    {
                                        encoder.Save(stream);
                                        _clipboardPreviewImage = stream.ToArray();
                                    }
                                    
                                    _clipboardPreviewImageSource = bitmap;
                                    _clipboardPreviewType = ItemType.Image;
                                    _clipboardPreviewText = null;
                                    _clipboardPreviewFilePath = null;
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error loading image file: {ex.Message}");
                                    // Fallback to file handling
                                    _clipboardPreviewFilePath = filePath;
                                    _clipboardPreviewType = ItemType.File;
                                    _clipboardPreviewText = null;
                                    _clipboardPreviewImage = null;
                                    _clipboardPreviewImageSource = null;
                                }
                            }
                            else
                            {
                                // Handle as regular file
                                _clipboardPreviewFilePath = filePath;
                                _clipboardPreviewType = ItemType.File;
                                _clipboardPreviewText = null;
                                _clipboardPreviewImage = null;
                                _clipboardPreviewImageSource = null;
                            }
                        }
                    }
                }
                
                // Notify that clipboard preview is available
                OnPropertyChanged(nameof(ClipboardPreviewText));
                OnPropertyChanged(nameof(ClipboardPreviewType));
                OnPropertyChanged(nameof(ClipboardPreviewImageSource));
            }
            catch (Exception ex)
            {
                // Silently log errors to avoid disrupting user workflow
                System.Diagnostics.Debug.WriteLine($"Error processing clipboard: {ex.Message}");
            }
        }

        private void PinText()
        {
            try
            {
                if (!Clipboard.ContainsText())
                    return;

                var text = Clipboard.GetText();
                if (string.IsNullOrWhiteSpace(text))
                    return;

                var item = new PinnedItem
                {
                    Type = ItemType.Text,
                    TextContent = text,
                    Source = ItemSource.Clipboard,
                    SourceApplication = _clipboardMonitor.LastClipboardSource,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                _dbContext.PinnedItems.Add(item);
                _dbContext.SaveChanges();

                // Reload items to ensure both grouped and ungrouped views are synchronized
                LoadItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error pinning text: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PinImage()
        {
            try
            {
                if (!Clipboard.ContainsImage())
                    return;

                var image = Clipboard.GetImage();
                if (image == null)
                    return;

                byte[] imageData;
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    imageData = stream.ToArray();
                }

                var item = new PinnedItem
                {
                    Type = ItemType.Image,
                    ImageData = imageData,
                    ImageWidth = image.PixelWidth,
                    ImageHeight = image.PixelHeight,
                    FileSize = imageData.Length,
                    Source = ItemSource.Clipboard,
                    SourceApplication = _clipboardMonitor.LastClipboardSource,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                _dbContext.PinnedItems.Add(item);
                _dbContext.SaveChanges();

                // Reload items to ensure both grouped and ungrouped views are synchronized
                LoadItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error pinning image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PinFile()
        {
            try
            {
                if (!Clipboard.ContainsFileDropList())
                    return;

                var files = Clipboard.GetFileDropList();
                if (files == null || files.Count == 0)
                    return;

                foreach (string? filePath in files)
                {
                    if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                        continue;

                    var fileInfo = new FileInfo(filePath);
                    var item = new PinnedItem
                    {
                        Type = ItemType.File,
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath),
                        FileSize = fileInfo.Length,
                        Source = ItemSource.Clipboard,
                        SourceApplication = _clipboardMonitor.LastClipboardSource,
                        IsCached = false, // Default to link mode
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };

                    _dbContext.PinnedItems.Add(item);
                    _dbContext.SaveChanges();
                }

                // Reload items to ensure both grouped and ungrouped views are synchronized
                LoadItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error pinning file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task ToggleFileCacheAsync(PinnedItemViewModel? itemViewModel)
        {
            if (itemViewModel == null)
                return;

            // Capture the IsCached value on the UI thread to avoid race conditions
            var isCached = itemViewModel.IsCached;
            var itemId = itemViewModel.Id;

            try
            {
                await Task.Run(() =>
                {
                    // Create a new DbContext for thread-safety
                    using var dbContext = new FastPinDbContext();
                    
                    var item = dbContext.PinnedItems.Find(itemId);
                    if (item == null || item.Type != ItemType.File)
                        return;

                    if (isCached)
                    {
                        // Cache the file
                        if (!string.IsNullOrEmpty(item.FilePath) && File.Exists(item.FilePath))
                        {
                            item.CachedFileData = File.ReadAllBytes(item.FilePath);
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show("File not found. Cannot cache.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                itemViewModel.IsCached = false;
                            });
                            return;
                        }
                    }
                    else
                    {
                        // Clear cached data
                        item.CachedFileData = null;
                    }

                    item.IsCached = isCached;
                    item.ModifiedDate = DateTime.Now;
                    dbContext.SaveChanges();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling file cache: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteItem(PinnedItemViewModel? itemToDelete)
        {
            if (itemToDelete == null)
                return;

            try
            {
                var result = MessageBox.Show(
                    FastPin.Resources.LocalizationService.GetString("DeleteConfirmation"),
                    FastPin.Resources.LocalizationService.GetString("ConfirmDelete"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                var item = _dbContext.PinnedItems.Find(itemToDelete.Id);
                if (item != null)
                {
                    _dbContext.PinnedItems.Remove(item);
                    _dbContext.SaveChanges();
                    
                    // Reload items to refresh both grouped and ungrouped views
                    LoadItems();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{FastPin.Resources.LocalizationService.GetString("Error")}: {ex.Message}", 
                    FastPin.Resources.LocalizationService.GetString("Error"), 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        private void AddTag()
        {
            if (SelectedItem == null || string.IsNullOrWhiteSpace(NewTagName))
                return;

            try
            {
                var tagName = NewTagName.Trim();

                // Find or create tag
                var tag = _dbContext.Tags.FirstOrDefault(t => t.Name == tagName);
                if (tag == null)
                {
                    tag = new Tag { Name = tagName };
                    _dbContext.Tags.Add(tag);
                    _dbContext.SaveChanges();
                }

                // Check if already tagged
                var existingItemTag = _dbContext.ItemTags
                    .FirstOrDefault(it => it.PinnedItemId == SelectedItem.Id && it.TagId == tag.Id);

                if (existingItemTag == null)
                {
                    var itemTag = new ItemTag
                    {
                        PinnedItemId = SelectedItem.Id,
                        TagId = tag.Id
                    };
                    _dbContext.ItemTags.Add(itemTag);
                    _dbContext.SaveChanges();

                    // Reload the item to get updated tags
                    var item = _dbContext.PinnedItems
                        .Include(p => p.ItemTags)
                        .ThenInclude(it => it.Tag)
                        .FirstOrDefault(p => p.Id == SelectedItem.Id);

                    if (item != null)
                    {
                        SelectedItem.RefreshTags();
                    }
                }

                NewTagName = string.Empty;
                LoadAllTags();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding tag: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveTag(string? tagName)
        {
            if (SelectedItem == null || string.IsNullOrWhiteSpace(tagName))
                return;

            try
            {
                var tag = _dbContext.Tags.FirstOrDefault(t => t.Name == tagName);
                if (tag != null)
                {
                    var itemTag = _dbContext.ItemTags
                        .FirstOrDefault(it => it.PinnedItemId == SelectedItem.Id && it.TagId == tag.Id);

                    if (itemTag != null)
                    {
                        _dbContext.ItemTags.Remove(itemTag);
                        _dbContext.SaveChanges();

                        // Refresh the item tags
                        var item = _dbContext.PinnedItems
                            .Include(p => p.ItemTags)
                            .ThenInclude(it => it.Tag)
                            .FirstOrDefault(p => p.Id == SelectedItem.Id);

                        if (item != null)
                        {
                            SelectedItem.RefreshTags();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing tag: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowAddTagPopup(PinnedItemViewModel? item)
        {
            if (item != null)
            {
                SelectedItem = item;
                NewTagName = string.Empty;
                // The popup will be shown by the event handler in MainWindow.xaml.cs
                OnAddTagPopupRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SelectExistingTag(string? tagName)
        {
            if (!string.IsNullOrWhiteSpace(tagName))
            {
                NewTagName = tagName;
                // Automatically add the tag when selected from the list
                AddTag();
                OnAddTagPopupClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler? OnAddTagPopupRequested;
        public event EventHandler? OnAddTagPopupClosed;

        private void AddPreviewTag()
        {
            if (string.IsNullOrWhiteSpace(PreviewNewTagName))
                return;

            try
            {
                var tagName = PreviewNewTagName.Trim();

                // Find or create tag in database
                var tag = _dbContext.Tags.FirstOrDefault(t => t.Name == tagName);
                if (tag == null)
                {
                    tag = new Tag { Name = tagName };
                    _dbContext.Tags.Add(tag);
                    _dbContext.SaveChanges();
                }

                // Check if already in preview tags
                if (!PreviewItemTags.Any(t => t.Name == tagName))
                {
                    PreviewItemTags.Add(new TagViewModel(tag));
                }

                PreviewNewTagName = string.Empty;
                LoadAllTags();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding tag: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemovePreviewTag(string? tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return;

            try
            {
                var tag = PreviewItemTags.FirstOrDefault(t => t.Name == tagName);
                if (tag != null)
                {
                    PreviewItemTags.Remove(tag);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing tag: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenTagManagement()
        {
            var window = new TagManagementWindow();
            window.ShowDialog();
            
            // Refresh tags after window closes
            LoadAllTags();
            LoadItems(); // Reload items to reflect any tag changes
        }

        private void LoadItems()
        {
            Items.Clear();
            GroupedItems.Clear();

            var query = _dbContext.PinnedItems
                .Include(p => p.ItemTags)
                .ThenInclude(it => it.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                query = query.Where(p =>
                    (p.TextContent != null && p.TextContent.ToLower().Contains(search)) ||
                    (p.FileName != null && p.FileName.ToLower().Contains(search)) ||
                    p.ItemTags.Any(it => it.Tag.Name.ToLower().Contains(search)));
            }

            // Filter by item type
            if (SelectedItemType.HasValue)
            {
                query = query.Where(p => p.Type == SelectedItemType.Value);
            }

            // Filter by tag
            if (!string.IsNullOrWhiteSpace(SelectedTag))
            {
                query = query.Where(p => p.ItemTags.Any(it => it.Tag.Name == SelectedTag));
            }

            // Filter by date
            if (SelectedDate.HasValue)
            {
                var selectedDay = SelectedDate.Value.Date;
                var nextDay = selectedDay.AddDays(1);
                query = query.Where(p => p.CreatedDate >= selectedDay && p.CreatedDate < nextDay);
            }

            var items = query
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            if (GroupByDate)
            {
                // Group items by date
                var groups = items
                    .GroupBy(p => GetDateGroup(p.CreatedDate))
                    .OrderByDescending(g => g.First().CreatedDate);

                foreach (var group in groups)
                {
                    var itemGroup = new ItemGroup
                    {
                        DateGroup = group.Key
                    };

                    foreach (var item in group)
                    {
                        itemGroup.Items.Add(new PinnedItemViewModel(item));
                    }

                    GroupedItems.Add(itemGroup);
                }
            }
            else
            {
                // Load items without grouping
                foreach (var item in items)
                {
                    Items.Add(new PinnedItemViewModel(item));
                }
            }
        }

        private string GetDateGroup(DateTime date)
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            if (date.Date == today)
                return FastPin.Resources.LocalizationService.GetString("Today");
            else if (date.Date == yesterday)
                return FastPin.Resources.LocalizationService.GetString("Yesterday");
            else if (date > today.AddDays(-7))
                return FastPin.Resources.LocalizationService.GetString("ThisWeek");
            else if (date > today.AddDays(-30))
                return FastPin.Resources.LocalizationService.GetString("ThisMonth");
            else if (date.Year == today.Year)
                return date.ToString("MMMM yyyy");
            else
                return date.ToString("yyyy");
        }

        private void ToggleGrouping()
        {
            GroupByDate = !GroupByDate;
        }

        private void LoadAllTags()
        {
            AllTags.Clear();
            var tags = _dbContext.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList();
            foreach (var tag in tags)
            {
                AllTags.Add(tag);
            }
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        private void ClearFilters()
        {
            SelectedItemType = null;
            SelectedDate = null;
            SelectedTag = null;
            SearchText = string.Empty;
        }

        public void StartClipboardMonitoring()
        {
            _clipboardMonitor.Start();
        }

        public void StopClipboardMonitoring()
        {
            _clipboardMonitor.Stop();
        }

        public void StartHotkeyMonitoring()
        {
            _hotkeyService.RegisterHotkey();
        }

        public void StopHotkeyMonitoring()
        {
            _hotkeyService.UnregisterHotkey();
        }

        private void OnHotkeyPressed(object? sender, EventArgs e)
        {
            // Raise event to show quick pin menu (handled in View)
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }

        private void PinClipboard()
        {
            if (_clipboardPreviewType == null)
                return;

            try
            {
                switch (_clipboardPreviewType.Value)
                {
                    case ItemType.Text:
                        if (!string.IsNullOrEmpty(_clipboardPreviewText))
                        {
                            var item = new PinnedItem
                            {
                                Type = ItemType.Text,
                                TextContent = _clipboardPreviewText,
                                RichTextContent = _clipboardPreviewRichText,
                                Source = ItemSource.Clipboard,
                                SourceApplication = _clipboardMonitor.LastClipboardSource,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now
                            };

                            _dbContext.PinnedItems.Add(item);
                            _dbContext.SaveChanges();
                            
                            // Associate selected tags
                            AssociatePreviewTags(item.Id);
                            
                            LoadItems();
                        }
                        break;
                    case ItemType.Image:
                        if (_clipboardPreviewImage != null && _clipboardPreviewImage.Length > 0)
                        {
                            // Use cached preview image data
                            int? imageWidth = null;
                            int? imageHeight = null;
                            
                            if (_clipboardPreviewImageSource != null)
                            {
                                imageWidth = _clipboardPreviewImageSource.PixelWidth;
                                imageHeight = _clipboardPreviewImageSource.PixelHeight;
                            }
                            
                            var item = new PinnedItem
                            {
                                Type = ItemType.Image,
                                ImageData = _clipboardPreviewImage,
                                ImageWidth = imageWidth,
                                ImageHeight = imageHeight,
                                FileSize = _clipboardPreviewImage.Length,
                                Source = ItemSource.Clipboard,
                                SourceApplication = _clipboardMonitor.LastClipboardSource,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now
                            };

                            _dbContext.PinnedItems.Add(item);
                            _dbContext.SaveChanges();
                            
                            // Associate selected tags
                            AssociatePreviewTags(item.Id);
                            
                            LoadItems();
                        }
                        break;
                    case ItemType.File:
                        if (!string.IsNullOrEmpty(_clipboardPreviewFilePath) && File.Exists(_clipboardPreviewFilePath))
                        {
                            var fileInfo = new FileInfo(_clipboardPreviewFilePath);
                            var item = new PinnedItem
                            {
                                Type = ItemType.File,
                                FilePath = _clipboardPreviewFilePath,
                                FileName = fileInfo.Name,
                                FileSize = fileInfo.Length,
                                Source = ItemSource.Clipboard,
                                SourceApplication = _clipboardMonitor.LastClipboardSource,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now
                            };

                            _dbContext.PinnedItems.Add(item);
                            _dbContext.SaveChanges();
                            
                            // Associate selected tags
                            AssociatePreviewTags(item.Id);
                            
                            LoadItems();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error pinning clipboard content: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            DiscardClipboard();
        }

        private void DiscardClipboard()
        {
            _clipboardPreviewText = null;
            _clipboardPreviewRichText = null;
            _clipboardPreviewImage = null;
            _clipboardPreviewImageSource = null;
            _clipboardPreviewFilePath = null;
            _clipboardPreviewType = null;
            PreviewItemTags.Clear();
            OnPropertyChanged(nameof(ClipboardPreviewText));
            OnPropertyChanged(nameof(ClipboardPreviewType));
            OnPropertyChanged(nameof(ClipboardPreviewImageSource));
        }

        private void AssociatePreviewTags(int itemId)
        {
            try
            {
                foreach (var tagViewModel in PreviewItemTags)
                {
                    var tag = _dbContext.Tags.FirstOrDefault(t => t.Name == tagViewModel.Name);
                    if (tag != null)
                    {
                        var itemTag = new ItemTag
                        {
                            PinnedItemId = itemId,
                            TagId = tag.Id
                        };
                        _dbContext.ItemTags.Add(itemTag);
                    }
                }
                _dbContext.SaveChanges();
                PreviewItemTags.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error associating tags: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyItem(PinnedItemViewModel? item)
        {
            if (item == null)
                return;

            try
            {
                switch (item.Type)
                {
                    case ItemType.Text:
                        if (!string.IsNullOrEmpty(item.TextContent))
                        {
                            // If rich text is available, copy it along with plain text
                            if (!string.IsNullOrEmpty(item.RichTextContent))
                            {
                                var dataObject = new DataObject();
                                dataObject.SetData(DataFormats.Text, item.TextContent);
                                dataObject.SetData(DataFormats.Rtf, item.RichTextContent);
                                Clipboard.SetDataObject(dataObject, true);
                            }
                            else
                            {
                                Clipboard.SetText(item.TextContent);
                            }
                        }
                        break;
                    case ItemType.Image:
                        if (item.ImageSource != null)
                        {
                            Clipboard.SetImage(item.ImageSource);
                        }
                        break;
                    case ItemType.File:
                        if (!string.IsNullOrEmpty(item.FilePath))
                        {
                            var files = new System.Collections.Specialized.StringCollection();
                            files.Add(item.FilePath);
                            Clipboard.SetFileDropList(files);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewImage(PinnedItemViewModel? item)
        {
            if (item == null || item.Type != ItemType.Image)
                return;

            try
            {
                var imageViewer = new ImageViewerWindow(item);
                imageViewer.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening image viewer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler? HotkeyPressed;
    }
}
