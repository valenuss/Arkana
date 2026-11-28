using Arkana.Helpers;
using Arkana.Models;
using Arkana.Services;

namespace Arkana.Pages;

public partial class StoryViewerPage : ContentPage, IQueryAttributable
{
    readonly StoryService storyService;

    List<Story> stories = new();
    int userId;
    int index;

    public StoryViewerPage(StoryService storyService)
    {
        InitializeComponent();
        this.storyService = storyService;
    }

    // recibe el id del dueño de las historias: storyviewer?userId=3
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("userId", out var value) && int.TryParse(value?.ToString(), out var id))
        {
            userId = id;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (stories.Count > 0)
        {
            return;
        }

        stories = storyService.GetActive(userId);

        if (stories.Count == 0)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        index = 0;
        Show();
    }

    // muestra la historia actual
    void Show()
    {
        var story = stories[index];

        StoryImage.Source = ImageSource.FromFile(story.ImagePath);
        OwnerAvatar.Initial = story.Username.ToInitial();
        NameLabel.Text = story.Username;
        TimeLabel.Text = story.CreatedAt.ToTimeAgo();
        CounterLabel.Text = $"{index + 1}/{stories.Count}";
        DeleteIcon.IsVisible = Session.CurrentUser?.Id == story.UserId;
    }

    // tocar la foto: siguiente historia, o cerrar si era la última
    private async void Next_Tapped(object? sender, TappedEventArgs e)
    {
        index++;

        if (index >= stories.Count)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        Show();
    }

    private async void Close_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("..");

    // eliminar la historia que se está viendo
    private async void Delete_Tapped(object? sender, TappedEventArgs e)
    {
        var confirm = await DisplayAlert(
            "Eliminar historia",
            "¿Seguro que quieres eliminarla?",
            "Eliminar",
            "Cancelar");

        if (!confirm)
        {
            return;
        }

        try
        {
            storyService.Delete(stories[index]);
            stories.RemoveAt(index);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            return;
        }

        if (stories.Count == 0)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        index = Math.Min(index, stories.Count - 1);
        Show();
    }
}