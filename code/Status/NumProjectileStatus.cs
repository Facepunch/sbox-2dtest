using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(9, 0, 1f)]
public class NumProjectileStatus : Status
{
	public NumProjectileStatus()
    {
		Title = "More Bullets";
		IconPath = "textures/icons/more_projectiles.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, "NumBullets", GetNumProjectilesForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase num projectiles by {0}", GetNumProjectilesForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase num projectiles by {0} → {1}", GetNumProjectilesForLevel(newLevel - 1), GetNumProjectilesForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetNumProjectilesForLevel(int level)
	{
		return level;
	}
}
