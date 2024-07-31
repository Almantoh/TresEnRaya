using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IImage = Microsoft.Maui.Graphics.IImage;


namespace _01ProyectoTresEnRaya.Graficos
{
    public class DibujarPrediccion : IDrawable
    {
        public List<Rectangulo> rectangulos;
        public IImage imagen;

        public DibujarPrediccion(List<Rectangulo> rectangunlos, IImage imagen)
        {
            this.rectangulos = rectangunlos;
            this.imagen = imagen;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.DrawImage(imagen, 0, 0, imagen.Width  , imagen.Height);

            canvas.StrokeSize = 2;
            canvas.StrokeColor = Colors.Red;
            canvas.FontColor = Colors.White;

            foreach (Rectangulo rect in rectangulos)
            {
                int ancho = DevicePlatform.Android == DeviceInfo.Current.Platform ? 110:130 ;
              
                Rect rec = new Rect(rect.Rect.X , rect.Rect.Y  - 15, ancho, 15);
                canvas.SetFillPaint(new SolidPaint(Colors.Red), rec);
                canvas.FillRoundedRectangle(rec, 0, 0);

                string mensaje = $"{rect.Ubicacion}: {Math.Round(rect.Probabilidad * 100, 2)}% -{rect.Tipo}";


                canvas.DrawString(mensaje, (int)rect.Rect.X + 3, (int)rect.Rect.Y - 3, HorizontalAlignment.Left);

                canvas.DrawRectangle(rect.Rect);
            }
        }
    }
}
