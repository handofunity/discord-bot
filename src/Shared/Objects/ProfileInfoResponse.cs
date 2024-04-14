namespace HoU.GuildBot.Shared.Objects;

public record ProfileInfoResponse( decimal SeasonalTokens,
                                   long SeasonalRank,
                                   string SeasonalRankName,
                                   string? SeasonalRankPercentageRange,
                                   ProfileCharacterData[] Characters);