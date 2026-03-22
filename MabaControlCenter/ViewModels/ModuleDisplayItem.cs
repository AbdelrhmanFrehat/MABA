using MabaControlCenter.Models;

namespace MabaControlCenter.ViewModels;

public class ModuleDisplayItem : ViewModelBase
{
    private bool _isRecommended;

    public ModuleDisplayItem(ModuleInfo module)
    {
        Module = module;
    }

    public ModuleInfo Module { get; }

    public bool IsRecommended
    {
        get => _isRecommended;
        set
        {
            if (_isRecommended == value) return;
            _isRecommended = value;
            OnPropertyChanged();
        }
    }
}
