using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(4, 0, 1f)]
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

		Player.Modify(this, PlayerStat.NumProjectiles, GetNumProjectilesForLevel(Level), ModifierType.Add);
		Player.Modify(this, PlayerStat.AttackSpeed, GetAttackSpeedMultForLevel(Level), ModifierType.Mult);
        Player.Modify(this, PlayerStat.MoveSpeed, GetMoveSpeedMultForLevel(Level), ModifierType.Mult);
    }

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase num projectiles by {0} but reduce attack speed by {1}% and reduce move speed by {2}%", GetNumProjectilesForLevel(Level), GetAttackSpeedPercentForLevel(Level), GetMoveSpeedPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase num projectiles by {0} → {1} but reduce attack speed by {2}% → {3}% and reduce move speed by {4}% → {5}%", GetNumProjectilesForLevel(newLevel - 1), GetNumProjectilesForLevel(newLevel), GetAttackSpeedPercentForLevel(newLevel - 1), GetAttackSpeedPercentForLevel(newLevel), GetMoveSpeedPercentForLevel(newLevel - 1), GetMoveSpeedPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetNumProjectilesForLevel(int level) { return level; }
	public float GetAttackSpeedMultForLevel(int level) { return 1f - 0.22f * level; }
	public float GetAttackSpeedPercentForLevel(int level) { return 22 * level; }
    public float GetMoveSpeedMultForLevel(int level) { return 1f - 0.1f * level; }
    public float GetMoveSpeedPercentForLevel(int level) { return 10 * level; }
}
