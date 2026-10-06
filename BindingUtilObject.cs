using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Arkana
{
    public class BindingUtilObject : INotifyPropertyChanged
    {
        // avisa a la pantalla que una propiedad cambio
        public void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ayuda: guarda el valor y avisa solo si de verdad cambio
        protected bool SetProperty<T>(ref T campo, T valor, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(campo, valor))
            {
                return false;
            }

            campo = valor;
            RaisePropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}