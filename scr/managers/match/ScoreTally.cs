using Godot;
using System.Collections.Generic;
using System.Linq;

public class ScoreTally
{
    public List<ScoreResult> DeterminePlacements(
        List<Player> players,
        FarmManager farmManager)
    {
        if (farmManager == null)
            return new List<ScoreResult>();

        Dictionary<int, int> playerScores = farmManager.GetAllPlantScores();
        var placements = playerScores.OrderByDescending(p => p.Value);

        List<ScoreResult> results = new();
        foreach (var pair in placements)
        {
            var player = players.FirstOrDefault(
                    p => p.PlayerId == pair.Key
            );

            results.Add(new ScoreResult
            {
                PlayerIndex = pair.Key,
                PlayerName = player != null
                    ? player.Name
                    : $"Player {pair.Key}",
                Score = pair.Value
            });
        }
        return results;
    }
}