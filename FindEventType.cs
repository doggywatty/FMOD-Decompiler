using FMOD.Studio;
using static Program;

// Finds out what Sheet Type an Event uses

public class FindEventType
{
    public static bool EventisTimeline(EventInstance evInst)
    {
        if (evInst.getTimelinePosition(out int timelinePos) == FMOD.RESULT.OK)
        {
            return true;
        }
        return false;
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

    public static void GetParameterInfo(EventDescription evDesc)
    {
        if (evDesc.getParameterDescriptionCount(out int parameterCount) != FMOD.RESULT.OK || parameterCount == 0)
            return;

        for (int i = 0; i < parameterCount; i++)
        {
            if (evDesc.getParameterDescriptionByIndex(i, out PARAMETER_DESCRIPTION parameter) == FMOD.RESULT.OK)
            {
                // Make XML of the Parameter (if it wasn't made already)
                if (!Parameters.ParameterList.Contains(parameter))
                {
                    Parameters.ParameterXML(parameter, evDesc);
                    Parameters.ParameterList.Add(parameter);
                }
            }
        }
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
                string parmName = parameter.name;
                uint parmID = parameter.id.data1 + parameter.id.data2;//combine uints because yeah
                Guid parmGUID = FMODGUIDToSysGuid(parameter.guid);

                // spacing for console
                var spacing = (i != 0 ? "\n\n" : "");

                parameterInfo += $"{spacing}\tParameter Name: {parmName}" +
                                 $"\n\tParameter ID: {parmID}" +
                                 $"\n\tParameter GUID: {parmGUID}" +
                                 $"\n\tParameter Min Value: {parameter.minimum}" +
                                 $"\n\tParameter Max Value: {parameter.maximum}" +
                                 $"\n\tParameter Default Value: {parameter.defaultvalue}" +
                                 $"\n\tParameter Type: {parameter.type}" +
                                 $"\n\tParameter Flags: {parameter.flags}";
            }
            else
            {
                parameterInfo += $"\tUnable to retrieve details for parameter at index {i}.\n";
            }
        }

        return parameterInfo;
    }
}
