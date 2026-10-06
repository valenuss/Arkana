using System.Collections.ObjectModel;
using System.ComponentModel;
using Arkana.Data;
using Arkana.Models;
using Arkana.Services;

namespace Arkana.Pages;

public partial class SearchPage : ContentPage
{
    readonly SearchData data;

    public SearchPage(AppDbContext db)
    {
        InitializeComponent();

        // la clase de datos se enlaza con la pantalla
        data = new SearchData(db);
        BindingContext = data;
    }

    // cada vez que se abre la pestaña, se carga la lista
    protected override void OnAppearing()
    {
        base.OnAppearing();
        data.Cargar();
    }

    // al tocar un filtro
    private void Chip_Tapped(object sender, TappedEventArgs e)
    {
        string filtro = (string)e.Parameter;

        Pintar(ChipAll, ChipAllLabel, filtro == "Todos");
        Pintar(ChipFollowing, ChipFollowingLabel, filtro == "Siguiendo");
        Pintar(ChipSuggested, ChipSuggestedLabel, filtro == "Sugeridos");

        data.SetFilter(filtro);
    }

    // pone rojo el filtro activo y oscuro los demas
    void Pintar(Border chip, Label etiqueta, bool activo)
    {
        if (activo)
        {
            chip.BackgroundColor = Color.FromArgb("#FF1F3D");
            etiqueta.TextColor = Colors.White;
        }
        else
        {
            chip.BackgroundColor = Color.FromArgb("#14141A");
            etiqueta.TextColor = Color.FromArgb("#9A9AA5");
        }
    }

    // seguir o dejar de seguir
    private void Follow_Clicked(object sender, EventArgs e)
    {
        Button boton = (Button)sender;
        UserItem item = (UserItem)boton.BindingContext;

        data.ToggleFollow(item);
    }
}


// clase de datos de la pantalla Explorar (como HelpSupportData de la clase)
public class SearchData : BindingUtilObject
{
    readonly AppDbContext db;

    // filtro activo: Todos, Siguiendo o Sugeridos
    string filtro = "Todos";

    public SearchData(AppDbContext db)
    {
        this.db = db;
        PropertyChanged += SearchData_PropertyChanged;
    }

    // lista de usuarios que se muestra
    ObservableCollection<UserItem> _users = new ObservableCollection<UserItem>();
    public ObservableCollection<UserItem> Users
    {
        get { return _users; }
        set { SetProperty(ref _users, value); }
    }

    // lo que se escribe en el buscador
    string _query = "";
    public string Query
    {
        get { return _query; }
        set { SetProperty(ref _query, value); }
    }

    // el usuario que tocaron en la lista
    UserItem? _selectedUser;
    public UserItem? SelectedUser
    {
        get { return _selectedUser; }
        set { SetProperty(ref _selectedUser, value); }
    }

    // reacciona cuando cambia una propiedad
    private async void SearchData_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // se escribio algo en el buscador
        if (e.PropertyName == nameof(Query))
        {
            Cargar();
        }

        // tocaron un usuario: abrir su perfil pasando el id
        if (e.PropertyName == nameof(SelectedUser) && SelectedUser != null)
        {
            int id = SelectedUser.Id;

            // deseleccionar para poder tocarlo de nuevo despues
            SelectedUser = null;

            await Shell.Current.GoToAsync($"{nameof(UserProfilePage)}?id={id}");
        }
    }

    public void SetFilter(string nuevoFiltro)
    {
        filtro = nuevoFiltro;
        Cargar();
    }

    // llena la lista segun el filtro y lo que escribieron
    public void Cargar()
    {
        User? actual = Session.CurrentUser;

        if (actual == null)
        {
            return;
        }

        // ids de las personas que ya sigo
        List<int> siguiendo = db.Follows
            .Where(f => f.FollowerId == actual.Id)
            .Select(f => f.FollowingId)
            .ToList();

        string texto = "";

        if (Query != null)
        {
            texto = Query.Trim().ToLower();
        }

        ObservableCollection<UserItem> lista = new ObservableCollection<UserItem>();

        foreach (User u in db.Users.Where(u => u.Id != actual.Id).ToList())
        {
            bool losSigo = siguiendo.Contains(u.Id);

            // filtros
            if (filtro == "Siguiendo" && losSigo == false)
            {
                continue;
            }
            if (filtro == "Sugeridos" && losSigo == true)
            {
                continue;
            }

            // buscador por nombre o usuario
            if (texto != "")
            {
                bool coincide = u.Name.ToLower().Contains(texto) ||
                                u.Username.ToLower().Contains(texto);

                if (coincide == false)
                {
                    continue;
                }
            }

            UserItem item = new UserItem();
            item.Id = u.Id;
            item.Name = u.Name;
            item.Handle = "@" + u.Username;
            item.Initial = u.Username.Substring(0, 1).ToUpper();

            if (losSigo)
            {
                item.ButtonText = "Siguiendo";
                item.ButtonColor = Color.FromArgb("#14141A");
                item.ButtonTextColor = Color.FromArgb("#9A9AA5");
            }
            else
            {
                item.ButtonText = "Seguir";
                item.ButtonColor = Color.FromArgb("#FF1F3D");
                item.ButtonTextColor = Colors.White;
            }

            lista.Add(item);
        }

        Users = lista;
    }

    // seguir o dejar de seguir
    public void ToggleFollow(UserItem item)
    {
        User? actual = Session.CurrentUser;

        if (actual == null)
        {
            return;
        }

        Follow? existente = db.Follows.FirstOrDefault(f =>
            f.FollowerId == actual.Id && f.FollowingId == item.Id);

        if (existente != null)
        {
            // dejar de seguir
            db.Follows.Remove(existente);
        }
        else
        {
            // seguir
            Follow nuevo = new Follow();
            nuevo.FollowerId = actual.Id;
            nuevo.FollowingId = item.Id;
            db.Follows.Add(nuevo);
        }

        db.SaveChanges();

        Cargar();
    }
}


// datos de cada usuario tal como se muestra en la lista
public class UserItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Handle { get; set; }
    public string Initial { get; set; }
    public string ButtonText { get; set; }
    public Color ButtonColor { get; set; }
    public Color ButtonTextColor { get; set; }
}