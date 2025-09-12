namespace HoU.GuildBot.Shared.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// Splits the <paramref name="items"/> into multiple messages, so that each message does not exceed Discord's message length limit of 2000 characters,
    /// including a <paramref name="baseMessageLength"/> to account for any additional text that may be added to each message.
    /// </summary>
    /// <param name="items">The items to split.</param>
    /// <param name="baseMessageLength">The length of the base message.</param>
    /// <returns>The <paramref name="items"/> in chunks that will fit into a single Discord message.</returns>
    public static List<string> SplitLongMessageWithList(this ICollection<string> items,
        int baseMessageLength)
    {
        decimal freeSpaceBaseAfterMessage = 2000 - Math.Max(baseMessageLength + 1, 500);
        var result = new List<string>();
        var allListElements = new Queue<string>(items.Select(m => m + " "));
        var totalLength = allListElements.Sum(m => m.Length);
        var totalMessagesRequired = (int)Math.Ceiling(totalLength / freeSpaceBaseAfterMessage);
        if (totalMessagesRequired == 1)
        {
            result.Add(string.Join(string.Empty, allListElements));
        }
        else
        {
            string? addToNext = null;
            for (var i = 0; i < totalMessagesRequired; i++)
            {
                var sb = new StringBuilder();
                if (addToNext != null)
                {
                    sb.Append(addToNext);
                    addToNext = null;
                }

                while (allListElements.TryDequeue(out var nextListElement))
                {
                    if (sb.Length + nextListElement.Length > freeSpaceBaseAfterMessage)
                    {
                        addToNext = nextListElement;
                        break;
                    }

                    sb.Append(nextListElement);
                }
                result.Add(sb.ToString());
            }
        }

        return result;
    }
}
