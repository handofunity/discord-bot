using ButtonComponent = HoU.GuildBot.Shared.Objects.ButtonComponent;
using SelectMenuComponent = HoU.GuildBot.Shared.Objects.SelectMenuComponent;

namespace HoU.GuildBot.DAL.Discord;

public static class Extensions
{
    internal static Embed? ToEmbed(this EmbedData? ed)
    {
        if (ed == null)
            return null;
        var builder = new EmbedBuilder();
        if (ed.Author != null)
            builder.Author = new EmbedAuthorBuilder
            {
                Name = ed.Author,
                Url = ed.AuthorUrl,
                IconUrl = ed.AuthorIconUrl
            };
        if (ed.ThumbnailUrl != null)
            builder.ThumbnailUrl = ed.ThumbnailUrl;
        if (ed.Title != null)
            builder.Title = ed.Title;
        if (ed.Url != null)
            builder.Url = ed.Url;
        if (ed.Color != null)
            builder.Color = new Color(ed.Color.Value.R, ed.Color.Value.G, ed.Color.Value.B);
        if (ed.Description != null)
            builder.Description = ed.Description;
        if (ed.Fields != null)
        {
            foreach (var fd in ed.Fields)
            {
                builder.AddField(fieldBuilder => fieldBuilder.WithName(fd.Name)
                                                             .WithValue(fd.Value ?? "<null>")
                                                             .WithIsInline(fd.Inline));
            }
        }

        if (ed.FooterText != null)
        {
            builder.Footer = new EmbedFooterBuilder()
               .WithText(ed.FooterText);
        }

        if (ed.ImageUrl != null)
            builder.ImageUrl = ed.ImageUrl;

        return builder.Build();
    }

    internal static Modal? ToModal(this ModalData? md)
    {
        if (md is null)
            return null;

        var builder = new ModalBuilder()
                     .WithCustomId(md.CustomId)
                     .WithTitle(md.Title);

        if (!md.ActionComponents.Any())
            return builder.Build();
        
        var rows = new Dictionary<int, List<IMessageComponent>>();
        foreach (var actionComponent in md.ActionComponents)
        {
            if (!rows.TryGetValue(actionComponent.ActionRowNumber, out var messageComponents))
            {
                messageComponents = new List<IMessageComponent>();
                rows[actionComponent.ActionRowNumber] = messageComponents;
            }
            
            switch (actionComponent)
            {
                case ButtonComponent button:
                {
                    var buttonBuilder = new ButtonBuilder()
                                       .WithCustomId(button.CustomId)
                                       .WithLabel(button.Label)
                                       .WithStyle((ButtonStyle)button.Style);
                    messageComponents.Add(buttonBuilder.Build());
                    break;
                }
                case SelectMenuComponent selectMenu:
                {
                    var menuBuilder = new SelectMenuBuilder()
                                     .WithCustomId(selectMenu.CustomId)
                                     .WithPlaceholder(selectMenu.Placeholder)
                                     .WithMinValues(0)
                                     .WithMaxValues(selectMenu.AllowMultiple ? selectMenu.Options.Count : 1);
                    foreach (var (optionKey, label) in selectMenu.Options)
                        menuBuilder.AddOption(label, optionKey);

                    messageComponents.Add(menuBuilder.Build());
                    break;
                }
            }
        }

        foreach (var row in rows)
            builder.AddComponents(row.Value, row.Key);

        return builder.Build();
    }
}