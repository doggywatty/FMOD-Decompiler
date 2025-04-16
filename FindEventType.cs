using System;
using FMOD.Studio;

// Finds out what Sheet Type an Event uses

// we can't really find out if it's an Action Event tho
// and Actions should be the default anyways
// sooooo ... if its not either, just default to action

public class FindEventType
{
    public static bool EventisTimeline(EventInstance evInst)
    {
        if (evInst.getTimelinePosition(out int timelinePos) == FMOD.RESULT.OK)
        {
            return true;
        }
        return false; // explicit return statement for clarity
    }

    public static bool EventisParameter(EventDescription evDesc)
    {
        if (evDesc.getParameterDescriptionCount(out int parameterCount) != FMOD.RESULT.OK)
        {
            Console.WriteLine("Error: Unable to retrieve parameter count.");
            return false;
        }
        return parameterCount > 0;
    }

    public static string DisplayParameterInfo(EventDescription evDesc)
    {
        if (evDesc.getParameterDescriptionCount(out int parameterCount) != FMOD.RESULT.OK || parameterCount == 0)
        {
            Console.WriteLine("No parameters found or unable to retrieve parameter details.");
        }

        var parameterInfo = string.Empty;
        for (int i = 0; i < parameterCount; i++)
        {
            if (evDesc.getParameterDescriptionByIndex(i, out PARAMETER_DESCRIPTION parameter) == FMOD.RESULT.OK)
            {
                parameterInfo += $"\tParameter Name: {parameter.name}" +
                                 $"\n\tParameter ID: {parameter.id}" +
                                 $"\n\tParameter Min Value: {parameter.minimum}" +
                                 $"\n\tParameter Max Value: {parameter.maximum}" +
                                 $"\n\tParameter Default Value: {parameter.defaultvalue}" +
                                 $"\n\tParameter Type: {parameter.type}" +
                                 $"\n\tParameter Flags: {parameter.flags}\n";
            }
            else
            {
                parameterInfo += $"\tUnable to retrieve details for parameter at index {i}.\n";
            }
        }
        return parameterInfo;
    }
}
