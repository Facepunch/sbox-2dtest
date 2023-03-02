using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(4, 0, 111f)]
public class GrenadeShootReloadStatus : Status
{
	public GrenadeShootReloadStatus()
    {
		Title = "Reload Grenade";
		IconPath = "textures/icons/blank_icon.png";
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
		return string.Format("{0}% chance to launch a grenade when you reload", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
	{
		return newLevel > 1 ? string.Format("{0}% → {1}% chance to launch a grenade when you reload", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public override void OnReload()
	{
		if(Sandbox.Game.Random.Float(0f, 1f) < GetChanceForLevel(Level))
        {
            var grenade = new Grenade()
            {
                Position = Player.Position + Player.AimDir * 0.5f,
                ExplosionSizeMultiplier = Player.Stats[PlayerStat.ExplosionSizeMultiplier],
            };
            grenade.Velocity = (grenade.Position - Player.Position) * Player.Stats[PlayerStat.GrenadeVelocity];
            MyGame.Current.AddThing(grenade);

			// todo:
            //MyGame.Current.PlaySfxNearby("ignite", Player.Position, pitch: Sandbox.Game.Random.Float(1.05f, 1.25f), volume: 0.5f, maxDist: 4f);
		}
	}

	public float GetChanceForLevel(int level)
	{
		return level == 4 ? 1f : level * 0.3f;
	}
	public float GetPercentForLevel(int level)
	{
		return level == 4 ? 100 : level * 30;
    }
}
