using System;
using FMOD.Studio;

// Finds out what Sheet Type an Event uses

// we can't really find out if it's an Action Event tho
// and Actions should be the default anyways
// sooooo ... if its not either, just default to action

public class FindEventType
{
    public static bool EventisTimeline(EventInstance eventInstance)
    {
        // its an idea
        // doesn't really work tho
        if (eventInstance.getTimelinePosition(out int timelinePos) == FMOD.RESULT.OK)
            return true;
        else
            return false;
    }

    public static bool EventisParameter(EventDescription eventDescription)
    {
        // pretty solid way of checking for params i think
        eventDescription.getParameterDescriptionCount(out int parameterCount);

        if (parameterCount > 0)
            return true;
        else
            return false;
    }

    public static string DisplayParameterInfo(EventDescription eventDescription)
    {
        eventDescription.getParameterDescriptionCount(out int parameterCount);
        eventDescription.getParameterDescriptionByIndex(parameterCount, out PARAMETER_DESCRIPTION parameter);

        return $"\tParameter Name: {parameter.name}"
           + $"\n\tParameter ID: {parameter.id}"
           + $"\n\tParameter Min Value: {parameter.minimum}"
           + $"\n\tParameter Max Value: {parameter.maximum}"
           + $"\n\tParameter Default Value: {parameter.defaultvalue}"
           + $"\n\tParameter Type: {parameter.type}"
           + $"\n\tParameter Flags: {parameter.flags}";
    }
}