/* RestPDF 

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.*/

using System;
using System.IO;

namespace RestPDF.Common {

    /// <summary>
    /// Clase que encapsula los métodos para la creación de un archivo de registro(Log)
    /// </summary>
    public class LogFile {

        /// <summary>
        /// Método que añade una línea de texto al archivo con el log.
        /// </summary>
        /// <param name="pathFile">string con la ruta y nombre del archivo</param>
        /// <param name="line">string con el contenido de la línea a añadir</param>
        private void addToFile(string pathFile, string line) {

            try {

                using (StreamWriter escritor = new StreamWriter(pathFile, true)) {

                    escritor.WriteLine(line);
                    escritor.Close();

                }

            } catch (IOException e) {

                Console.WriteLine("Error: " + e.Message);
            }

        }

        /// <summary>
        /// Método que escribe los datos de un error en el arhivo Log.
        /// </summary>
        /// <param name="RequestId">El id de la petición</param>
        /// <param name="StatusCode">El código del error</param>
        /// <param name="msgError">Mensaje con el error</param>
        /// <param name="masInfo">Información adicional del error</param>
        /// <param name="ruta">ruta para el archivo log</param>
        public void writeLog(string RequestId, int StatusCode, string msgError, string masInfo, string ruta) {

            var satoLinea = Environment.NewLine;

            string logDirName = ruta + "\\log";

            Directory.CreateDirectory(logDirName);// Si no existe el directorio, se crea.

            string rutaLog = logDirName + "\\" + "log_" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt";

            var texto = "[ " + DateTime.Now.ToString() + " ]" + satoLinea;// Fecha en la que se ha producido la excepción
            texto += "Request ID: " + RequestId + satoLinea;// El id de la petición
            texto += "Status Code: " + StatusCode + satoLinea;// El código del error producido
            texto += msgError + satoLinea;// Mensaje del la excepción producida
            texto += masInfo + satoLinea;// Mensaje del la excepción producida

            addToFile(rutaLog, texto);// Ponemos una linea en blanco

        }

    }

}