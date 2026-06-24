using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FastPin.Commands;
using FastPin.Models;
using FastPin.Services;

namespace FastPin.ViewModels
{
    /// <summary>
    /// ViewModel for Tag Management window
    /// </summary>
    public class TagManagementViewModel : ViewModelBase, IDisposable
    {
        private readonly FastPinApiClient _apiClient;
        private TagViewModel? _selectedTag;
        private string _editingTagName = string.Empty;
        private string? _editingTagClass;
        private string? _editingTagColor;
        private bool _isEditingExisting;

        public TagManagementViewModel()
        {
            _apiClient = new FastPinApiClient();

            SaveTagCommand = new RelayCommand(SaveTag, CanSaveTag);
            NewTagCommand = new RelayCommand(NewTag);
            DeleteTagCommand = new RelayCommand<TagViewModel>(DeleteTag);
            SetColorCommand = new RelayCommand<string>(SetColor);

            LoadTags();
        }

        public ObservableCollection<TagViewModel> Tags { get; } = new ObservableCollection<TagViewModel>();

        public TagViewModel? SelectedTag
        {
            get => _selectedTag;
            set
            {
                if (SetProperty(ref _selectedTag, value))
                {
                    LoadTagForEditing();
                }
            }
        }

        public string EditingTagName
        {
            get => _editingTagName;
            set => SetProperty(ref _editingTagName, value);
        }

        public string? EditingTagClass
        {
            get => _editingTagClass;
            set => SetProperty(ref _editingTagClass, value);
        }

        public string? EditingTagColor
        {
            get => _editingTagColor;
            set => SetProperty(ref _editingTagColor, value);
        }

        public ICommand SaveTagCommand { get; }
        public ICommand NewTagCommand { get; }
        public ICommand DeleteTagCommand { get; }
        public ICommand SetColorCommand { get; }

        private void LoadTags()
        {
            Tags.Clear();
            var tags = _apiClient.GetTagsAsync().GetAwaiter().GetResult()
                .OrderBy(t => t.Name)
                .ToList();

            foreach (var tag in tags)
            {
                Tags.Add(new TagViewModel(tag));
            }
        }

        private void LoadTagForEditing()
        {
            if (SelectedTag == null)
            {
                EditingTagName = string.Empty;
                EditingTagClass = null;
                EditingTagColor = null;
                _isEditingExisting = false;
            }
            else
            {
                EditingTagName = SelectedTag.Name;
                EditingTagClass = SelectedTag.Class;
                EditingTagColor = SelectedTag.Color;
                _isEditingExisting = true;
            }
        }

        private bool CanSaveTag()
        {
            return !string.IsNullOrWhiteSpace(EditingTagName);
        }

        private void SaveTag()
        {
            try
            {
                var tagName = EditingTagName.Trim();

                if (_isEditingExisting && SelectedTag != null)
                {
                    if (Tags.Any(t => t.Name == tagName && t.Id != SelectedTag.Id))
                    {
                        MessageBox.Show("A tag with this name already exists.", "Duplicate Tag", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var updated = new Tag
                    {
                        Id = SelectedTag.Id,
                        Name = tagName,
                        Class = string.IsNullOrWhiteSpace(EditingTagClass) ? null : EditingTagClass.Trim(),
                        Color = string.IsNullOrWhiteSpace(EditingTagColor) ? null : EditingTagColor.Trim()
                    };

                    _apiClient.UpdateTagAsync(updated).GetAwaiter().GetResult();
                    SelectedTag.Name = updated.Name;
                    SelectedTag.Class = updated.Class;
                    SelectedTag.Color = updated.Color;
                }
                else
                {
                    if (Tags.Any(t => t.Name == tagName))
                    {
                        MessageBox.Show("A tag with this name already exists.", "Duplicate Tag", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var newTag = _apiClient.EnsureTagAsync(tagName).GetAwaiter().GetResult();
                    newTag.Class = string.IsNullOrWhiteSpace(EditingTagClass) ? null : EditingTagClass.Trim();
                    newTag.Color = string.IsNullOrWhiteSpace(EditingTagColor) ? null : EditingTagColor.Trim();
                    _apiClient.UpdateTagAsync(newTag).GetAwaiter().GetResult();

                    var tagViewModel = new TagViewModel(newTag);
                    Tags.Add(tagViewModel);
                    SelectedTag = tagViewModel;
                }

                MessageBox.Show("Tag saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving tag: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewTag()
        {
            SelectedTag = null;
            EditingTagName = string.Empty;
            EditingTagClass = null;
            EditingTagColor = "#0078D4";
            _isEditingExisting = false;
        }

        private void DeleteTag(TagViewModel? tag)
        {
            if (tag == null)
                return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the tag '{tag.Name}'? This will remove it from all pinned items.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _apiClient.DeleteTagAsync(tag.Id).GetAwaiter().GetResult();
                    Tags.Remove(tag);

                    if (SelectedTag == tag)
                    {
                        SelectedTag = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting tag: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetColor(string? colorHex)
        {
            if (!string.IsNullOrWhiteSpace(colorHex))
            {
                EditingTagColor = colorHex;
            }
        }

        public void Dispose()
        {
        }
    }
}
