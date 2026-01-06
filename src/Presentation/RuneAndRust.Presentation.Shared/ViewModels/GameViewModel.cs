using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Presentation.Shared.ViewModels;

public partial class GameViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _sessionId;

    [ObservableProperty]
    private GameState _state;

    [ObservableProperty]
    private PlayerViewModel _player = new();

    [ObservableProperty]
    private RoomViewModel _currentRoom = new();

    [ObservableProperty]
    private DateTime _lastPlayedAt;

    [ObservableProperty]
    private string _lastMessage = string.Empty;

    public bool IsPlaying => State == GameState.Playing;
    public bool IsGameOver => State == GameState.GameOver;

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

    public void SetMessage(string message)
    {
        LastMessage = message;
    }

    public void ClearMessage()
    {
        LastMessage = string.Empty;
    }
}
