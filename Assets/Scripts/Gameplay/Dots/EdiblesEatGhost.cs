namespace Game
{
    public class EdiblesEatGhost : EdiblesBase
    {
        protected override void ResolveCollideWithPlayer(PacmanBehaviour player)
        {
            player.SetPlayerEatGhostState(true);
        }
    }
}