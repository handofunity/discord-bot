namespace HoU.GuildBot.Shared.Enums;

public enum SpamCheckResult
{
    NoSpam = 0,
    SoftLimitHit = 1,
    BetweenSoftAndHardLimit = 2,
    HardLimitHit = 3
}