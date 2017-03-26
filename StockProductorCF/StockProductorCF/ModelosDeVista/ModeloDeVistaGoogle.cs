using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using StockProductorCF.Modelos;
using StockProductorCF.Servicios;

namespace StockProductorCF.ModelosDeVista
{
    public class ModeloDeVistaGoogle : INotifyPropertyChanged
    {
        private PerfilGoogle _perfilGoogle;

        public PerfilGoogle PerfilGoogle
        {
            get { return _perfilGoogle; }
            set
            {
                _perfilGoogle = value;
                OnPropertyChanged();
            }
        }

        public async Task EstablecerPerfilUsuarioGoogleAsinc(string tokenDeAcceso)
        {
            var serviciosGoogle = new ServiciosGoogle();

            PerfilGoogle = await serviciosGoogle.ObtenerPerfilGoogleAsinc(tokenDeAcceso);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string nombrePropiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }
    }
}
