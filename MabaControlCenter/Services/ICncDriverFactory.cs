using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface ICncDriverFactory
{
    ICncDriver CreateDriver(CncMachineProfile profile);
}
