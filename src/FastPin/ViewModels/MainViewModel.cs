using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
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
        private string _searchText = string.Empty;
        private string _newTagName = string.Empty;
        private PinnedItemViewModel? _selectedItem;

        public MainViewModel()
        {
            _dbContext = new FastPinDbContext();
            _dbContext.Database.EnsureCreated();

            _clipboardMonitor = new ClipboardMonitorService();
            _clipboardMonitor.ClipboardChanged += OnClipboardChanged;

            // Initialize commands
            PinTextCommand = new RelayCommand(PinText);
            PinImageCommand = new RelayCommand(PinImage);
            PinFileCommand = new RelayCommand(PinFile);
            DeleteItemCommand = new RelayCommand(DeleteItem, () => SelectedItem != null);
            AddTagCommand = new RelayCommand(AddTag, () => !string.IsNullOrWhiteSpace(NewTagName));
            ClearSearchCommand = new RelayCommand(ClearSearch);

            LoadItems();
            LoadAllTags();
        }

        public ObservableCollection<PinnedItemViewModel> Items { get; } = new ObservableCollection<PinnedItemViewModel>();
        public ObservableCollection<string> AllTags { get; } = new ObservableCollection<string>();

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

        public ICommand PinTextCommand { get; }
        public ICommand PinImageCommand { get; }
        public ICommand PinFileCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand AddTagCommand { get; }
        public ICommand ClearSearchCommand { get; }

        private void OnClipboardChanged(object? sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    PinText();
                }
                else if (Clipboard.ContainsImage())
                {
                    PinImage();
                }
                else if (Clipboard.ContainsFileDropList())
                {
                    PinFile();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing clipboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                _dbContext.PinnedItems.Add(item);
                _dbContext.SaveChanges();

                var viewModel = new PinnedItemViewModel(item);
                Items.Insert(0, viewModel);
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
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                _dbContext.PinnedItems.Add(item);
                _dbContext.SaveChanges();

                var viewModel = new PinnedItemViewModel(item);
                Items.Insert(0, viewModel);
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

                foreach (string filePath in files)
                {
                    if (!File.Exists(filePath))
                        continue;

                    var item = new PinnedItem
                    {
                        Type = ItemType.File,
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath),
                        IsCached = false, // Default to link mode
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };

                    _dbContext.PinnedItems.Add(item);
                    _dbContext.SaveChanges();

                    var viewModel = new PinnedItemViewModel(item);
                    Items.Insert(0, viewModel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error pinning file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteItem()
        {
            if (SelectedItem == null)
                return;

            try
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this item?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                var item = _dbContext.PinnedItems.Find(SelectedItem.Id);
                if (item != null)
                {
                    _dbContext.PinnedItems.Remove(item);
                    _dbContext.SaveChanges();
                    Items.Remove(SelectedItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void LoadItems()
        {
            Items.Clear();

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

            var items = query
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            foreach (var item in items)
            {
                Items.Add(new PinnedItemViewModel(item));
            }
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

        public void StartClipboardMonitoring()
        {
            _clipboardMonitor.Start();
        }

        public void StopClipboardMonitoring()
        {
            _clipboardMonitor.Stop();
        }
    }
}
