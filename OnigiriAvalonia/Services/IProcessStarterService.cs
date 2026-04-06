namespace Finalspace.Onigiri.Services;

public interface IProcessStarterService
{
    void Start(string executable, params string[] args);
}
