using Godot;
using System.Collections.Generic;
using System.Linq;

public class ScoreTally
{
    public List<ScoreResult> DeterminePlacements(IReadOnlyList<Player> players, FarmManager farmManager)
    {
        if (farmManager == null) return new();

        Dictionary<int, Player> playerLookup = players.ToDictionary(p => p.PlayerId);

        return farmManager
            .GetAllPlantScores()
            .OrderByDescending(x => x.Value)
            .Select(pair =>
            {
                playerLookup.TryGetValue(pair.Key, out Player player);

                return new ScoreResult
                {
                    PlayerId = pair.Key,
                    PlayerName = player?.Name ?? $"Player {pair.Key}",
                    CharacterIndex = player?.CharacterIndex ?? -1,
                    Score = pair.Value
                };
            }).ToList();
    }

    public ScoreResult GetWinner(IReadOnlyList<ScoreResult> results)
    {
        if (results == null || results.Count == 0) return null;

        ScoreResult first = results[0];
        if (results.Count > 1 && results[1].Score == first.Score)
            return null; // tie

        return first;
    }
}