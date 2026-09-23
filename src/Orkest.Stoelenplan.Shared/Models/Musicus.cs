namespace Orkest.Stoelenplan.Shared.Models;

public sealed class Musicus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Naam { get; set; } = "";
    public Sectie Sectie { get; set; }
}
