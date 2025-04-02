using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class TaskUtils
{
    private static readonly List<CancellationTokenSource> _activeTokens = new();

    public static async Task DelayedSwap(float delay, CancellationTokenSource tokenSource)
    {
        _activeTokens.Add(tokenSource);

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(delay), tokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            Debug.Log("Task was cancelled.");
        }
        finally
        {
            _activeTokens.Remove(tokenSource);
            tokenSource.Dispose();
        }
    }
    
    public static void CancelAll()
    {
        var tokensCopy = _activeTokens.ToList();

        foreach (var token in tokensCopy)
        {
            token.Cancel();
            token.Dispose();
            _activeTokens.Remove(token);
        }
    }

}