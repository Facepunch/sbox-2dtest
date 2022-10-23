using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(9, 0, 1f)]
public class DashCooldownStatus : Status
{
	public DashCooldownStatus()
    {
		Title = "Quick Lungs";
		IconPath = "textures/icons/dash_cooldown.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);

		Player.Modify(this, nameof(Player.DashCooldown), GetMultForLevel(Level), ModifierType.Mult);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Dash cooldown is {0}% faster", GetPercentForLevel(Level));
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return newLevel > 1 ? string.Format("Dash cooldown is {0}% → {1}% faster", GetPercentForLevel(newLevel - 1), GetPercentForLevel(newLevel)) : GetDescription(newLevel);
	}

	public float GetMultForLevel(int level)
    {
		switch (level)
        {
			case 1:
				return 0.85f;
			case 2:
				return 0.75f;
			case 3:
				return 0.65f;
			case 4:
				return 0.55f;
			case 5:
				return 0.45f;
			case 6:
				return 0.4f;
			case 7:
				return 0.35f;
			case 8:
				return 0.3f;
			case 9:
				return 0.25f;
		}

		return 1f;
    }

	public float GetPercentForLevel(int level)
	{
		switch (level)
		{
			case 1:
				return 15;
			case 2:
				return 25;
			case 3:
				return 35;
			case 4:
				return 45;
			case 5:
				return 55;
			case 6:
				return 60;
			case 7:
				return 65;
			case 8:
				return 70;
			case 9:
				return 75;
		}

		return 0f;
	}
}
