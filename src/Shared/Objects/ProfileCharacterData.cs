namespace HoU.GuildBot.Shared.Objects;

public record ProfileCharacterData(int Order,
                                   short AdventurerLevel,
                                   string CharacterName,
                                   short GearScore,
                                   string PrimaryArchetype,
                                   string SecondaryArchetype,
                                   string? PrimaryProfession,
                                   short? PrimaryProfessionLevel,
                                   string? SecondaryProfession,
                                   short? SecondaryProfessionLevel);