using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Test2D;

public struct StatusData
{
    public int maxLevel;
    public int reqLevel;
    public float weight;
    public List<string> reqStatuses;

    public StatusData(int _maxLevel, int _reqLevel, float _weight, List<string> _reqStatuses = null)
    {
        maxLevel = _maxLevel;
        reqLevel = _reqLevel;
        weight = _weight;
        reqStatuses = _reqStatuses;
    }
}

public class StatusAttribute : Attribute
{
    public int MaxLevel { get; }
    public int ReqLevel { get; }
    public float Weight { get; }
    public Type[] ReqStatuses { get; }

    public StatusAttribute(int maxLevel, int reqLevel, float weight, params Type[] reqStatuses)
    {
        MaxLevel = maxLevel;
        ReqLevel = reqLevel;
        Weight = weight;
        ReqStatuses = reqStatuses;
    }
}

public class StatusManager
{
    public static List<TypeDescription> GetRandomStatuses(PlayerCitizen player, int numStatuses)
    {
        List<(TypeDescription Type, float Weight)> valid = new List<(TypeDescription, float)>();

        foreach (var type in TypeLibrary.GetDescriptions<Status>())
        {
            //Log.Info("--------------- " + type.Name);

            var attrib = type.GetAttribute<StatusAttribute>();
            if (attrib == null)
            {
                //Log.Info("not valid - no status attribute!");
                continue;
            }
                
            if (player.Level < attrib.ReqLevel)
            {
                //Log.Info("not valid - ReqLevel is " + attrib.ReqLevel + " and player level is " + player.Level);
                continue;
            }

            if (player.GetStatusLevel(type) >= attrib.MaxLevel)
            {
                //Log.Info("not valid - MaxLevel is " + attrib.MaxLevel + " and player status level is " + player.GetStatusLevel(type));
                continue;
            }

            if (attrib.ReqStatuses.Length > 0 && attrib.ReqStatuses.All(x => !player.HasStatus(TypeLibrary.GetDescription(x))))
            {
                //Log.Info("not valid - none of the required statuses owned");
                continue;
            }

            //int currLevel = player.GetStatusLevel(type);

            //Log.Info("valid: adding with weight of " + attrib.Weight);
            valid.Add((type, attrib.Weight));
        }

        // todo: handle if valid has < elements than numStatuses

        List<TypeDescription> output = new List<TypeDescription>();

        while(output.Count < numStatuses)
        {
            float totalWeight = valid.Sum(x => x.Weight);
            var rand = Rand.Float(0f, totalWeight);
            //Log.Info("--- output.Count: " + output.Count + " totalWeight: " + totalWeight +" rand: " + rand);

            for (int i = valid.Count - 1; i >= 0; i--)
            {
                var (type, weight) = valid[i];
                rand -= weight;

                //Log.Info("i: " + i + " type: " + type.Name + " weight: " + weight + " rand is now " + rand);

                if (rand < 0f)
                {
                    output.Add(type);
                    valid.Remove((type, weight));
                    break;
                }
            }
        }

        return output;
    }

    public static Status CreateStatus(TypeDescription type)
    {
        var status = type.Create<Status>();

        var attrib = type.GetAttribute<StatusAttribute>();
        if (attrib != null)
            status.MaxLevel = attrib.MaxLevel;

        return status;
    }

    //public static Type GetStatusType(string statusName)
    //{
    //    return TypeLibrary.GetDescription(statusName).TargetType;
    //}

    public static int TypeToIdentity(TypeDescription type)
    {
        return type.Identity;
    }

    public static TypeDescription IdentityToType(int typeIdentity)
    {
        return TypeLibrary.GetDescriptionByIdent(typeIdentity);
    }
}
