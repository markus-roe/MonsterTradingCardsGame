using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services.Interfaces;
using System.Collections.Concurrent;

/// <summary>
/// The LobbyService class manages the lobby for users waiting to engage in battles.
/// It uses concurrent queues to handle multiple users and ensures thread-safe operations.
/// </summary>
public class LobbyService
{
    // Queue for users waiting to participate in a battle
    private ConcurrentQueue<User> waitingUsers = new ConcurrentQueue<User>();

    // Dictionary to store the tasks for waiting users, allowing asynchronous operations
    private readonly Dictionary<User, TaskCompletionSource<string>> waitingTasks = new Dictionary<User, TaskCompletionSource<string>>();

    // Battle service to initiate and manage battles
    private readonly IBattleService _battleService;

    public LobbyService(IBattleService battleService)
    {
        _battleService = battleService;
    }

    /// <summary>
    /// Users enter the lobby and are queued for battle. The method returns a task that completes when the user's battle is ready.
    /// </summary>
    /// <param name="user">The user entering the lobby.</param>
    /// <returns>A Task representing the asynchronous operation, containing the battle log as a string.</returns>
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

    /// <summary>
    /// Checks if there are enough users in the lobby to start a battle. If so, it initiates the battle and resolves the tasks with the battle log.
    /// </summary>
    private void CheckAndStartBattle()
    {
        while (waitingUsers.Count >= 2)
        {
            if (waitingUsers.TryDequeue(out var user1) && waitingUsers.TryDequeue(out var user2))
            {
                lock (waitingTasks)
                {
                    var battleLog = _battleService.StartBattle(user1, user2);

                    waitingTasks[user1].SetResult(battleLog);
                    waitingTasks[user2].SetResult(battleLog);
                    waitingTasks.Remove(user1);
                    waitingTasks.Remove(user2);
                }
            }
        }
    }
}
