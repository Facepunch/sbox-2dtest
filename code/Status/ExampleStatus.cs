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
		player.Modify(this, "AttackSpeed", 4f, ModifierType.Set);
	}

	public override void Update(float dt)
	{
		base.Update(dt);
	}

	public override void Remove()
	{
		base.Remove();
	}
}

public class ExampleStatus2 : Status
{
	public override void Init(PlayerCitizen player)
	{
		base.Init(player);

		player.Modify(this, "AttackSpeed", 6f, ModifierType.Mult);
		player.Modify(this, "ReloadSpeed", 6f, ModifierType.Mult);
	}

	public override void Update(float dt)
	{
		base.Update(dt);
	}

	public override void Remove()
	{
		base.Remove();
	}
}
