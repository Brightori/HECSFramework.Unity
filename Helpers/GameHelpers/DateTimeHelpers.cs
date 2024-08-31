using System;
using HECSFramework.Core;

namespace Helpers
{
    [Documentation(Doc.Helpers, Doc.HECS, Doc.Time, "this class contains helpers around datetime and time")]
    public static class DateTimeHelpers
    {
        public static float TimeLeft(DateTime dateTime, float intervalInSeconds)
        {
            return (float)(DateTime.UtcNow - dateTime).TotalSeconds - intervalInSeconds;
        }
    }

    [Documentation(Doc.Helpers, Doc.Time, "we can have realisation for providing time from additional source")]
    public interface ITimeProvider
    {
        DateTime GetTime();
    }
}
