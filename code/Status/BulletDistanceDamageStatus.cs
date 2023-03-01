using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class BulletDistanceDamageStatus : Status
{
	public BulletDistanceDamageStatus()
    {
		Title = "Sniper Training";
		IconPath = "textures/icons/bullet_distance_damage.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.BulletDistanceDamage, GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase bullet damage by {0} per metre traveled", GetPrintAmountForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase bullet damage by {0} → {1} per metre traveled", GetPrintAmountForLevel(newLevel - 1), GetPrintAmountForLevel(newLevel)) : GetDescription(newLevel);
	}

    public float GetAddForLevel(int level)
    {
        return 1f * level;
    }

    public string GetPrintAmountForLevel(int level)
    {
        return GetAddForLevel(level).ToString("#.##");
    }
}
