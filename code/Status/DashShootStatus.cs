using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class DashShootStatus : Status
{
	public DashShootStatus()
    {
		Title = "Cheeky Shot";
		IconPath = "textures/icons/dash_shoot.png";
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
		return string.Format("Launch {0} bullet when you dash", GetAmountForLevel(Level));
	}

    public override string GetUpgradeDescription(int newLevel)
    {
        return newLevel > 1 ? string.Format("Launch {0} → {1} bullets when you dash", GetAmountForLevel(newLevel - 1), GetAmountForLevel(newLevel)) : GetDescription(newLevel);
    }

	public override void OnDashStarted()
	{
        int maxShardsNum = (int)Player.Stats[PlayerStat.FrozenShardsNum];
        int numShards = Sandbox.Game.Random.Int(1, maxShardsNum);
        Vector2 aimDir = (new Vector2(Sandbox.Game.Random.Float(-1f, 1f), Sandbox.Game.Random.Float(-1f, 1f))).Normal;
        Player.SpawnBulletRing(Player.Position, (int)GetAmountForLevel(Level), Player.DashVelocity.Normal);
	}

    public float GetAmountForLevel(int level)
    {
        return level * 1f;
    }
}
