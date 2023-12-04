using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;
using System.Collections.Concurrent;

public class LobbyService
{
    private ConcurrentQueue<User> waitingUsers = new ConcurrentQueue<User>();
    private readonly Dictionary<User, TaskCompletionSource<string>> waitingTasks = new Dictionary<User, TaskCompletionSource<string>>();
    private readonly IBattleService battleService;

    public LobbyService(IBattleService battleService)
    {
        this.battleService = battleService;
    }

    public Task<string> EnterLobbyAsync(User user)
    {
        var tcs = new TaskCompletionSource<string>();
        lock (waitingTasks)
        {
            waitingTasks[user] = tcs;
        }
        waitingUsers.Enqueue(user);
        CheckAndStartBattle();
        return tcs.Task;
    }

    private void CheckAndStartBattle()
    {
        lock (waitingTasks)
        {
            while (waitingUsers.Count >= 2)
            {
                if (waitingUsers.TryDequeue(out var user1) && waitingUsers.TryDequeue(out var user2))
                {
                    var battleLog = battleService.StartBattle(user1, user2);
                    waitingTasks[user1].SetResult(battleLog);
                    waitingTasks[user2].SetResult(battleLog);
                    waitingTasks.Remove(user1);
                    waitingTasks.Remove(user2);
                }
            }
        }
    }
}
