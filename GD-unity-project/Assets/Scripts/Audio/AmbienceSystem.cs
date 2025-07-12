using System.Collections.Generic;
using UnityEngine;

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

    /// <summary>
    /// Comando centrale per spegnere tutti i suoni ambientali di tutte le stanze attive.
    /// </summary>
    public static void StopAllRoomAmbience()
    {
        // Itera su una copia per sicurezza
        List<RoomAmbienceController> roomsToStop = new List<RoomAmbienceController>(_activeRooms);
        
        foreach (var room in roomsToStop)
        {
            if (room != null)
            {
                room.DeactivateAllSounds();
            }
        }
    }
}