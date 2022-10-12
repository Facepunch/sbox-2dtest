using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Test2D;

//public struct StatusData
//{
//    public int maxLevel;
//    public List<string> reqStatuses;
//    public int reqLevel;
//}

public class StatusManager
{
    //private Dictionary<string, StatusData> _statuses = new Dictionary<string, StatusData>();
    private static List<string> _statusNames = new List<string> {
        //"AttackSpeedStatus",
        //"CritChanceStatus",
        //"CritMultiplierStatus",
        //"DamageStatus",
        //"MaxAmmoStatus",
        //"MovespeedStatus",
        //"NumProjectileStatus",
        //"PiercingStatus",
        //"ReduceSpreadStatus",
        "ReloadSpeedStatus",
        "BulletLifetimeStatus",
    };

    public StatusManager()
    {
        
    }

    public static string GetRandomValidStatus(PlayerCitizen player, List<string> ignoredStatuses = null)
    {
        return _statusNames[Rand.Int(0, _statusNames.Count - 1)];
    }

    public static Status CreateStatus(string statusName)
    {
        return TypeLibrary.Create<Status>(statusName);
    }

    public static Type GetStatusType(string statusName)
    {
        return TypeLibrary.GetDescription(statusName).TargetType;
    }
}
