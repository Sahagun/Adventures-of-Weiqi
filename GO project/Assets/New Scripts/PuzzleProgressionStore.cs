using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class PuzzleProgressionStore
{
    private const string KeyPrefix = "PuzzlePoolProgression";
    private const string UnlockSuffix = "Unlocked";
    private const string BestSolveRateSuffix = "BestSolveRate";
    private const string BestSolvedCountSuffix = "BestSolvedCount";
    private const string BestPuzzleCountSuffix = "BestPuzzleCount";

    public static bool IsPoolUnlocked(string poolId,bool unlockedByDefault = false)
    {
        string normalizedPoolId = NormalizePoolId(poolId);
        if (string.IsNullOrEmpty(normalizedPoolId))
            return unlockedByDefault;

        return PlayerPrefs.GetInt(BuildKey(normalizedPoolId,UnlockSuffix),unlockedByDefault ? 1 : 0) == 1;
    }

    public static void SetPoolUnlocked(string poolId,bool isUnlocked = true)
    {
        string normalizedPoolId = NormalizePoolId(poolId);
        if (string.IsNullOrEmpty(normalizedPoolId))
            return;

        PlayerPrefs.SetInt(BuildKey(normalizedPoolId,UnlockSuffix),isUnlocked ? 1 : 0);
    }

    public static void SavePoolResult(string poolId,int solvedCount,int totalPuzzles)
    {
        string normalizedPoolId = NormalizePoolId(poolId);
        if (string.IsNullOrEmpty(normalizedPoolId) || totalPuzzles <= 0)
            return;

        float solveRate = Mathf.Clamp01((float)solvedCount / totalPuzzles);
        float bestSolveRate = PlayerPrefs.GetFloat(BuildKey(normalizedPoolId,BestSolveRateSuffix),-1f);
        int bestSolvedCount = PlayerPrefs.GetInt(BuildKey(normalizedPoolId,BestSolvedCountSuffix),-1);

        bool shouldUpdateBest =
            solveRate > bestSolveRate ||
            (Mathf.Approximately(solveRate,bestSolveRate) && solvedCount > bestSolvedCount);

        if (!shouldUpdateBest)
            return;

        PlayerPrefs.SetFloat(BuildKey(normalizedPoolId,BestSolveRateSuffix),solveRate);
        PlayerPrefs.SetInt(BuildKey(normalizedPoolId,BestSolvedCountSuffix),solvedCount);
        PlayerPrefs.SetInt(BuildKey(normalizedPoolId,BestPuzzleCountSuffix),totalPuzzles);
    }

    public static void ClearPool(string poolId)
    {
        string normalizedPoolId = NormalizePoolId(poolId);
        if (string.IsNullOrEmpty(normalizedPoolId))
            return;

        PlayerPrefs.DeleteKey(BuildKey(normalizedPoolId,UnlockSuffix));
        PlayerPrefs.DeleteKey(BuildKey(normalizedPoolId,BestSolveRateSuffix));
        PlayerPrefs.DeleteKey(BuildKey(normalizedPoolId,BestSolvedCountSuffix));
        PlayerPrefs.DeleteKey(BuildKey(normalizedPoolId,BestPuzzleCountSuffix));
    }

    public static void ClearPools(IEnumerable<string> poolIds)
    {
        if (poolIds == null)
            return;

        foreach (string poolId in poolIds)
        {
            ClearPool(poolId);
        }
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    private static string BuildKey(string normalizedPoolId,string suffix)
    {
        return $"{KeyPrefix}.{normalizedPoolId}.{suffix}";
    }

    private static string NormalizePoolId(string poolId)
    {
        if (string.IsNullOrWhiteSpace(poolId))
            return string.Empty;

        StringBuilder builder = new StringBuilder(poolId.Length);
        string trimmedPoolId = poolId.Trim().ToLowerInvariant();

        foreach (char character in trimmedPoolId)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        }

        return builder.ToString().Trim('_');
    }
}
