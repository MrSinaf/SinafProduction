namespace SinafProduction.Models;

public record class User(ulong id, string name, string password, bool isAdmin, int version);