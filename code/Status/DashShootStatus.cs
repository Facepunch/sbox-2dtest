using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(1, 0, 1f)]
public class DashShootStatus : Status
{
	public DashShootStatus()
    {
		Title = "Cheeky Shot";
		IconPath = "textures/icons/dash_shoot.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		Description = GetDescription(Level);
	}

	public override string GetDescription(int newLevel)
	{
		return string.Format("Shoot 1 time when you dash");
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return GetDescription(newLevel);
	}

	public override void OnDashStarted()
	{
		Player.Shoot();
	}
}
