namespace AetherFlow.Core.Enums{
    public enum ZoneType {
        
        Neutral, // Zone is neutral, no team control
        ControlledByTeamA, // Zone is controlled by Team A
        ControlledByTeamB, // Zone is controlled by Team B
        Contested // Zone is currently contested by both teams
    }
}