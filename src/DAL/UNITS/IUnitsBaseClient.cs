namespace HoU.GuildBot.DAL.UNITS;

public interface IUnitsBaseClient
{
    string BaseUrl { get; set; }
    
    AuthorizationEndpoint AuthorizationEndpoint { set; }
    
    IBearerTokenManager BearerTokenManager { set; }
}