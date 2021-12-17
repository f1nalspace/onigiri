namespace Finalspace.Onigiri.Events
{
    public class StatusChangedArgs
    {
        public string Header { get; set; }
        public string Subject { get; set; }
        public int Percentage { get; set; }
    }
    public delegate void StatusChangedEventHandler(object sender, StatusChangedArgs args);
}
