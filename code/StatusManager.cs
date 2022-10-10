using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Test2D;

public struct StatusData
{
    public string name;
    public int maxLevel;
    public List<string> reqStatuses;
    public int reqLevel;


}

public class StatusManager
{
    public StatusManager()
    {
        
    }

    public string GetRandomValidStatus(PlayerCitizen player, List<string> ignoredStatuses)
    {
        return "";
    }

    public static Status CreateStatus(string statusName)
    {
        return TypeLibrary.Create<Status>(statusName);
    }
}
