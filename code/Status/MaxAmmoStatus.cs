using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(7, 0, 1f)]
public class MaxAmmoStatus : Status
{
	public MaxAmmoStatus()
    {
		Title = "Bigger Clip";
		IconPath = "textures/icons/max_ammo.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.MaxAmmoCount, GetAdditionalAmmoCountForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase max ammo by {0}", GetAdditionalAmmoCountForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase max ammo by {0}→{1}", GetAdditionalAmmoCountForLevel(newLevel - 1), GetAdditionalAmmoCountForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetAdditionalAmmoCountForLevel(int level)
	{
		return level;
	}
}
