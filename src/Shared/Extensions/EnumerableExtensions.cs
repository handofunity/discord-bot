using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.Shared.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Performs a bulk operation on the <paramref name="items"/> with an <paramref name="itemAction"/> executed for each item.
    /// If there's more than one item, the next item will be delayed by the <see cref="Constants.GlobalActionDelay"/>.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <param name="items">The items to process.</param>
    /// <param name="itemAction">The action to perform for each item.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    public static async Task PerformBulkOperation<T>(this IEnumerable<T> items, Func<T, Task> itemAction)
    {
        // If there's more than one item, this method will include a delay between each
        // item to comply with the rate limits before those are even hit.
        var enumeration = items.ToArray();
        for (var i = 0; i < enumeration.Length; i++)
        {
            var item = enumeration[i];
            await itemAction(item);

            if (enumeration.Length > 1      // Delay only if there's more than one item
             && i < enumeration.Length - 1) // Delay if the item is not the last item
                await Task.Delay(Constants.GlobalActionDelay);
        }
    }
}