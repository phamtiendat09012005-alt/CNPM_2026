namespace SportsStoreAI.API.Services;

public static class OrderWorkflow
{
    private static readonly Dictionary<string, string[]> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AwaitingPayment"] = new[] { "Pending", "Cancelled" },
        ["Pending"] = new[] { "Confirmed", "Cancelled" },
        ["Confirmed"] = new[] { "Processing", "Cancelled" },
        ["Processing"] = new[] { "Shipping" },
        ["Shipping"] = new[] { "Completed" },
        ["Completed"] = Array.Empty<string>(),
        ["Cancelled"] = Array.Empty<string>()
    };

    public static bool CanMove(string current, string next)
        => Allowed.TryGetValue(current, out var values)
           && values.Contains(next, StringComparer.OrdinalIgnoreCase);

    public static void EnsureCanMove(string current, string next)
    {
        if (!CanMove(current, next))
        {
            throw new InvalidOperationException(
                $"Không thể chuyển trạng thái từ {current} sang {next}.");
        }
    }
}
