using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
namespace Test2D;

[Status(7, 0, 1f)]
public class XpDamage : Status
{
	public XpDamage()
    {
		Title = "XP Shrapnel";
		IconPath = "textures/icons/xp_damage.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("When you gain XP, hurt nearby enemies for {0} per point", GetAmountForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
	{
		return newLevel > 1 ? string.Format("When you gain XP, hurt nearby enemies for {0}→{1} per point", GetAmountForLevel(newLevel - 1), GetAmountForLevel(newLevel)) : GetDescription(newLevel);
	}

    public override void OnGainExperience(int xp)
    {
        List<Thing> nearbyThings = new List<Thing>();

        for (int dx = -2; dx <= 2; dx++)
            for (int dy = -2; dy <= 2; dy++)
                Player.Game.AddThingsInGridSquare(new MyGame.GridSquare(Player.GridPos.x + dx, Player.GridPos.y + dy), nearbyThings);

        float radius = 2f;
        float damage = xp * GetAmountForLevel(Level) * Player.GetDamageMultiplier();

        // todo:
        Player.Game.PlaySfxNearby("shoot", Player.Position, pitch: Utils.Map(xp, 1, 16, 0.6f, 1.4f), volume: Utils.Map(xp, 1, 16, 0.4f, 1f), maxDist: 3f);

        foreach (Thing thing in nearbyThings)
        {
            if (thing == Player)
                continue;

            if (thing is Enemy enemy && !enemy.IsDying && (!enemy.IsSpawning || enemy.ElapsedTime > 0.75f))
            {
                var dist_sqr = (thing.Position - Player.Position).LengthSquared;
                if (dist_sqr < MathF.Pow(radius, 2f))
                {
                    enemy.Damage(damage, Player, false);
                }
            }
        }
    }

    public float GetAmountForLevel(int level)
	{
		return level * 0.85f;
	}

    public string GetPrintAmountForLevel(int level)
    {
        return string.Format("{0:0.00}", GetAmountForLevel(level));
    }
}
