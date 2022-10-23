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

		Player.Modify(this, nameof(Player.NumProjectiles), GetNumProjectilesForLevel(Level), ModifierType.Add);
		Player.Modify(this, nameof(Player.AttackSpeed), GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase num projectiles by {0} but reduce attack speed by {1}%", GetNumProjectilesForLevel(Level), GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase num projectiles by {0} → {1} but reduce attack speed by {2}% → {3}%", GetNumProjectilesForLevel(newLevel - 1), GetNumProjectilesForLevel(newLevel), GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetNumProjectilesForLevel(int level)
	{
		return level;
	}

	public float GetMultForLevel(int level)
	{
		return 1f - 0.2f * level;
	}

	public float GetPercentForLevel(int level)
	{
		return 20 * level;
	}
}
