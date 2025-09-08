public static class GameStatus {
	// True if the game is paused
	public static bool gamePaused = false;

	// True if I have pressed "Start Game"
	public static bool gameStarted = false;

	// True if I have reached the last loop
	public static bool gameEnded = false;
	
	// True if I have the UI pause menu open
	public static bool isUIPauseMenuOpen = false;
	
	// True if I have the UI inventory menu open
	public static bool isUIInventoryMenuOpen = false;

	public enum LoopIteration {
		FIRST_ITERATION = 0,
		SECOND_ITERATION = 1,
		THIRD_ITERATION = 2
	}

	public static LoopIteration loopIteration = LoopIteration.FIRST_ITERATION;
}