using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TaskDelayService
{
    private readonly Dictionary<DelayedEntityEnum, List<CancellationTokenSource>> _activeTokens = new();

    public async Task DelayedSwap(DelayedEntityEnum delayedEntity,float delay, CancellationTokenSource tokenSource)
    {
        if (!_activeTokens.TryGetValue(delayedEntity, out var tokens))
        {
            tokens = new List<CancellationTokenSource>();
            _activeTokens[delayedEntity] = tokens;
        }
        tokens.Add(tokenSource);

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
            _activeTokens[delayedEntity].Remove(tokenSource);
            tokenSource.Dispose();
        }
    }
    
    public void CancelEntity(DelayedEntityEnum delayedEntity)
    {
        if(!_activeTokens.TryGetValue(delayedEntity, out var activeToken))return;
        
        var tokensCopy = activeToken.ToList();

        foreach (var token in tokensCopy)
        {
            token.Cancel();
            token.Dispose();
            _activeTokens[delayedEntity].Remove(token);
        }
    }
    
    public enum DelayedEntityEnum
    {
        BlocksMovement,
        Balloon
    }

}