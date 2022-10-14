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
            var attrib = type.GetAttribute<StatusAttribute>();
            if (attrib == null)
                continue;

            if (player.Level < attrib.ReqLevel)
                continue;

            if (player.GetStatusLevel(type) >= attrib.MaxLevel)
                continue;

            if (attrib.ReqStatuses.Any(x => !player.HasStatus(TypeLibrary.GetDescription(x))))
                continue;

            valid.Add((type, attrib.Weight));
        }

        // todo: handle if valid has < elements than numStatuses

        List<TypeDescription> output = new List<TypeDescription>();

        while(output.Count < numStatuses)
        {
            float totalWeight = valid.Sum(x => x.Weight);

            var rand = Rand.Float(0f, totalWeight);
            for(int i = valid.Count - 1; i >= 0; i--)
            {
                var (type, weight) = valid[i];
                rand -= weight;
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
