using System.Numerics;
using Sensum.Framework.Entities;
using Sensum.Framework.Growtopia.Entities.Enums;
using Sensum.Framework.Growtopia.Entities.Structs;
using Sensum.Framework.Growtopia.Network;
using Sensum.Framework.Utils;

namespace Sensum.Framework.Growtopia.Player;

public unsafe class NetAvatar
{
    public uint UserId;
    public int NetId;

    public string? Name;
    public byte Level;

    public Vector2 Pos;
    public Vector2Int Velocity;

    public IconState IconState;
    public VisualState VisualState;

    public bool IsBot;
    public bool IsMod;
    public bool IsLocal;
    public bool IsSuperMod;
    public bool IsInvisible;
    public bool IsFacingLeft => VisualState is VisualState.PlaceLeft or VisualState.PunchLeft or VisualState.StandingLeft;

    public Vector2Int TilePos => new((int)(Pos.X / 32), (int)(Pos.Y / 32));
    public string FixedName => Name == null ? "" : Name.Substring(2, Name.Length - 4);
    public bool IsNameAdmin => Name != null && Name.StartsWith("`^");
    public bool IsNameOwner => Name != null && Name.StartsWith("`2");
    public bool IsNameMod => Name != null && Name.StartsWith("`#@");
    public bool IsNameDev => Name != null && Name.StartsWith("`6@");

    public void UpdateState(GameUpdatePacket* packet)
    {
        VisualState = (VisualState)packet->CharacterState;
        Pos = packet->WorldPos;
        Velocity = packet->Velocity;
    }

    public void OnSetIconState(IconState state, ENetClient client)
    {
        IconState = state;
        if (IconState == IconState.Chat && client.FeatureFlags.HasFlag(ClientFeatureFlags.BotDetection))
            client.BotDetector.IncreaseChatIconCount(this);
    }
}