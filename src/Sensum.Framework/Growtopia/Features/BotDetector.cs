using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Player;

namespace Sensum.Framework.Growtopia.Features;

public class BotDetector : IResourceLifecycle
{
    private readonly LinkedList<PlayerData> players = [];

    public void IncreaseChatIconCount(NetAvatar netAvatar)
    {
        var player = addIfDoesNotExist(netAvatar);
        player.ChatIconCount++;
    }

    public void AppendChatMessage(NetAvatar netAvatar, string message)
    {
        var player = addIfDoesNotExist(netAvatar);
        if (netAvatar.IsBot && player.CheckingMessages) return;
        player.Messages.Add(message);
        if (player.Messages.Count > 5 && player.CheckingMessages == false)
        {
            player.CheckingMessages = true;
            new Thread(() => checkPlayerMessages(player)).Start();
        }
    }


    private void checkPlayerMessages(PlayerData playerData)
    {
        int avgLevenshteinDistance = 0;
        for (int i = 0; i < playerData.Messages.Count; i++)
        {
            string message = playerData.Messages[i];
            string? nextMessage = i + 1 < playerData.Messages.Count ? playerData.Messages[i + 1] : null;
            if (nextMessage == null) continue;
            avgLevenshteinDistance += Fastenshtein.Levenshtein.Distance(message, nextMessage);
        }
        avgLevenshteinDistance /= playerData.Messages.Count;

        if (avgLevenshteinDistance <= 5)
        {
            playerData.IncreaseHeat(1f);
        }

        playerData.CheckingMessages = false;
        playerData.Messages.Clear();
    }

    private PlayerData addIfDoesNotExist(NetAvatar netAvatar)
    {
        foreach (var player in players)
        {
            if (player.UserId == netAvatar.UserId)
            {
                return player;
            }
        }

        var newPlayer = new PlayerData
        {
            UserId = netAvatar.UserId,
            Heat = 0f,
            ChatIconCount = 0,
            CheckingMessages = false,
            Messages = []
        };
        players.AddLast(newPlayer);
        return newPlayer;
    }

    public void Reset()
    {
        players.Clear();
    }

    public void Destroy()
    {
        Reset();
    }
}

public class PlayerData
{
    public uint UserId;
    public float Heat;
    public int ChatIconCount;
    public bool CheckingMessages;
    public List<string> Messages = null!;
    public void IncreaseHeat(float amount)
    {
        Heat += amount;
        if (Heat > 1f)
        {
            Heat = 1f;
        }
    }
    public void DecreaseHeat(float amount)
    {
        Heat -= amount;
        if (Heat < 0f)
        {
            Heat = 0f;
        }
    }
}