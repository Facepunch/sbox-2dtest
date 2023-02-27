using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(3, 0, 1f)]
public class NumDashesStatus : Status
{
	public NumDashesStatus()
    {
		Title = "More Dashes";
		IconPath = "textures/icons/num_dashes.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, PlayerStat.NumDashes, GetNumDashesForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase num dashes by {0}", GetNumDashesForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase num dashes by {0} → {1}", GetNumDashesForLevel(newLevel - 1), GetNumDashesForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetNumDashesForLevel(int level)
	{
		return level;
	}
}
