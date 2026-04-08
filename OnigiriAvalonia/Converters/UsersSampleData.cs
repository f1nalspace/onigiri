using Finalspace.Onigiri.Models;

namespace Finalspace.Onigiri.Converters;

public static class UsersSampleData
{
    public static User[] DefaultUsers { get; } =
    [
        new User { UserName = "final", DisplayName = "Final" },
        new User { UserName = "anni", DisplayName = "Anni" },
    ];
}
