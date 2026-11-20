using System.Collections.ObjectModel;
using System.ComponentModel;
using Arkana.Data;
using Arkana.Helpers;
using Arkana.Services;

namespace Arkana.Pages;

public partial class SearchPage : ContentPage
{
    readonly SearchData data;

    public SearchPage(AppDbContext db, FollowService follows)
    {
        InitializeComponent();

        // la clase de datos se enlaza con la pantalla
        data = new SearchData(db, follows);
        BindingContext = data;
    }

    // cada vez que se abre la pestaña, se carga la lista
    protected override void OnAppearing()
    {
        base.OnAppearing();
        data.Cargar();
    }

    // seguir o dejar de seguir
    private void Follow_Clicked(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: UserItem item })
        {
            data.ToggleFollow(item);
        }
    }
}


// nombres de los filtros de la pantalla Explorar
public static class UserFilter
{
    public const string All = "Todos";
    public const string Following = "Siguiendo";
    public const string Suggested = "Sugeridos";
}


// clase de datos de la pantalla Explorar
public class SearchData : BindingUtilObject
{
    readonly AppDbContext db;
    readonly FollowService follows;

    public SearchData(AppDbContext db, FollowService follows)
    {
        this.db = db;
        this.follows = follows;
        PropertyChanged += SearchData_PropertyChanged;
    }

    // lista de usuarios que se muestra
    ObservableCollection<UserItem> _users = new();
    public ObservableCollection<UserItem> Users
    {
        get => _users;
        set => SetProperty(ref _users, value);
    }

    // lo que se escribe en el buscador
    string _query = "";
    public string Query
    {
        get => _query;
        set => SetProperty(ref _query, value);
    }

    // filtro activo: Todos, Siguiendo o Sugeridos
    string _filter = UserFilter.All;
    public string Filter
    {
        get => _filter;
        set => SetProperty(ref _filter, value);
    }

    // el usuario que tocaron en la lista
    UserItem? _selectedUser;
    public UserItem? SelectedUser
    {
        get => _selectedUser;
        set => SetProperty(ref _selectedUser, value);
    }

    // reacciona cuando cambia una propiedad
    private async void SearchData_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            // se escribió algo o se cambió el filtro
            case nameof(Query):
            case nameof(Filter):
                Cargar();
                break;

            // tocaron un usuario: abrir su perfil pasando el id
            case nameof(SelectedUser) when SelectedUser is { } selected:
                SelectedUser = null; // deseleccionar para poder tocarlo de nuevo
                await Shell.Current.GoToAsync($"{Routes.UserProfile}?id={selected.Id}");
                break;
        }
    }

    // llena la lista según el filtro y lo que escribieron
    public void Cargar()
    {
        if (Session.CurrentUser is not { } current)
        {
            return;
        }

        var following = follows.GetFollowingIds(current.Id);
        var text = (Query ?? "").Trim();

        var items = db.Users
            .Where(u => u.Id != current.Id)
            .AsEnumerable()
            .Where(u => Filter switch
            {
                UserFilter.Following => following.Contains(u.Id),
                UserFilter.Suggested => !following.Contains(u.Id),
                _ => true
            })
            .Where(u => text.Length == 0 ||
                        u.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                        u.Username.Contains(text, StringComparison.OrdinalIgnoreCase))
            .Select(u => new UserItem
            {
                Id = u.Id,
                Name = u.Name,
                Handle = "@" + u.Username,
                Initial = u.Username.ToInitial(),
                IsFollowing = following.Contains(u.Id)
            });

        Users = new ObservableCollection<UserItem>(items);
    }

    // seguir o dejar de seguir
    public void ToggleFollow(UserItem item)
    {
        if (Session.CurrentUser is not { } current)
        {
            return;
        }

        follows.Toggle(current.Id, item.Id);
        Cargar();
    }
}


// datos de cada usuario tal como se muestra en la lista
public class UserItem
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Handle { get; init; } = "";
    public string Initial { get; init; } = "?";
    public bool IsFollowing { get; init; }

    public string FollowText => IsFollowing ? "Siguiendo" : "Seguir";
}