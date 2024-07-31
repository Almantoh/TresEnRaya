using Camera.MAUI;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace _01ProyectoTresEnRaya
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCameraView() // Para permitir el uso de la camara
                .UseMauiCommunityToolkit() // Para incluir los elementos del Community Toolki
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
