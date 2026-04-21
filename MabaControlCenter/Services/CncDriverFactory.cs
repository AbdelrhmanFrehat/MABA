using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public class CncDriverFactory : ICncDriverFactory
{
    private readonly ILoggingService _loggingService;

    public CncDriverFactory(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public ICncDriver CreateDriver(CncMachineProfile profile)
    {
        return profile.DriverType switch
        {
            CncDriverType.ArduinoSerial => new ArduinoSerialCncDriver(_loggingService, profile),
            CncDriverType.Simulated => new SimulatedCncDriver(_loggingService, profile),
            _ => new ArduinoSerialCncDriver(_loggingService, profile)
        };
    }
}
