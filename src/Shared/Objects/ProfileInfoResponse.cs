namespace HoU.GuildBot.Shared.Objects;

public record ProfileInfoResponse( decimal SeasonalTokens,
                                   long SeasonalRank,
                                   string SeasonalRankName,
                                   ProfileCharacterData[] Characters);