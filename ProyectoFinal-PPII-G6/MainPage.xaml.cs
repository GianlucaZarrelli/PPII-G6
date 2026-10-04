namespace ProyectoFinal_PPII_G6
{
    /// <summary>
    /// Página de ejemplo de la plantilla de MAUI. No se usa en la aplicación.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Cantidad de clics en el botón.
        /// </summary>
        int count = 0;

        /// <summary>
        /// Inicializa la página.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Suma un clic y actualiza el texto del botón.
        /// </summary>
        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}
