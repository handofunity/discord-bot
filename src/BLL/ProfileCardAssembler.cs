﻿namespace HoU.GuildBot.BLL;

internal static class ProfileCardAssembler
{
    private static readonly SKColor _textColor = SKColors.White;
    private static readonly SKTypeface _titleTypeface = SKTypeface.FromFamilyName("Linux Libertine");
    private static readonly SKTypeface _contentTypeface = SKTypeface.FromFamilyName("Marcellus");
    private static readonly SKFont _titleFont = new(_titleTypeface, size: 28f);
    private static readonly SKPaint _titlePaint = new(_titleFont) { Color = _textColor };
    private static readonly SKFont _guildFont = new(_titleTypeface, size: 40f);
    private static readonly SKPaint _guildPaint = new(_guildFont) { Color = _textColor };
    private static readonly SKFont _unitsAndRankFont = new(_contentTypeface, size: 22f);
    private static readonly SKPaint _unitsAndRankPaint = new(_unitsAndRankFont) { Color = _textColor };
    private static readonly SKFont _tableHeaderFont = new(_titleTypeface, size: 24f);
    private static readonly SKPaint _tableHeaderPaint = new(_tableHeaderFont) { Color = _textColor };
    private static readonly SKFont _tableContentFont = new(_contentTypeface, size: 20f);
    private static readonly SKPaint _tableContentPaint = new(_tableContentFont) { Color = _textColor };

    internal static void AssembleProfileCard(SKBitmap bitmap,
                                             string displayName,
                                             SKImage backgroundImage,
                                             SKImage avatarFrameImage,
                                             SKImage? userImage,
                                             Dictionary<string, SKImage> archetypeImages,
                                             ProfileInfoResponse profileData,
                                             string guildTag,
                                             bool hasPvpRole,
                                             bool hasArtisanRole,
                                             bool hasPveRole)
    {
        using var canvas = new SKCanvas(bitmap);

        canvas.DrawImage(backgroundImage, 0, 0);
        if (userImage != null)
            canvas.DrawImage(userImage, 68, 74);
        canvas.DrawImage(avatarFrameImage, 0, 0);
        WriteName(canvas, displayName);
        WriteGuildTag(canvas, guildTag);
        WriteRankWithPercentageRange(canvas, profileData.SeasonalRankName, profileData.SeasonalRankPercentageRange);
        WriteUnitsAndRank(canvas, profileData);
        WritePlayStyles(canvas, hasPvpRole, hasArtisanRole, hasPveRole);
        WriteCharactersTitle(canvas);
        WriteCharacters(canvas, profileData, archetypeImages);
    }

    private static void WriteName(SKCanvas canvas,
                                  string displayName)
    {
        const int maxNameLengthInPixel = 507;
        const int maxNameHeightInPixel = 64;

        var nameFont = new SKFont(_titleTypeface, size: 96f);
        var namePaint = new SKPaint(nameFont)
        {
            Color = _textColor
        };

        // Calculate name and name position
        var nameSize = ShrinkFontAndPaintToFitSize(displayName, nameFont, namePaint, maxNameLengthInPixel, maxNameHeightInPixel);

        // Measure the tallest letter to get the vertical offset of the baseline.
        float verticalLetterSize = 0;
        var maxLetterSize = new SKRect();
        var measured = new List<char>(displayName.Length);
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var character in displayName)
        {
            if (measured.Contains(character))
                continue; // Do not measure the same character twice.

            measured.Add(character);
            namePaint.MeasureText(character.ToString(), ref maxLetterSize);
            if (maxLetterSize.Height > verticalLetterSize)
                verticalLetterSize = maxLetterSize.Height;
        }

        var nameLeftOffset = 681 - nameSize.Width / 2f;
        var nameTopOffset = 278 + verticalLetterSize / 2f;

        // Write name
        canvas.DrawText(displayName,
                        nameLeftOffset,
                        nameTopOffset,
                        nameFont,
                        namePaint);
    }

    private static void WriteGuildTag(SKCanvas canvas,
                                      string guildTag)
    {
        SKRect guildTagSize = new();
        _guildPaint.MeasureText(guildTag, ref guildTagSize);
        canvas.DrawText(guildTag,
                        682f - guildTagSize.Width / 2f,
                        372f + guildTagSize.Height / 2f,
                        _guildFont,
                        _guildPaint);
    }

    private static void WriteRankWithPercentageRange(SKCanvas canvas,
                                      string rank,
                                      string? percentageRange)
    {
        const float maxWidthInPixel = 172f;
        const float maxHeightInPixel = 19f;

        var rankAndPercentageRangeFont = new SKFont(_contentTypeface, size: 18f);
        var rankAndPercentageRangePaint = new SKPaint(rankAndPercentageRangeFont) { Color = _textColor };

        var sb = new StringBuilder(rank);
        if (percentageRange is not null)
            sb.Append($" ({percentageRange})");
        var text = sb.ToString();

        var rankAndPercentageRangeSize = ShrinkFontAndPaintToFitSize(text,
                                                                     rankAndPercentageRangeFont,
                                                                     rankAndPercentageRangePaint,
                                                                     maxWidthInPixel,
                                                                     maxHeightInPixel);
        canvas.DrawText(text,
                        682f - rankAndPercentageRangeSize.Width / 2f,
                        398f + rankAndPercentageRangeSize.Height / 2f,
                        rankAndPercentageRangeFont,
                        rankAndPercentageRangePaint);
    }

    private static void WriteUnitsAndRank(SKCanvas canvas,
                                          ProfileInfoResponse profileData)
    {
        const float verticalOffset = 487f;
        canvas.DrawText("UNITS:",
                        59f,
                        verticalOffset,
                        _titleFont,
                        _titlePaint);
        canvas.DrawText(profileData.SeasonalTokens.ToString("0"),
                        146f,
                        verticalOffset,
                        _unitsAndRankFont,
                        _unitsAndRankPaint);
        canvas.DrawText("RANK:",
                        268f,
                        verticalOffset,
                        _titleFont,
                        _titlePaint);
        canvas.DrawText(profileData.SeasonalRank.ToString(),
                        351f,
                        verticalOffset,
                        _unitsAndRankFont,
                        _unitsAndRankPaint);
    }

    private static void WritePlayStyles(SKCanvas canvas,
                                        bool hasPvpRole,
                                        bool hasArtisanRole,
                                        bool hasPveRole)
    {
        const float verticalOffset = 487f;
        const float firstSectionOffset = 415f;
        const float playStylesMaxWidth = 534f;

        var playStyles = 0;
        if (hasPvpRole)
            playStyles++;
        if (hasArtisanRole)
            playStyles++;
        if (hasPveRole)
            playStyles++;

        // Depending on the amount of play styles, split the max width into equal sections.
        // For each section, center the corresponding text and write it.
        var sectionWidth = playStylesMaxWidth / playStyles;
        var currentOffset = firstSectionOffset;
        if (hasPvpRole)
        {
            var (text, offset) = GetTextAndOffsetForCenteredText("PvP", sectionWidth, _titlePaint);
            canvas.DrawText(text, currentOffset + offset, verticalOffset, _titleFont, _titlePaint);
            currentOffset += sectionWidth;
        }
        if (hasArtisanRole)
        {
            var (text, offset) = GetTextAndOffsetForCenteredText("Artisan", sectionWidth, _titlePaint);
            canvas.DrawText(text, currentOffset + offset, verticalOffset, _titleFont, _titlePaint);
            currentOffset += sectionWidth;
        }
        if (hasPveRole)
        {
            var (text, offset) = GetTextAndOffsetForCenteredText("PvE", sectionWidth, _titlePaint);
            canvas.DrawText(text, currentOffset + offset, verticalOffset, _titleFont, _titlePaint);
        }
    }

    private static void WriteCharactersTitle(SKCanvas canvas)
    {
        const float tableHeaderVerticalOffset = 548f;
        canvas.DrawText("Lv",
                        59f,
                        tableHeaderVerticalOffset,
                        _tableHeaderFont,
                        _tableHeaderPaint);
        canvas.DrawText("Character",
                        165f,
                        tableHeaderVerticalOffset,
                        _tableHeaderFont,
                        _tableHeaderPaint);
        canvas.DrawText("Class",
                        347f,
                        tableHeaderVerticalOffset,
                        _tableHeaderFont,
                        _tableHeaderPaint);
        canvas.DrawText("Artisan",
                        485f,
                        tableHeaderVerticalOffset,
                        _tableHeaderFont,
                        _tableHeaderPaint);
        canvas.DrawText("Lv",
                        640f,
                        tableHeaderVerticalOffset,
                        _tableHeaderFont,
                        _tableHeaderPaint);
        canvas.DrawText("Artisan",
                        752f,
                        tableHeaderVerticalOffset,
                        _tableHeaderFont,
                        _tableHeaderPaint);
        canvas.DrawText("Lv",
                        913f,
                        tableHeaderVerticalOffset,
                        _tableHeaderFont,
                        _tableHeaderPaint);
    }

    private static void WriteCharacters(SKCanvas canvas,
                                        ProfileInfoResponse profileData,
                                        Dictionary<string, SKImage> archetypeImages)
    {
        const float verticalOffsetIncrement = 32f;
        const float levelCellWidth = 32f;
        const float nameCellWidth = 238f;
        const float artisanCellWidth = 212f;

        var verticalCharacterOffset = 585f;
        foreach (var characterData in profileData.Characters.OrderBy(m => m.Order))
        {
            var adventurerLevel = GetTextAndOffsetForCenteredText(characterData.AdventurerLevel.ToString(),
                                                                  levelCellWidth,
                                                                  _tableContentPaint);
            canvas.DrawText(adventurerLevel.Text,
                            55f + adventurerLevel.Offset,
                            verticalCharacterOffset,
                            _tableContentFont,
                            _tableContentPaint);
            var characterName = GetTextAndOffsetForCenteredText(characterData.CharacterName,
                                                                nameCellWidth,
                                                                _tableContentPaint);
            canvas.DrawText(characterName.Text,
                            94f + characterName.Offset,
                            verticalCharacterOffset,
                            _tableContentFont,
                            _tableContentPaint);

            canvas.DrawImage(archetypeImages[characterData.PrimaryArchetype],
                             336f,
                             verticalCharacterOffset - 23f);

            canvas.DrawImage(archetypeImages[characterData.SecondaryArchetype],
                             373f,
                             verticalCharacterOffset - 23f);

            if (characterData.PrimaryProfession is not null && characterData.PrimaryProfessionLevel is not null)
            {
                var profession = GetTextAndOffsetForCenteredText(characterData.PrimaryProfession,
                                                                 artisanCellWidth,
                                                                 _tableContentPaint);
                canvas.DrawText(profession.Text,
                                415f + profession.Offset,
                                verticalCharacterOffset,
                                _tableContentFont,
                                _tableContentPaint);
                var professionLevel = GetTextAndOffsetForCenteredText(characterData.PrimaryProfessionLevel.ToString()!,
                                                                      levelCellWidth,
                                                                      _tableContentPaint);
                canvas.DrawText(professionLevel.Text,
                                636f + professionLevel.Offset,
                                verticalCharacterOffset,
                                _tableContentFont,
                                _tableContentPaint);
            }

            if (characterData.SecondaryProfession is not null && characterData.SecondaryProfessionLevel is not null)
            {
                var profession = GetTextAndOffsetForCenteredText(characterData.SecondaryProfession,
                                                                 artisanCellWidth,
                                                                 _tableContentPaint);
                canvas.DrawText(profession.Text,
                                682f + profession.Offset,
                                verticalCharacterOffset,
                                _tableContentFont,
                                _tableContentPaint);
                var professionLevel = GetTextAndOffsetForCenteredText(characterData.SecondaryProfessionLevel.ToString()!,
                                                                      levelCellWidth,
                                                                      _tableContentPaint);
                canvas.DrawText(professionLevel.Text,
                                909f + professionLevel.Offset,
                                verticalCharacterOffset,
                                _tableContentFont,
                                _tableContentPaint);
            }

            // Increase vertical offset for next character
            verticalCharacterOffset += verticalOffsetIncrement;
        }
    }

    private static (string Text, float Offset) GetTextAndOffsetForCenteredText(string text,
                                                                               float maxWidth,
                                                                               SKPaint paint)
    {
        const float padding = 2f;
        SKRect nameSize = new();
        paint.MeasureText(text, ref nameSize);

        // If actual width fits in maxWidth minus padding on both sides, get the left offset.
        if (nameSize.Width <= maxWidth - 2 * padding)
        {
            var offset = (maxWidth - nameSize.Width) / 2f;
            return (text, offset);
        }

        // If actual width does not fit, shorten text until it fits.
        var shortenedText = text;
        while (nameSize.Width > maxWidth - 2 * padding)
        {
            shortenedText = shortenedText[..^1];
            paint.MeasureText(shortenedText + "...", ref nameSize);
        }
        return (shortenedText + "...", padding);
    }

    private static SKRect ShrinkFontAndPaintToFitSize(string text,
                                                      SKFont font,
                                                      SKPaint paint,
                                                      float maxWidthInPixel,
                                                      float maxHeightInPixel)
    {
        SKRect size = new();
        do
        {
            paint.MeasureText(text, ref size);
            if (size.Width > maxWidthInPixel)
            {
                font.Size--; // Reduce font size for drawing.
                paint.TextSize--; // Reduce paint size for measuring.
            }
            else if (size.Height > maxHeightInPixel)
            {
                font.Size--; // Reduce font size for drawing.
                paint.TextSize--; // Reduce paint size for measuring.
            }
        } while (size.Width > maxWidthInPixel || size.Height > maxHeightInPixel);

        return size;
    }
}