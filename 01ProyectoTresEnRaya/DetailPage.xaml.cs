namespace _01ProyectoTresEnRaya;

public partial class DetailPage : ContentPage
{
    /// <summary>
    /// Carga la segunda ventana <see cref="DetailPage"/> y dibuja su canvas segun lo que le pases.
    /// </summary>
    /// <param name="rec">Un objeto de la clase DibujarPrediccion que permite dibujar en Canvas </param>
    public DetailPage(Graficos.DibujarPrediccion rec)
	{
		InitializeComponent();

        // Asigna a la imagen el objeto que reciba en el consturctor que use la interfaz 'IDrawable'
        imagen.Drawable = rec;
	}
}