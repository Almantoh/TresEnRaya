using CommunityToolkit.Maui.Views;

namespace _01ProyectoTresEnRaya;

public partial class PopUpGanador : Popup
{
    /// <summary>
    /// Crea un <see cref="PopUpGanador"/> indicando quien en el ganador seg�n la im�gen que recibe.
    /// </summary>
    /// <param name="image">The imagen de la ficha ganadora</param>
    public PopUpGanador(ImageSource? image)
	{
		InitializeComponent();
        // Si la imagen es nulo, se entiende que los jugadores han empatado
        if (image == null)
        {
            // Quita una columna del grid para ajustar mejor el texto al ancho
            grido.ColumnDefinitions.RemoveAt(1); 

            // Modfifica el mensaje por defecto del label y le asigna un tama�o mas peque�o
            lbl_info.Text = "Como diria un sabio: 'Fin del Juego'";
            lbl_info.FontSize = 14;
        } else
        {
            // Asgina la imagen de un ganador (en este caso 'x' o 'o')
            img_turno.Source = image;
        }
	}
    private void Button_Clicked(object sender, EventArgs e)
    {
        // Cierra la ventana si el usuario pulsa en Aceptar
        this.CloseAsync();
    }
}