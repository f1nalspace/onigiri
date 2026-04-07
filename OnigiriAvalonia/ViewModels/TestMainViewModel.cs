namespace Finalspace.Onigiri.ViewModels;

public class TestMainViewModel : MainViewModel
{
    public TestMainViewModel() : base()
    {
        _allAnimes.Add(new TestAnimeViewModel());
        _allAnimes.Add(new TestAnimeViewModel());
        _allAnimes.Add(new TestAnimeViewModel());

        _allUsers.Add(new Models.User()
        {
            UserName = "anni",
            DisplayName = "Anni",
        });
        _allUsers.Add(new Models.User()
        {
            UserName = "final",
            DisplayName = "Final",
        });

        RefreshAnimes();
        Users.ReplaceAll(_allUsers);
    }
}
