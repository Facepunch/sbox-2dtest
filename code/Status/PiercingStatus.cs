using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

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

		Player.Modify(this, "BulletNumPiercing", GetNumPiercingForLevel(Level), ModifierType.Add);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Increase bullet pierces by {0}", GetNumPiercingForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Increase bullet pierces by {0} → {1}", GetNumPiercingForLevel(newLevel - 1), GetNumPiercingForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetNumPiercingForLevel(int level)
	{
		return level;
	}
}
