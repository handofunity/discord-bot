namespace HoU.GuildBot.Shared.DAL;

public interface IWebAccess
{
    /// <summary>
    /// Tries to load the content located at the <paramref name="url"/>.
    /// </summary>
    /// <param name="url">The URL where the content can be found.</param>
    /// <returns>The content as raw bytes, or <b>null</b>, if the content couldn't be loaded.</returns>
    Task<byte[]?> GetContentFromUrlAsync(string? url);
}