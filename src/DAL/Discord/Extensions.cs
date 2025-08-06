﻿using ButtonComponent = HoU.GuildBot.Shared.Objects.ButtonComponent;
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
        
        var rows = new Dictionary<int, List<IMessageComponentBuilder>>();
        foreach (var actionComponent in md.ActionComponents)
        {
            if (!rows.TryGetValue(actionComponent.ActionRowNumber, out var messageComponentBuilders))
            {
                messageComponentBuilders = new List<IMessageComponentBuilder>();
                rows[actionComponent.ActionRowNumber] = messageComponentBuilders;
            }
            
            switch (actionComponent)
            {
                case ButtonComponent button:
                {
                    var buttonBuilder = new ButtonBuilder()
                                       .WithCustomId(button.CustomId)
                                       .WithLabel(button.Label)
                                       .WithStyle((ButtonStyle)button.Style);
                    messageComponentBuilders.Add(buttonBuilder);
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

                    messageComponentBuilders.Add(menuBuilder);
                    break;
                }
            }
        }

        foreach (var row in rows)
        {
            var actionRowBuilder = new ActionRowBuilder()
               .WithComponents(row.Value);
            
            builder.Components.ActionRows.Add(actionRowBuilder);
        }

        return builder.Build();
    }
}