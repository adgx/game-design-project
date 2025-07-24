using System.Collections.Generic;
using Audio;

public static class AmbienceSystem
{
    private static readonly List<RoomAmbienceController> _activeRooms = new List<RoomAmbienceController>();

    public static void Register(RoomAmbienceController room)
    {
        if (!_activeRooms.Contains(room))
        {
            _activeRooms.Add(room);
        }
    }

    public static void Unregister(RoomAmbienceController room)
    {
        if (_activeRooms.Contains(room))
        {
            _activeRooms.Remove(room);
        }
    }
}