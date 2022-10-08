using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Test2D;

public class MovespeedStatus : Status
{
	float _mult;

	public MovespeedStatus()
    {
		Title = "Fast Shoes";
		Description = "Increase move speed";
		IconPath = "textures/icons/shoe.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);
	}

	public override void Refresh()
    {
		_mult = 1f + 0.2f * Level;
		Description = "Increase move speed by " + _mult.ToString("#.#") + "x.";

		Player.Modify(this, "MoveSpeed", _mult, ModifierType.Mult);
	}
}
