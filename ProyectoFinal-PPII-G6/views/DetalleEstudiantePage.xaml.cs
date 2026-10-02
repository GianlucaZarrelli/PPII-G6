using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views
{
    public partial class DetalleEstudiantePage : ContentPage
    {
        public DetalleEstudiantePage(DetalleEstudianteViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
