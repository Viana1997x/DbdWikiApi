using System.Collections.Generic;

namespace DbdWikiApi.Models
{
    public class UpdateFavoritesDto
    {
        public List<CharacterBuild> FavoriteKillers { get; set; } = new();
        public List<CharacterBuild> FavoriteSurvivors { get; set; } = new();
    }
}