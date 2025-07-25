using System.Collections.Generic;

namespace Audio
{
    public static class AmbienceSystem
    {
        private static readonly List<RoomAmbienceController> ActiveRooms = new List<RoomAmbienceController>();

        public static void Register(RoomAmbienceController room)
        {
            if (!ActiveRooms.Contains(room))
            {
                ActiveRooms.Add(room);
            }
        }

        public static void Unregister(RoomAmbienceController room)
        {
            if (ActiveRooms.Contains(room))
            {
                ActiveRooms.Remove(room);
            }
        }
    
        /// <summary>
        /// It iterates over all recorded rooms and pauses their ambient sounds.
        /// </summary>
        public static void PauseAllRoomAmbience()
        {
            foreach (var room in ActiveRooms)
            {
                if (room != null) // Safety control
                {
                    room.PauseAllSounds();
                }
            }
        }

        /// <summary>
        /// It iterates over all recorded rooms and picks up their ambient sounds.
        /// </summary>
        public static void ResumeAllRoomAmbience()
        {
            foreach (var room in ActiveRooms)
            {
                if (room != null) // Safaety control
                {
                    room.ResumeAllSounds();
                }
            }
        }
    }
}