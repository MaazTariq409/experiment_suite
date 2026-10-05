namespace Enterprise.Shared.Security;

public sealed record ActorContext(string UserId, string Role, IReadOnlySet<string> Permissions);
