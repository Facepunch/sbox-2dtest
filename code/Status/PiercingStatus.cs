using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(9, 0, 1f)]
public class PiercingStatus : Status
{
	public PiercingStatus()
    {
		Title = "Piercing";
		IconPath = "textures/icons/piercing.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, nameof(Player.BulletNumPiercing), GetNumPiercingForLevel(Level), ModifierType.Add);
		Player.Modify(this, nameof(Player.BulletDamage), GetDamageMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase bullet pierces by {0} but reduce damage by {1}%", GetNumPiercingForLevel(Level), GetDamagePercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase bullet pierces by {0}→{1} but reduce damage by {2}%→{3}%", GetNumPiercingForLevel(newLevel - 1), GetNumPiercingForLevel(newLevel), GetDamagePercentForLevel(newLevel - 1), GetDamagePercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetNumPiercingForLevel(int level)
	{
		return level;
	}

	public float GetDamageMultForLevel(int level)
	{
		return 1f - 0.2f * level;
	}

	public float GetDamagePercentForLevel(int level)
	{
		return 20 * level;
	}
}
