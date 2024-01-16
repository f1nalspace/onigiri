using Finalspace.Onigiri.ViewModels;

namespace Finalspace.Onigiri.Services
{
    public interface IThemeManagerService
    {
        MainTheme CurrentTheme { get; }
        void ChangeTheme(MainTheme theme);
    }
}
