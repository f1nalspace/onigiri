using Finalspace.Onigiri.Models;
using System;
using System.Windows.Markup;

namespace Finalspace.Onigiri.Converters
{
    public class UsersSampleData : MarkupExtension
    {
        private static readonly User[] _defaultUsers = new[] {
            new User() { UserName = "final", DisplayName = "Final" },
            new User() { UserName = "anni", DisplayName = "Anni" },
        };

        public override object ProvideValue(IServiceProvider serviceProvider) => _defaultUsers;
    }
}
