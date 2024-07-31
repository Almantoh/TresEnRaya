using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;


namespace _01ProyectoTresEnRaya
{
    public class Azure
    {
        // Claves para acceder a mi Key de la API de Custom Vision e indicar el Endpoint
        private string Endpoint = "URL de la API de Azure";
        private string PredictionKey = "KEY de la API de Azure";

        // ID del proyecto de Identificar XoY, junto con la itereación publicada a la que accede
        private string projectID = "aad225d4-7cec-4e99-91f1-442343c57719";
        private string PublishedName = "Iteration2";

        // Cliente para realizar solicitudes
        private CustomVisionPredictionClient cliente;

        /// <summary>
        /// Inicializa el cliente encargado de hacer consultas a la API de Azure.
        /// </summary>
        public Azure()
        {
            // Iniciaizamos el cliente con los datos de la clase
            cliente = new CustomVisionPredictionClient( new ApiKeyServiceClientCredentials(PredictionKey) )
            { Endpoint = Endpoint };
        }

        /// <summary>
        /// Genera una predicción sobre si esa imagen es 'x' o 'y', llamando a la API de Custom Vision
        /// </summary>
        /// <param name="stream">Un flujo con la imagen</param>
        /// <returns>La prediccion</returns>
        public async Task<ImagePrediction> GetImagePredictionXoY(Stream stream)
        {
            // Clasifica la imagen que recibe del flujo con los datos asignados anteriormente, guardando el resultado en una variabe
            ImagePrediction prediction = await cliente.ClassifyImageAsync(Guid.Parse(projectID), PublishedName, stream);
            return prediction;
        }

        /// <summary>
        /// Genera una predicción sobre la posición del tablero llamando a la API de Custom Vision
        /// </summary>
        /// <param name="stream">Un flujo con la imagen</param>
        /// <returns>La prediccion</returns>
        public async Task<ImagePrediction> GetImagePrediction(Stream stream)
        {
            // ID del proyecto de Identificar cada casilla en el tablero, junto con la itereación publicada a la que accede
            string ProjectID = "74a82736-c574-4dc3-a3a8-dc5da954a095"; //https://aad.portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/Overview
            string PublishedName = "Iteration2";

            // sifica la imagen que recibe del flujo con los datos asignados anteriormente, guardando el resultado en una variabe
            ImagePrediction prediction = await cliente.DetectImageAsync(Guid.Parse(ProjectID), PublishedName, stream);
            return prediction;
        }

        public void cerrarCliente()
        {
            // Una vez no necesites hacer mas predicciones, libera recursos 
            cliente.Dispose();
        }
    }
}
