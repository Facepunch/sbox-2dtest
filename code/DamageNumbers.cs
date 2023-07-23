
using System;
using System.Collections.Generic;
using System.Drawing;
using Sandbox;
using Test2D;

public enum DamageNumberType { Normal, Crit, Player }

internal static class DamageNumbers
{
	public static void Create( Vector3 pos, float amount, DamageNumberType damageNumberType)
	{
		if(amount < 1f)
        {
			amount = MathF.Ceiling(amount);
        }
		else
        {
			float fractional = amount - MathF.Floor(amount);
			if (fractional > 0f && Sandbox.Game.Random.Float(0f, 1f) > fractional)
				amount = MathF.Floor(amount);
			else
				amount = MathF.Ceiling(amount);
		}

		var number = amount;
		var particle = Particles.Create("particles/damagenumber_ss2/dmg_number_ss2.vpcf", pos.WithZ(400f));

		if ( amount < 10 )
		{
			particle.SetPositionComponent( 21, 0, number % 10 );
		}
		else if ( amount < 100 )
		{
			particle.SetPositionComponent( 21, 1, number % 10 );
			particle.SetPositionComponent( 22, 1, 1 );

			number /= 10;
			particle.SetPositionComponent( 21, 0, MathF.Floor(number % 10) );
		}
		else
		{
			particle.SetPositionComponent( 21, 2, number % 10 );
			particle.SetPositionComponent( 22, 2, 1 );

			number /= 10;
			particle.SetPositionComponent( 21, 1, MathF.Floor(number % 10));
			particle.SetPositionComponent( 22, 1, 1 );

			number /= 10;
			particle.SetPositionComponent( 21, 0, MathF.Floor(number % 100));
			particle.SetPositionComponent( 22, 0, 1 );
		}

		Color color = damageNumberType == DamageNumberType.Normal ? Color.White : (damageNumberType == DamageNumberType.Crit ? Color.Yellow : Color.Red);
		float size = Utils.Map(amount, 1f, 20f, 0.15f, 0.18f, EasingType.Linear) * Utils.Map(amount, 20f, 100f, 0.1f, 0.15f, EasingType.Linear) * 1.6f;
		Vector3 velocity = new Vector3(Game.Random.Float(-1f, 1f) * 2f, Game.Random.Float(3.5f, 4.5f), 0f);
		Vector3 gravity = new Vector3(0f, -7f, 0f);

		particle.Set("Color", color);
		particle.Set("Size", size);
		particle.SetPosition(1, velocity);
		particle.Set("Gravity", gravity);
	}

}
