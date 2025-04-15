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

    public static int GetParameterNum(EventDescription eventDescription)
    {
        eventDescription.getParameterDescriptionCount(out int parameterCount);
        return parameterCount;
    }
}