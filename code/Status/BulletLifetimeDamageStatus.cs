using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 111f)]
public class BulletLifetimeDamageStatus : Status
{
	public BulletLifetimeDamageStatus()
    {
		Title = "Growing Bullets";
		IconPath = "textures/icons/blank_icon.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.BulletDamageGrow, GetAddForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Bullets grow their damage by {0} per second", GetPrintAmountForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Bullets grow their damage by {0} → {1} per second", GetPrintAmountForLevel(newLevel - 1), GetPrintAmountForLevel(newLevel)) : GetDescription(newLevel);
	}

    public float GetAddForLevel(int level)
    {
        return 1.25f * level;
    }

    public string GetPrintAmountForLevel(int level)
    {
        return GetAddForLevel(level).ToString("#.#");
    }
}
