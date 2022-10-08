using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public class ExampleStatus : Status
{
	float ReloadSpdMultiplier = 1.1f;

	public ExampleStatus()
    {
		Title = "Example 1";
		Description = "Increase reload speed.";
		IconPath = "textures/icons/bullets.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);

		Description = "Multiply RELOAD SPD by " + ReloadSpdMultiplier.ToString("#.#") + "x.";

		//player.Modify(this, "AttackSpeed", 0.01f, ModifierType.Add);
		//player.Modify(this, "AttackSpeed", 4f, ModifierType.Set);
		//player.Modify(this, "MoveSpeed", 5f, ModifierType.Mult);
		//player.Modify(this, "AttackSpeed", 2f, ModifierType.Mult);
		player.Modify(this, "ReloadSpeed", 1.1f, ModifierType.Mult);
		player.Modify(this, "BulletInaccuracy", 0.5f, ModifierType.Mult);
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
	float AtkSpdMultiplier = 1.1f;

	public ExampleStatus2()
	{
		Title = "Example Status 2";
		Description = "Increase attack speed.";
		IconPath = "textures/icons/background.png";
	}

	public override void Init(PlayerCitizen player)
	{
		base.Init(player);

		Description = "Increase ATK SPD by " + AtkSpdMultiplier.ToString("#.#") + "x.";

		player.Modify(this, "AttackSpeed", AtkSpdMultiplier, ModifierType.Mult);
		//player.Modify(this, "ReloadSpeed", 6f, ModifierType.Mult);
	}

	public override void Update(float dt)
	{
		base.Update(dt);

		//if (ElapsedTime > 3f)
		//	Player.RemoveStatus(this);
	}

	public override void Remove()
	{
		base.Remove();
	}
}
