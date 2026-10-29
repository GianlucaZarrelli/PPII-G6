using Foundation;
using Microsoft.Maui;

namespace ProyectoFinal_PPII_G6
{
    /// <summary>
    /// Delegado de escena de Mac Catalyst. Xcode 27 exige el ciclo de vida por escenas:
    /// MAUI crea la ventana de la aplicación a través de esta clase.
    /// El nombre registrado tiene que coincidir con UISceneDelegateClassName en Info.plist.
    /// </summary>
    [Register("SceneDelegate")]
    public class SceneDelegate : MauiUISceneDelegate
    {
    }
}