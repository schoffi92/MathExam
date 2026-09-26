using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExam.Core;

namespace MathExam.App.ViewModels;

/// <summary>One profile on the Players screen.</summary>
public partial class PlayerRow : ObservableObject
{
    public PlayerRow(Profile profile, Action<PlayerRow> onRemove)
    {
        Name = profile.Name;
        _dailyGoal = profile.DailyGoal;
        RemoveCommand = new RelayCommand(() => onRemove(this));
    }

    public string Name { get; }

    // decimal?, because that is what NumericUpDown binds to; null while the box is being edited.
    [ObservableProperty]
    private decimal? _dailyGoal;

    public IRelayCommand RemoveCommand { get; }

    public Profile ToProfile() => new(Name, (int)Math.Clamp(DailyGoal ?? 0, 0, ProfileList.MaxDailyGoal));
}

/// <summary>Add and remove profiles and set their daily goals. The changes are handed back when leaving.</summary>
public partial class PlayersViewModel : ObservableObject
{
    private readonly string? _current;
    private readonly Action<ProfileList> _onDone;

    public PlayersViewModel(ProfileList profiles, Action<ProfileList> onDone)
    {
        _current = profiles.Current;
        _onDone = onDone;
        foreach (var profile in profiles.Profiles)
            Rows.Add(new PlayerRow(profile, Remove));
    }

    public ObservableCollection<PlayerRow> Rows { get; } = [];

    public int MaxDailyGoal => ProfileList.MaxDailyGoal;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string _newName = "";

    /// <summary>Why the typed name cannot be added; empty while nothing is typed.</summary>
    public string ErrorMessage => NewName.Trim().Length == 0 ? "" : Current().ValidateNewName(NewName) ?? "";

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        Rows.Add(new PlayerRow(new Profile(NewName.Trim()), Remove));
        NewName = "";
    }

    private bool CanAdd() => NewName.Trim().Length > 0 && Current().ValidateNewName(NewName) is null;

    [RelayCommand]
    private void Back()
    {
        var list = Current();
        // A removed player can no longer be the current one; the guest plays instead.
        _onDone(list with { Current = list.Find(_current)?.Name });
    }

    private void Remove(PlayerRow row)
    {
        Rows.Remove(row);
        OnPropertyChanged(nameof(ErrorMessage));
        AddCommand.NotifyCanExecuteChanged();
    }

    private ProfileList Current() => new(Rows.Select(r => r.ToProfile()).ToList(), _current);
}
