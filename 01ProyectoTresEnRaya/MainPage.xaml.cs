using _01ProyectoTresEnRaya.Graficos;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;
using Rect = Microsoft.Maui.Graphics.Rect;

using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using xImage = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using CommunityToolkit.Maui.Views;


namespace _01ProyectoTresEnRaya
{
    public partial class MainPage : ContentPage
    {
        // Imagen de ImageSharp
        private xImage? imagenPlugin = null;

        // Botones en el tablero que estan pulsados (tienen contenido y han sido jugados)
        private List<ImageButton> botonesPulsados = new List<ImageButton>();

        // Los botones que hay en el tablero
        private List<ImageButton> listaBotones;

        // Imagen de 'x' u 'o'
        private (ImageSource o, ImageSource x) imagenes = (
            ImageSource.FromResource("_01ProyectoTresEnRaya.Resources.Images.o.png"),
            ImageSource.FromResource("_01ProyectoTresEnRaya.Resources.Images.x.png")
        );

        // Almacena un dibujo de la clase DibujarPrediccion implementando IDrawable para su posterior uso
        private DibujarPrediccion? rec = null;

        // Almacena si es la primera vez que va a relizar una predicción. 
        private bool primeraEjec = true;

        public MainPage()
        {
            InitializeComponent();
            // Rellena la lista de botones con los botones que encuentra en el tablero
            listaBotones = tablero.Children.OfType<ImageButton>().ToList();

            // Asigna por defecto el turno a 'x'
            img_turno.Source = imagenes.x;
        }

        /// <summary>
        /// Ofrece al usuario capturar una imágen a traves de una de las cámaras del dispositivo.
        /// </summary>
        /// <returns>Una promesa de un flujo de datos con la foto</returns>
        private async Task<Stream?> CapturarFoto()
        {
            Stream? sourceStream = null;
            // Comprueba si su dispositivo cuenta con alguna cámara
            if (MediaPicker.Default.IsCaptureSupported)
            {
                // Toma una foto y a mete a un flujo de datos
                FileResult? photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo != null)
                {
                    // Almacenta la foto en un flujo de Datos y pone el puntero a 0 
                    sourceStream = await photo.OpenReadAsync();
                    sourceStream.Position = 0;
                }
            }
            else
            {
                // Si no tiene cámara en su dispositivo se le notifica al usuario
                await DisplayAlert("ERROR", "Tu dispositivo no tiene ninguna cámara disponible", "OK");
            }
            return sourceStream;
        }
        private async void btn_generar_Clicked(object sender, EventArgs e)
        {
            // Por si queda algo de residuo limpia el tablero
            LimpiarTablero();

            // Toma una foto
            Stream? flujo = await CapturarFoto();
            if (flujo != null)
            {
                // Si el usuario ha confirmado la foto que tomó, va a tratar de generar una predicción
                generarPrediccion(flujo); 
            }
        }

        /// <summary>
        /// Genera una predicción, recibiendo una imagen y opera en consecuencia.
        /// </summary>
        /// <param name="stream">The stream con la imágen</param>
        private async void generarPrediccion(Stream stream)
        {
            // Muestra un PopUp indicando que esta cargando
            LoadingPopup popup = new LoadingPopup();
            this.ShowPopup(popup);

            // Trabajo con la IA
            // Creamos un obejto de Azure
            Azure azure = new Azure();

            // Si es la primera vez que va a generar una predicción, muestra una pequeña información al usuario
            if (primeraEjec)
            {
                await DisplayAlert("INFORMACIÓN", "Pulsa en la imágen de arriba para cambiar el turno", "Ok");
                primeraEjec = false;
            }

            // Trabaja con la imagen que se le ha pasado para adaptarla al ancho del <ContentPage> y dependiendo de la plataforma en 
            // que se ejecute lo hace de una u otra manera
            IImage image;
            xImage? xImagen = null;
            if (DevicePlatform.Android == DeviceInfo.Current.Platform) 
            { 
                image = PlatformImage.FromStream(stream);
                
                image = image.Downsize((int)contentPage.Height, false);
                // La rota 90º porque por defecto la imagen que da el MediaPicker en Android es apaisada (excepto en el móvil de Ángel)
                xImagen = ConvertirIImageToXImage(image);
                xImagen.Mutate(
                     x => x.Rotate(90)
                );
                image = ConvertirxImageToIImage(xImagen);
            } else 
            {
                // Hay que hacerlo distino proque el método Downsize() no funciona en Windows
                image = generarIImage(stream);
                xImagen = ConvertirIImageToXImage(image);
            }

            try
            {
                // Obetenemos la predicción
                ImagePrediction? res = await azure.GetImagePrediction(image.AsStream());
                if (res != null)
                {
                    // Iteralmos las predicciones y si tiene mas de un 80% de probabilidad trabajamos con esa predicción. 
                    List<Rectangulo> rectangulos = new List<Rectangulo>();
                    foreach (PredictionModel e in res.Predictions)
                    {
                        if (e.Probability > 0.8)
                        {
                            // Obtenemos la posición donde se encuentra la imágen
                            BoundingBox o = e.BoundingBox;

                            // Las dimensiones son respecto a los ancho y alto de la imagen por es se multuplica
                            (double, double, double, double) medias = (o.Left * image.Width, o.Top * image.Height, o.Width * image.Width, o.Height * image.Height);

                            // Necesito el rectángulo en el formato de SixLabors y enformato de .NET MAUI, para trabajar en distintas operaciones
                            // ya que no hay retrocompatibilidad entre ambos
                            Rectangulo rectangulo = new Rectangulo(
                                    new Rect(medias.Item1, medias.Item2, medias.Item3, medias.Item4),
                                    new Rectangle((int)medias.Item1, (int)medias.Item2, (int)medias.Item3, (int)medias.Item4),
                                    e.TagName,
                                    e.Probability
                                );

                            // Comprobar que si ya existe que la probabilidad de éxito sea mayor quel existente en la lista de predicciones
                            // y si es así, sustituirla
                            Rectangulo? s;
                            if ((s = rectangulos.Find(x => x.Ubicacion == rectangulo.Ubicacion)) != null)
                            {
                                if (s.Probabilidad < rectangulo.Probabilidad)
                                {
                                    rectangulos.Remove(s);
                                    rectangulos.Add(rectangulo);
                                }
                            } else
                            {
                                rectangulos.Add(rectangulo);
                            }
                        }
                    }

                    // A partir de aqui, en teoria tenemos una Lista con los 9 objetos Rectangulo que representan cada posición

                    foreach (Rectangulo item in rectangulos)
                    {
                        // Clonamos la imagen para no dañar la original
                        using Image<Rgba64> clone = xImagen.CloneAs<Rgba64>();

                        // Recortamos la iamgen clonada segun el Rectángulo de la predicción actial
                        clone.Mutate(
                            o => o.Crop(item.XRectangulo)
                        );

                        // Creamos un flujo de memoria y sacamos que ficha ('x','o' o 'Nulo') contiene esa casilla y lo añade a 
                        // su respectivo objeto de la clase Rectangulo
                        MemoryStream ms = new MemoryStream();
                        clone.Save(ms, JpegFormat.Instance);
                        ms.Seek(0, SeekOrigin.Begin);

                        ImagePrediction XoY = await azure.GetImagePredictionXoY(ms);
                        item.Tipo = XoY.Predictions[0].TagName.ToCharArray()[0];

                        // Cerramos Flujo
                        ms.Dispose();
                        ms.Close();
                    }
                    // Rellenamos el tablero de Juego en función de la informaciónr recabada
                    RellenarTablero(rectangulos);

                    // Alamcenamos el resultado en formato Canvas para su posterior uso
                    rec = new DibujarPrediccion(rectangulos, image);
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                // Si no tiene internet se le informa al usuario
                await DisplayAlert("ERROR", "Error de conexión. Asegurate de tener conexión a Internet", "OK");
            }
            catch (Exception)
            {
                await DisplayAlert("ERROR", "Error al intentar crear la prediccón. Pongase en contacto con el matao que te haya hecho esto", "OK");
            }

            // Cerrar conexión con el Servidor Azure
            azure.cerrarCliente();

            // Cerrar el PopUp de cargando
            await popup.CloseAsync();
        }

        /// <summary>
        /// Recibe un flujo de datos y devuevle una Iimage de NET MAUI
        /// </summary>
        /// <param name="flujo">The flujo de datos</param>
        /// <returns>Imagen en NET MAUI</returns>
        private IImage generarIImage(Stream flujo)
        {
            // Generamos la Imagen desde el flujo
            IImage imagenOriginal = PlatformImage.FromStream(flujo);

            // Actualizamos su equivalente en ImageSharp
            imagenPlugin = ConvertirIImageToXImage(imagenOriginal);

            // Adaptamos la imagen a la pantalla
            IImage image = AdaptarImagenPantalla();
            return image;
        }
        /// <summary>
        /// Adapta la imagenPlugin a la pantalla
        /// </summary>
        /// <returns>Una iamgen de NET MAUI</returns>
        private IImage AdaptarImagenPantalla()
        {
            // Calculamos la relación de aspecto
            double ratio = Math.Max(imagenPlugin.Width / (double) contentPage.Width, imagenPlugin.Height / (double)contentPage.Height);

            // Adaptamos la imágen a la Relación de aspecto del <ContentPane>
            imagenPlugin.Mutate(
                    x => x.Resize((int)(imagenPlugin.Width / ratio), (int)(imagenPlugin.Height / ratio))
                );

            return ConvertirxImageToIImage(imagenPlugin);
        }

        /// <summary>
        /// Recibe una lista de triangulos y rellena el tablero de juego.
        /// </summary>
        /// <param name="rectangulos">Una lista de rectángulos.</param>
        private void RellenarTablero(List<Rectangulo> rectangulos)
        {
            foreach (var item in rectangulos)
            {
                // 'N' significa que no hay nada ubicado en esa casilla
                if (item.Tipo != 'N')
                {
                    // Seleccionamos el boton pulsado
                    ImageButton boton;
                    switch (item.Ubicacion)
                    {
                        case "Abajo_centro":
                            boton = lbl_21;
                            break;
                        case "Abajo_dcha":
                            boton = lbl_31;
                            break;
                        case "Abajo_izda":
                            boton = lbl_11;
                            break;
                        case "Arriba_centro":
                            boton = lbl_23;
                            break;
                        case "Arriba_dcha":
                            boton = lbl_33;
                            break;
                        case "Arriba_izda":
                            boton = lbl_13;
                            break;
                        case "Centro":
                            boton = lbl_22;
                            break;
                        case "Centro_dcha":
                            boton = lbl_32;
                            break;
                        case "Centro_izda":
                            boton = lbl_12;
                            break;
                        default:
                            boton = new ImageButton();
                            break;
                    }
                    // Pulsa el boton rellenado y lo rellena con la imágen del que toca
                    ImageSource imagen = item.Tipo == 'x' ? imagenes.x : imagenes.o;
                    pulsarBoton(boton, imagen, true);
                }
            }
            // Al finalizar, comprueba si hay un ganador
            ComprobarJuego();
        }

        /// <summary>
        /// Convierte una Imagen de MAUI a una de ImageSharp.
        /// </summary>
        /// <param name="mauiImage">La imagen MAUI.</param>
        /// <returns>La iamgen pasada a ImageSharp</returns>
        public xImage ConvertirIImageToXImage(IImage mauiImage)
        {
            return xImage.Load<Rgba64>(mauiImage.AsStream());
        }
        /// <summary>
        /// Convierte una Imagen de ImageSharp a una de MAUI .
        /// </summary>
        /// <param name="imageSix">La iamgen de ImageSharp.</param>
        /// <returns>La iamgen pasada a .NET MAUI</returns>
        public IImage ConvertirxImageToIImage(xImage imageSix)
        {
            // Crea un flujo de memoria y almacena la imagen de ImageSharp ahí
            using var ms = new MemoryStream();
            imageSix.Save(ms, new PngEncoder());

            // Posiciona el puntero en el inicio del flujo
            ms.Seek(0, SeekOrigin.Begin);

            // devueve la Imagen de MAUI creada desde ese flujo
            return PlatformImage.FromStream(ms);
        }

        private void BotonPulsado(object sender, EventArgs e)
        {
            // Obtiene el boton que ha sido pulsado
            ImageButton btn = (ImageButton) sender;

            // Cambia el turno del jugador 
            ImageSource imagen = img_turno.Source == imagenes.o ? imagenes.o : imagenes.x;

            // Pulsa el boton
            pulsarBoton(btn, imagen, false);
        }

        /// <summary>
        /// Simula que el usuario ha pulsado un boton.
        /// </summary>
        /// <param name="btn">El boton del tablero que ha pulsado</param>
        /// <param name="imagen">La imagen del turno que habia.</param>
        /// <param name="generando">Si se esta generando con IA</param>
        private void pulsarBoton(ImageButton btn, ImageSource imagen, bool generando) 
        {
            // Actualiza el botón para que se personalice segun la ficha tocada y lo añade a la lista de botones pulsados
            btn.Source = imagen;
            btn.BackgroundColor = imagen == imagenes.x ? Colors.Pink : Colors.LightBlue;
            btn.IsEnabled = false;

            botonesPulsados.Add(btn);
            img_turno.Source = img_turno.Source == imagenes.o ? imagenes.x : imagenes.o;

            // Si no esta generando, tambien comprueba si hay un ganador
            if (!generando)
            {
                ComprobarJuego();
            }
        }
        /// <summary>
        /// Comprueba si ha ganado alguien
        /// </summary>
        private void ComprobarJuego()
        {
            // Comprueba si x o o ha ganado
            bool xGano = HaGanado(imagenes.x);
            bool oGano = HaGanado(imagenes.o);

            // En dependencia de quien a ganado o si ya no se puede colocar ninguna ficha muestra un mensaje
            if (xGano)
            {
                this.ShowPopup(new PopUpGanador(imagenes.x));
                FinJuego();
            }
            else if (oGano)
            {
                this.ShowPopup(new PopUpGanador(imagenes.o));
                FinJuego();
            }
            else if (botonesPulsados.Count >= 9)
            {
                this.ShowPopup(new PopUpGanador(null));
                FinJuego();
            }
        }

        /// <summary>
        /// Pone a nulos todos los botones para que no se pueda poner ninguna ficha mas
        /// </summary>
        private void FinJuego()
        {
            // Pone a nulos todos los botones para que no se pueda poner ninguna ficha mas
            listaBotones.ForEach(item => item.IsEnabled = false);
        }
        /// <summary>
        /// Comprueba si x o y ha ganado
        /// </summary>
        /// <param name="x">Una imagen que represetna x o y</param>
        /// <returns>Si ha ganado esas fichas</returns>
        private bool HaGanado(ImageSource x)
        {
            bool gano = false;
            // Verificar líneas horizontales
            if ((lbl_11.Source == x && lbl_21.Source == x && lbl_31.Source == x) ||
                (lbl_12.Source == x && lbl_22.Source == x && lbl_32.Source == x) ||
                (lbl_13.Source == x && lbl_23.Source == x && lbl_33.Source == x))
            {
                gano = true;
            }
            else // Verificar líneas verticales
            if ((lbl_11.Source == x && lbl_12.Source == x && lbl_13.Source == x) ||
                (lbl_21.Source == x && lbl_22.Source == x && lbl_23.Source == x) ||
                (lbl_31.Source == x && lbl_32.Source == x && lbl_33.Source == x))
            {
                gano = true;
            }
            else // Verificar diagonales
            if ((lbl_11.Source == x && lbl_22.Source == x && lbl_33.Source == x) ||
                (lbl_13.Source == x && lbl_22.Source == x && lbl_31.Source == x))
            {
                gano = true;
            }

            return gano;
        }

        private void btn_reiniciar_Clicked(object sender, EventArgs e)
        {
            // Si pulsas el botón de reiniciar, se limpia el tablero.
            LimpiarTablero();
        }

        /// <summary>
        /// Limpia todos los botones del tablero y lo deja a estado por defecto.
        /// </summary>
        private void LimpiarTablero()
        {
            // Recorre todos los botones y los activa, los quita la imagen y el fondo
            foreach (ImageButton item in listaBotones)
            {
                item.IsEnabled = true;
                item.Source = null;
                item.BackgroundColor = Colors.Gray;
            }

            // Limpia la lista de botones pulsados y asigna el turno de X
            botonesPulsados.Clear();
            img_turno.Source = imagenes.x;
        }

        private async void btn_cargarImagen_Clicked(object sender, EventArgs e)
        {
            // Si no ha generado ninguna imagen avisa al usuario, si no carga la ventana con la imágen generada 
            if (rec == null)
            {
                await DisplayAlert("AVISO", "Primero genera una predicción pulsando en 'Capturar'", "Ok");
            } else
            {
                await Navigation.PushAsync(new DetailPage(rec));
            }
        }

        private void img_turno_Clicked(object sender, EventArgs e)
        {
            // Si pulsas la imágen, cambia el turno
            img_turno.Source = img_turno.Source == imagenes.o ? imagenes.x : imagenes.o;
        }
    }
}
