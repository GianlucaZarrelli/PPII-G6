using ProyectoFinal_PPII_G6.ViewModels;

namespace ProyectoFinal_PPII_G6.Views;

/// <summary>
/// Página de carga masiva de alumnos: elegir comisión y archivo CSV, revisar la vista previa e importar.
/// </summary>
public partial class CargaMasivaPage : ContentPage
{
    private readonly CargaMasivaViewModel _viewModel;

    /// <summary>
    /// Crea la página y le asigna su ViewModel.
    /// </summary>
    public CargaMasivaPage(CargaMasivaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Recarga las comisiones cada vez que se muestra la pantalla.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarComisionesAsync();
    }
}