using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdiblesEatGhost : EdiblesBase
{
    protected override void ResolveCollideWithPlayer(PlayerBehaviour player)
    {
        player.SetPlayerEatGhostState(true);
    }
}
