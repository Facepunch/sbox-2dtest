
using System;
using System.Collections.Generic;
using Sandbox;

public enum DamageType { Normal, Crit, Player }

internal static class DamageNumbers
{
	public static void Create( Vector3 pos, float amount, DamageType damageType)
	{
		if(amount < 1f)
        {
			amount = MathF.Ceiling(amount);
        }
		else
        {
			float fractional = amount - MathF.Floor(amount);
			if (fractional > 0f && Sandbox.Game.Random.Float(0f, 1f) > fractional)
				amount = MathF.Ceiling(amount);
			else
				amount = MathF.Floor(amount);
		}

		var path = damageType == DamageType.Normal ? "particles/dmg_number.vpcf" : (damageType == DamageType.Crit ? "particles/dmg_number_crit.vpcf" : "particles/dmg_number_player.vpcf");
		var number = amount;
		var particle = Particles.Create( path, pos.WithZ(400f) );

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
	}

}
