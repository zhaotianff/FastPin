using FastPin.Models;

namespace FastPin.ViewModels
{
    /// <summary>
    /// ViewModel for Tag display and editing
    /// </summary>
    public class TagViewModel : ViewModelBase
    {
        private readonly Tag _model;

        public TagViewModel(Tag model)
        {
            _model = model;
        }

        public int Id => _model.Id;

        public string Name
        {
            get => _model.Name;
            set
            {
                if (_model.Name != value)
                {
                    _model.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Color
        {
            get => _model.Color;
            set
            {
                if (_model.Color != value)
                {
                    _model.Color = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Class
        {
            get => _model.Class;
            set
            {
                if (_model.Class != value)
                {
                    _model.Class = value;
                    OnPropertyChanged();
                }
            }
        }

        public Tag Model => _model;
    }
}
