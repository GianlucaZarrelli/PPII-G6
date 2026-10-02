using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views;

public partial class EstudiantePage : ContentPage
{
	public EstudiantePage(EstudianteViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}