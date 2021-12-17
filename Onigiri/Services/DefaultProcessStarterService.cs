using System.Diagnostics;

namespace Finalspace.Onigiri.Services
{
    class DefaultProcessStarterService : IProcessStarterService
    {
        public void Start(string executable, params string[] args)
        {
            string argsString = args.Length > 0 ? string.Join(" ", args) : "";
            Process.Start(executable, argsString);
        }
    }
}
