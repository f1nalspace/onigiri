namespace Finalspace.Onigiri.ViewModels
{
    public class TestMainViewModel : MainViewModel
    {
        public TestMainViewModel() : base()
        {
            _animes.Add(new TestAnimeViewModel());
            _animes.Add(new TestAnimeViewModel());
            _animes.Add(new TestAnimeViewModel());

            _users.Add(new Models.User()
            {
                UserName = "anni",
                DisplayName = "Anni",
            });
            _users.Add(new Models.User()
            {
                UserName = "final",
                DisplayName = "Final",
            });

            AnimesView.Refresh();
            UsersView.Refresh();
        }
    }
}
