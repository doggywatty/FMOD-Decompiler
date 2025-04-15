using FMOD.Studio;

public class FindEventType
{
    public static bool EventisTimeline(EventInstance eventInstance)
    {
        // should work maybe
        // its possible that we can get false positives tho, idk
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
        {
            // TODO: Make it only run this code if Verbose == true
            for (int i = 0; i < parameterCount; i++)
            {
                eventDescription.getParameterDescriptionByIndex(i, out PARAMETER_DESCRIPTION paramDesc);
                Console.WriteLine($"Parameter: {paramDesc.name}, Type: {paramDesc.type}");
            }
            return true;
        }
        else
            return false;
    }
    public static bool EventisAction(EventInstance eventInstance, EventDescription eventDescription)
    {
        // return true if an event isn't either of the two above

        // we can't really find out if it's an action or not
        // and Actions should be the default anyways
        // sooooo ... this is the best we got for now
        if (!EventisTimeline(eventInstance) && !EventisParameter(eventDescription))
            return true;
        else
            return false;
    }
}