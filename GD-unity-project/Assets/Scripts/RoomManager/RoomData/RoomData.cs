using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoomManager.RoomData
{
    /// <summary>
    /// Holds configuration data for an individual room.
    /// Used to control spawning, types, and special object chances.
    /// </summary>
    [CreateAssetMenu(fileName = "RoomData", menuName = "Scriptable Objects/RoomData")]
    public class RoomData : ScriptableObject
    {
        /// <summary>
        /// Prefab associated with this room.
        /// </summary>
        [Tooltip("The list of prefabs used to instantiate this room.")]
        public List<GameObject> roomPrefab;

        /// <summary>
        /// Display name of the room.
        /// </summary>
        [Tooltip("Name of the room used for identification.")]
        public string roomName;

        /// <summary>
        /// Base spawn budget used for enemy or item placement.
        /// </summary>
        [Tooltip("Base budget used to determine what spawns in the room.")]
        public int roomSpawnBudget;

        /// <summary>
        /// Spawn budget for loop iteration 1 (used as a base for scaling).
        /// </summary>
        [Tooltip("Base spawn budget used during loop 1. Used for scaling difficulty.")]
        public int roomSpawnBudgetLoop1;

		/// <summary>
		/// Chance (0–1) to spawn a health vending machine.
		/// </summary>
		[Range(0f, 1f)]
		[Tooltip("Chance to spawn a health vending machine in this room.")]
		public float healthVendingMachineSpawnChance;

		/// <summary>
		/// Internal flag for spawning a health vending machine (set at runtime).
		/// </summary>
		[HideInInspector]
		public bool spawnHealthVendingMachine;

		/// <summary>
		/// Chance (0–1) to spawn a power up vending machine.
		/// </summary>
		[Range(0f, 1f)]
        [Tooltip("Chance to spawn a vending machine in this room.")]
        public float powerUpVendingMachineSpawnChance;

		/// <summary>
		/// Internal flag for spawning a power up vending machine (set at runtime).
		/// </summary>
		[HideInInspector]
        public bool spawnPowerUpVendingMachine;

        /// <summary>
        /// Chance (0–1) to spawn an upgrade terminal.
        /// </summary>
        [Range(0f, 1f)]
        [Tooltip("Chance to spawn an upgrade terminal in this room.")]
        public float upgradeTerminalSpawnChance;

        /// <summary>
        /// Internal flag for spawning an upgrade terminal (set at runtime).
        /// </summary>
        [HideInInspector]
        public bool spawnUpgradeTerminal;

        /// <summary>
        /// Chance (0–1) to spawn a collectible paper item.
        /// </summary>
        [Range(0f, 1f)]
        [Tooltip("Chance to spawn a paper item in this room.")]
        public float paperSpawnChance;

        /// <summary>
        /// Internal flag for spawning a paper item (set at runtime).
        /// </summary>
        [HideInInspector]
        public bool spawnPaper;

        /// <summary>
        /// Type/category of the room (e.g. Combat, Treasure, etc).
        /// </summary>
        [Tooltip("Classification of the room type.")]
        public RoomType roomType;

        /// <summary>
        /// Scales the room's spawn budget based on difficulty and current loop iteration.
        /// </summary>
        /// <param name="difficultyMultiplier">Multiplier to scale difficulty (e.g., 1.2 for 20% harder).</param>
        public void SetDifficulty(float difficultyMultiplier)
        {
            roomSpawnBudget = roomSpawnBudgetLoop1 +
                              (int)Math.Round(roomSpawnBudgetLoop1 * difficultyMultiplier * (int)GameStatus.loopIteration);
        }
        
        public RoomData Clone()
        {
            var clone = CreateInstance<RoomData>();
            
            clone.roomPrefab = roomPrefab;
            clone.roomName = roomName;
            clone.roomSpawnBudget = roomSpawnBudget;
            clone.roomSpawnBudgetLoop1 = roomSpawnBudgetLoop1;
			clone.healthVendingMachineSpawnChance = healthVendingMachineSpawnChance;
			clone.spawnHealthVendingMachine = spawnHealthVendingMachine;
			clone.powerUpVendingMachineSpawnChance = powerUpVendingMachineSpawnChance;
            clone.spawnPowerUpVendingMachine = spawnPowerUpVendingMachine;
            clone.upgradeTerminalSpawnChance = upgradeTerminalSpawnChance;
            clone.spawnUpgradeTerminal = spawnUpgradeTerminal;
            clone.paperSpawnChance = paperSpawnChance;
            clone.spawnPaper = spawnPaper;
            clone.roomType = roomType;

            return clone;
        }
    }
}
