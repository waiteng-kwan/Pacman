using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatGhostDot : BasicDot
{
    protected override void ResolveCollideWithPlayer(PlayerBehaviour player)
    {
        player.SetPlayerEatGhostState(true);
    }
}
