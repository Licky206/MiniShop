namespace MiniShop.Api.Auth;

/// <summary>Pravilo "korisnik je vlasnik resursa ili Admin". Nema parametara.</summary>
public sealed class SameOwnerRequirement : IAuthorizationRequirement { }