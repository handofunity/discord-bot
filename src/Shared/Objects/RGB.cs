namespace HoU.GuildBot.Shared.Objects;

public struct RGB
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Properties

    public byte R { get; }

    public byte G { get; }

    public byte B { get; }

    #endregion

    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Constructors

    public RGB(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    #endregion
}