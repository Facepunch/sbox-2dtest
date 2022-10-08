using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

public class MovespeedStatus : Status
{
	float Mult = 1.2f;

	public MovespeedStatus()
    {
		Title = "Fast Shoes";
		Description = "Increase move speed by " + Mult.ToString("#.#") + "x.";
		IconPath = "textures/icons/shoe.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
		player.Modify(this, "MoveSpeed", Mult, ModifierType.Mult);
	}
}
