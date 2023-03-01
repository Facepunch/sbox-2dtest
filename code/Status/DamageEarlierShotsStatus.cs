using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 111f)]
public class DamageEarlierShotsStatus : Status
{
	public DamageEarlierShotsStatus()
    {
		Title = "Build Up";
		IconPath = "textures/icons/blank_icon.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

        Player.Modify(this, PlayerStat.DamagePerEarlierShot, GetAmountForLevel(Level), ModifierType.Add);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Bullets deal {0} more damage for each time you've shot (reset when you reload)", GetPrintAmountForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
	{
		return newLevel > 1 ? string.Format("Bullets deal {0} → {1} more damage for each time you've shot (reset when you reload)", GetPrintAmountForLevel(newLevel - 1), GetPrintAmountForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAmountForLevel(int level)
	{
		return level * 0.25f;
	}

    public string GetPrintAmountForLevel(int level)
    {
        return GetAmountForLevel(level).ToString("#.##");
    }
}
