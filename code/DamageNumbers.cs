
using System;
using System.Collections.Generic;
using Sandbox;

internal static class DamageNumbers
{

	private struct DamageNumberData
	{
		public Vector3 Position;
		public float Amount;
		//public bool IsArmor;
	}

	//private static List<DamageNumberData> Batch = new();

	//[Event.Tick.Client]
	//public static void ProcessDamageNumbers()
	//{
	//	var armorTotal = 0;
	//	var armorAmount = 0f;
	//	var armorCenter = Vector3.Zero;
	//	int total = 0;
	//	var amount = 0f;
	//	var center = Vector3.Zero;

	//	foreach ( var dmg in Batch )
	//	{
	//		if ( dmg.IsArmor )
	//		{
	//			armorTotal++;
	//			armorAmount += dmg.Amount;
	//			armorCenter += dmg.Position;
	//		}
	//		else
	//		{
	//			total++;
	//			amount += dmg.Amount;
	//			center += dmg.Position;
	//		}
	//	}

	//	armorCenter /= armorTotal;
	//	center /= total;

	//	if ( armorAmount > 0 ) Create( armorCenter, armorAmount, true );
	//	if ( amount > 0 ) Create( center, amount, false );

	//	Batch.Clear();
	//}

	//public static void Enqueue( Vector3 pos, float amount, bool armor = false )
	//{
	//	Batch.Add( new()
	//	{
	//		Position = pos,
	//		Amount = amount,
	//		IsArmor = armor
	//	} );
	//}

	public static void Create( Vector3 pos, float amount)
	{
		var path = "particles/dmg_number.vpcf";
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
