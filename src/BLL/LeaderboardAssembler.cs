namespace HoU.GuildBot.BLL;

internal static class LeaderboardAssembler
{
    private static readonly SKColor _textColor = SKColors.Black;
    private static readonly SKTypeface _seasonTypeface = SKTypeface.FromFamilyName("Linux Libertine");
    private static readonly SKTypeface _userTypeface = SKTypeface.FromFamilyName("Marcellus");
    private static readonly SKFont _seasonFont = new(_seasonTypeface, size: 32f);
    private static readonly SKPaint _seasonPaint = new(_seasonFont) { Color = _textColor };
    private static readonly SKFont _userFont = new(_userTypeface, size: 24f);
    private static readonly SKPaint _userPaint = new(_userFont) { Color = _textColor };

    internal static void AssembleLeaderboard(SKBitmap bitmap,
                                             SKImage backgroundImage,
                                             SKImage foregroundImage,
                                             DiscordLeaderboardResponse leaderboardData,
                                             Dictionary<DiscordUserId, string> userDisplayNames)
    {
        using var canvas = new SKCanvas(bitmap);

        canvas.DrawImage(backgroundImage, 0, 0);
        canvas.DrawImage(foregroundImage, 0, 0);
        if (leaderboardData.Season is not null)
            WriteSeasonName(canvas, leaderboardData.Season);
        WriteLeaderboardPositions(canvas, leaderboardData.LeaderboardPositions, userDisplayNames);
    }

    private static void WriteSeasonName(SKCanvas canvas,
                                        string season)
    {
        canvas.DrawText(season,
                        804f,
                        60f,
                        _seasonFont,
                        _seasonPaint);
    }

    private static void WriteLeaderboardPositions(SKCanvas canvas,
                                                  List<DiscordLeaderboardPositionModel> leaderboardPositions,
                                                  Dictionary<DiscordUserId, string> userDisplayNames)
    {
        const float horizontalOffsetIncrement = 485f;
        const float horizontalNameOffset = 45f;
        const float verticalTopRowOffset = 148f;
        const float verticalOffsetIncrement = 47f;
        float horizontalOffset = 40f;
        float horizontalRightOffset = 475f;
        float verticalOffset = 0f;

        foreach (var position in leaderboardPositions)
        {
            if (position.Rank == 1 || position.Rank == 11)
            {
                verticalOffset = verticalTopRowOffset;
                if (position.Rank == 11)
                {
                    horizontalOffset += horizontalOffsetIncrement;
                    horizontalRightOffset += horizontalOffsetIncrement;
                }
            }
            else
            {
                verticalOffset += verticalOffsetIncrement;
            }

            var displayName = userDisplayNames.TryGetValue((DiscordUserId)position.DiscordUserId, out var foundName)
                ? foundName
                : "N/A";
            if (displayName.Length > 22)
                displayName = displayName[..22];
            canvas.DrawText(position.Rank.ToString("D2"),
                            horizontalOffset,
                            verticalOffset,
                            _userFont,
                            _userPaint);
            canvas.DrawText(displayName,
                            horizontalOffset + horizontalNameOffset,
                            verticalOffset,
                            _userFont,
                            _userPaint);

            var tokensText = position.Tokens.ToString();
            var requiredSizeForTokens = new SKRect();
            _userPaint.MeasureText(tokensText, ref requiredSizeForTokens);
            canvas.DrawText(tokensText,
                            horizontalRightOffset - requiredSizeForTokens.Width,
                            verticalOffset,
                            _userFont,
                            _userPaint);
        }
    }
}
