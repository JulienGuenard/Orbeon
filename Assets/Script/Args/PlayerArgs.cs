using System;

public class PlayerArgs : EventArgs {

	public Player currentPlayer;
	public Phase currentPhase;

	public PlayerArgs(Player currentPlayer, Phase currentPhase)
	{
		this.currentPlayer = currentPlayer;
		this.currentPhase = currentPhase;
	}
}