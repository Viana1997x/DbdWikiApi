namespace DbdWikiApi.Models;

public class CharacterBuild
{
    public string CharacterName { get; set; } = null!;
    public List<string> Perks { get; set; } = new List<string>(4);
}