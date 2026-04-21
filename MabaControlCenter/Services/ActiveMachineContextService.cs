using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class ActiveMachineContextService : IActiveMachineContextService
{
    private readonly IEffectiveCapabilitiesResolver _capabilitiesResolver;

    public ActiveMachineContextService(IEffectiveCapabilitiesResolver capabilitiesResolver)
    {
        _capabilitiesResolver = capabilitiesResolver;
        Current = new ActiveMachineContext();
    }

    public ActiveMachineContext Current { get; private set; }
    public event EventHandler? ContextChanged;

    public ActiveMachineContext Resolve(RuntimeProfile? profile, MachineDefinition? liveDefinition = null)
    {
        if (profile == null)
        {
            Current = new ActiveMachineContext { StatusText = "No runtime profile is active." };
            ContextChanged?.Invoke(this, EventArgs.Empty);
            return Current;
        }

        var definition = liveDefinition ?? profile.DefinitionSnapshot;
        if (definition == null)
        {
            Current = new ActiveMachineContext
            {
                RuntimeProfile = profile,
                StatusText = "The active runtime profile is missing its machine definition snapshot."
            };
            ContextChanged?.Invoke(this, EventArgs.Empty);
            return Current;
        }

        var driverType = ResolveDriverType(profile, definition);
        var driverCapabilities = _capabilitiesResolver.ToDriverCapabilities(driverType);
        Current = new ActiveMachineContext
        {
            RuntimeProfile = profile,
            MachineDefinition = definition,
            DriverType = driverType,
            CncDriverType = _capabilitiesResolver.ToCncDriverType(driverType),
            DriverCapabilities = driverCapabilities,
            EffectiveCapabilities = _capabilitiesResolver.Resolve(definition, driverCapabilities, driverType),
            RuntimeUiVariant = string.IsNullOrWhiteSpace(definition.RuntimeBinding.RuntimeUiVariant) ? "generic-v1" : definition.RuntimeBinding.RuntimeUiVariant,
            IsOfflineSnapshot = liveDefinition == null,
            StatusText = liveDefinition == null
                ? $"Using cached definition snapshot for {definition.DisplayNameEn}."
                : $"Using current machine definition {definition.DisplayNameEn} v{definition.Version}."
        };

        ContextChanged?.Invoke(this, EventArgs.Empty);
        return Current;
    }

    private static DriverType ResolveDriverType(RuntimeProfile profile, MachineDefinition definition)
    {
        var requested = profile.Overrides.DriverType ?? definition.RuntimeBinding.DefaultDriverType;
        if (definition.RuntimeBinding.SupportedDriverTypes.Count == 0 || definition.RuntimeBinding.SupportedDriverTypes.Contains(requested))
            return requested;

        return definition.RuntimeBinding.SupportedDriverTypes.FirstOrDefault();
    }
}
