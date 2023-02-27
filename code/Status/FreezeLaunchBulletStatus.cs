using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

[Status(1, 0, 0.75f)]
public class FreezeLaunchBulletStatus : Status
{
	public FreezeLaunchBulletStatus()
    {
		Title = "Best Served Cold";
		IconPath = "textures/icons/freeze_launch_bullet.png";
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
		return string.Format("Shoot 1 time when you freeze an enemy");
	}

	public override string GetUpgradeDescription(int newLevel)
    {
		return GetDescription(newLevel);
	}

	public override void OnFreeze(Enemy enemy)
	{
		Player.Shoot();
	}
}
