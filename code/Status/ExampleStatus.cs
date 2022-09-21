using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.MyGame;

namespace Sandbox;

public class ExampleStatus : Status
{
	public override void Init(PlayerCitizen player)
	{
		base.Init(player);

		player.Modify(this, "AttackSpeed", 0.01f, ModifierType.Add);
	}

	public override void Update(float dt)
	{

	}

	public override void Remove()
	{

	}
}

public class ExampleStatus2 : Status
{
	public override void Init(PlayerCitizen player)
	{
		base.Init(player);

		player.Modify(this, "AttackSpeed", 2f, ModifierType.Mult);
	}

	public override void Update(float dt)
	{

	}

	public override void Remove()
	{

	}
}
