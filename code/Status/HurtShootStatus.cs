using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(3, 0, 0.75f)]
public class HurtShootStatus : Status
{
	public HurtShootStatus()
    {
		Title = "Rage Engine";
		IconPath = "textures/icons/hurt_shoot.png";
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
        return string.Format("{0}% chance to shoot when you take damage", GetPercentForLevel(Level));
    }

    public override string GetUpgradeDescription(int newLevel)
    {
        return newLevel > 1 ? string.Format("{0}% → {1}% chance to shoot when you take damage", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
    }

    public override void OnHurt(float amount)
    {
        if (Sandbox.Game.Random.Float(0f, 1f) < GetChanceForLevel(Level))
        {
            Player.Shoot();
        }
    }

    public float GetChanceForLevel(int level)
    {
        return level * 0.33f;
    }
    public float GetPercentForLevel(int level)
    {
        return level * 33;
    }
}
