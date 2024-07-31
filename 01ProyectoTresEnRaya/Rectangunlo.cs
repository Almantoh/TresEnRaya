using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _01ProyectoTresEnRaya
{
    /// <summary>
    /// Calse que encapsula todos los datos de una casilla en el tablero
    /// </summary>
    public class Rectangulo
    {
        // Un Rectangulo de MAUI para dibujarlo en la prediccion
        private Rect rect; 

        // Un Rectangulo de ImageSharp para trabajar con la imagen
        private Rectangle xRectangulo;

        // La ubicación en el tablero de esa casilla 
        private string ubicacion;

        // La probablilidad de que esa esa la correcta ubicación de esa casilla
        private double probabilidad;

        // El contenido que tiene el triangulo. Puede ser o 'N', 'o' u 'x'
        private char tipo;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="Rectangulo"/>.
        /// </summary>
        /// <param name="rect">Un Rectangulo de MAUI para dibujarlo en la prediccion.</param>
        /// <param name="xRectangulo">Un Rectangulo de ImageSharp para trabajar con la imagen.</param>
        /// <param name="tipo">La ubicación en el tablero de esa casilla</param>
        /// <param name="probabilidad">La probablilidad de que esa esa la correcta ubicación de esa casilla.</param>
        public Rectangulo(Rect rect, Rectangle xRectangulo, string tipo, double probabilidad)
        {
            this.rect = rect;
            this.xRectangulo = xRectangulo;
            this.ubicacion = tipo;
            this.probabilidad = probabilidad;
        }

        public Rect Rect { get => rect; set => rect = value; }
        public string Ubicacion { get => ubicacion; set => ubicacion = value; }
        public double Probabilidad { get => probabilidad; set => probabilidad = value; }
        public Rectangle XRectangulo { get => xRectangulo; set => xRectangulo = value; }
        public char Tipo { get => tipo; set => tipo = value; }

        public override bool Equals(object? obj)
        {
            return obj is Rectangulo rectangunlo &&
                   ubicacion == rectangunlo.ubicacion;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(rect, xRectangulo, ubicacion, probabilidad);
        }
    }
}
