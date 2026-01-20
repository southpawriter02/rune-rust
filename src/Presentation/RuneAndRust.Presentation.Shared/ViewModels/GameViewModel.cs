using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Presentation.Shared.ViewModels;

/// <summary>
/// ViewModel representing the overall game state for data binding in views.
/// </summary>
/// <remarks>
/// GameViewModel aggregates player and room state along with game metadata.
/// It provides observable properties for UI binding and methods to update
/// from DTOs received from the application layer.
/// </remarks>
public partial class GameViewModel : ObservableObject
{
    /// <summary>
    /// The unique identifier of the current game session.
    /// </summary>
    [ObservableProperty]
    private Guid _sessionId;

    /// <summary>
    /// The current state of the game (Playing, GameOver, Victory, etc.).
    /// </summary>
    [ObservableProperty]
    private GameState _state;

    /// <summary>
    /// The player character's view model.
    /// </summary>
    [ObservableProperty]
    private PlayerViewModel _player = new();

    /// <summary>
    /// The current room's view model.
    /// </summary>
    [ObservableProperty]
    private RoomViewModel _currentRoom = new();

    /// <summary>
    /// The timestamp when the session was last played.
    /// </summary>
    [ObservableProperty]
    private DateTime _lastPlayedAt;

    /// <summary>
    /// The most recent message to display to the user.
    /// </summary>
    [ObservableProperty]
    private string _lastMessage = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the game is actively being played.
    /// </summary>
    public bool IsPlaying => State == GameState.Playing;

    /// <summary>
    /// Gets a value indicating whether the player has been defeated.
    /// </summary>
    public bool IsGameOver => State == GameState.GameOver;

    /// <summary>
    /// Updates this view model from a game state DTO.
    /// </summary>
    /// <param name="dto">The DTO containing the updated game state.</param>
    public void UpdateFrom(GameStateDto dto)
    {
        SessionId = dto.SessionId;
        State = dto.State;
        LastPlayedAt = dto.LastPlayedAt;

        Player.UpdateFrom(dto.Player);
        CurrentRoom.UpdateFrom(dto.CurrentRoom);

        OnPropertyChanged(nameof(IsPlaying));
        OnPropertyChanged(nameof(IsGameOver));
    }

    /// <summary>
    /// Sets the message to display to the user.
    /// </summary>
    /// <param name="message">The message text.</param>
    public void SetMessage(string message)
    {
        LastMessage = message;
    }

    /// <summary>
    /// Clears the current message.
    /// </summary>
    public void ClearMessage()
    {
        LastMessage = string.Empty;
    }
}
