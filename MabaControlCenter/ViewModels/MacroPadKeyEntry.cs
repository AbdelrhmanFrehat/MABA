namespace MabaControlCenter.ViewModels;

public sealed class MacroPadKeyEntry : ViewModelBase
{
    private DexterCalibrationViewModel? _vm;
    private string _mapping = "";
    private int _physicalIndex;
    private bool _bulkLoad;

    public MacroPadKeyEntry(string id)
    {
        Id = id;
    }

    public void Attach(DexterCalibrationViewModel vm) => _vm = vm;

    public string Id { get; }
    public string Label => Id.ToUpperInvariant();

    public int PhysicalIndex
    {
        get => _physicalIndex;
        set
        {
            if (_physicalIndex == value) return;
            _physicalIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayTitle));
        }
    }

    public string DisplayTitle => $"{Label} ({PhysicalIndex})";

    public string Mapping
    {
        get => _mapping;
        set
        {
            var v = value ?? "";
            if (_mapping == v) return;
            _mapping = v;
            OnPropertyChanged();
            if (!_bulkLoad)
                _vm?.OnKeyMappingCommitted();
        }
    }

    internal void SetMappingFromProfile(string value)
    {
        var v = value ?? "";
        if (_mapping == v) return;
        _bulkLoad = true;
        try
        {
            _mapping = v;
            OnPropertyChanged(nameof(Mapping));
        }
        finally
        {
            _bulkLoad = false;
        }
    }

    public bool IsTargetListen =>
        _vm != null && string.Equals(_vm.ListeningKeyId, Id, StringComparison.Ordinal);

    public string ListenButtonText => IsTargetListen ? "Cancel" : "Listen 🎙";

    public void RefreshListenUi()
    {
        OnPropertyChanged(nameof(IsTargetListen));
        OnPropertyChanged(nameof(ListenButtonText));
    }
}
