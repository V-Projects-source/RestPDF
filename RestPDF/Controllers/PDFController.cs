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

using GenPDF;
using RestPDF.Common;
using RestPDF.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;

namespace RestPDF.Controllers{

    /// <summary>
    /// Clase que encapsula el controlador que obtiene las peticiones al servicio.
    /// Analiza los datos recibidos en formato Json y carga los archivos con los tipos de letra que se encuentran en "wwwroot/Fonts".
    /// </summary>
    public class PDFController : ApiController{

        /// <summary>
        /// Método  que obtiene las peticiones Post al servicio, comprueba los datos recibidos en json y llama a la clase para generar el archivo a devolver.
        /// </summary>
        /// 
        /// <param name="data">Datos en Json que corresponden con el modelo "JsonData"</param>
        /// 
        /// <returns>Devuelve un archivo en formato pdf o un mensaje en Json con el modelo "ErrorModel"</returns>
        [HttpPost]// Post vrs/pdf
        public HttpResponseMessage Body(JsonData data){

            LogFile log = new LogFile();
            ErrorModel res = checkInData(data);// Comprobamos los datos de entrada
            var requestId = Request.GetCorrelationId().ToString();

            var path = HostingEnvironment.MapPath(@"\");

            res.requesId = requestId;

            //Si el StatusCode no es OK(200), se devuelve el mensaje con el error
            if (res.statusCode != HttpStatusCode.OK){

                log.writeLog(requestId, (int)res.statusCode, res.msgError, "", path);// Invocamos el método para escribir los datos en el log
                return Request.CreateResponse(res.statusCode, res, Configuration.Formatters.JsonFormatter);
            }

            byte[] bytes = null;
            HttpResponseMessage response;
            DocProperties docProp = new DocProperties();
            string download = "inline";// valores posibles: inline o attachment.
            string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.fff");

            try {

                /*********************************************/
                /* Valores obtenidos por Json en la petición */
                string siteUrl = data.siteUrl;// Url que añadir a los "src" de las imágenes en caso de no tenerlo
                string contenidoHtml = data.contentHtml;// Html con el contenido
                string htmlFromUrl = data.htmlFromUrl;// 

                if (data.download != null && data.download.Trim() != "") {

                    download = data.download;// String que indica como se devuelve el archivo con el pdf generado(inline o attachment)
                }


                if (data.pageType != null && data.pageType.Trim() != "") {

                    docProp.pageType = data.pageType;// Tipo de página: A4, A3, B1, LETTER, etc
                }



                docProp.headerHeight = data.headerHeight;// Altura de la cabecera

                docProp.footerHeight = data.footerHeight;// Altura del pie de página


                if (data.bottomMargin != null && data.bottomMargin != "") {

                    docProp.bottomMargin = float.Parse(data.bottomMargin);// Margen inferior para el contenido
                }

                if (data.topMargin != null && data.topMargin != "") {

                    docProp.topMargin = float.Parse(data.topMargin);// Margen superior para el contenido
                }

                if (data.leftMargin != null && data.leftMargin != "") {
                    docProp.leftMargin = float.Parse(data.leftMargin);// Margen izquierdo para el contenido
                }

                if (data.rightMargin != null && data.rightMargin != "") {
                    docProp.rightMargin = float.Parse(data.rightMargin);// Margen derecho para el contenido
                }

                string urlCSS = data.urlCSS;// Url al archivo con el css
                string css = "";// string con el css para aplicar tanto a "contenidoHtml" como a "pieHtml"

                if (data.fileName != null && data.fileName.Trim() != "") {

                    fileName = data.fileName + "_" + fileName;
                }

                docProp.numberPage = data.numberPage;

                if (data.numberPageSize != 0) { docProp.numberPageSize = data.numberPageSize; }

                if (data.numberPageOf != null && data.numberPageOf.Trim() != "") { docProp.numberPageOf = data.numberPageOf; }

                if (data.numberPageVPos != 0) { docProp.numberPageVPos = data.numberPageVPos; }
                //*********************************************/

                //StreamReader reader = new StreamReader(HostingEnvironment.MapPath(@"\wwwroot\Samples\pcss_chino.html"));
                //contenidoHtml = reader.ReadToEnd();
                //reader = new StreamReader(HostingEnvironment.MapPath(@"\wwwroot\Samples\pie_chino.html"));
                //pieHtml = reader.ReadToEnd();

                // Si se ha pasado por Json la url de donde cargar el css
                if (urlCSS != null && urlCSS.Trim() != "") {

                    StreamReader reader = null;

                    using (var handler = new HttpClientHandler { UseDefaultCredentials = true })// Usamos la credenciales para acceder al archivo en le url
                        reader = new StreamReader(new HttpClient(handler).GetStreamAsync(urlCSS).Result);// Obtenemos el css desde la url

                    css = reader.ReadToEnd();
                }

                // Si se ha pasado por Json la url de donde cargar el Html para el contenido
                if (htmlFromUrl != null && htmlFromUrl.Trim() != "") {

                    WebClient client = new WebClient();

                    client.Credentials = CredentialCache.DefaultCredentials;
                    contenidoHtml = client.DownloadString(htmlFromUrl);// Cambiamos el Html del contenido por el obtenido en la url
                }
                /*********************************************/

                List<string> pathFonts = getDefaultPathFonst();// Tipos de letra para incluir en el pdf

                // Si se han pasado por json las rutas para añadir más tipos de letra
                if (data.pathFonts != null && data.pathFonts != "") {

                    var rutas = data.pathFonts.Split('|');

                    // Para cada ruta a un tipo de letra obtenidos por Json
                    foreach (var ruta in rutas) {
                        pathFonts.Add(ruta);
                    }
                }

                // Creamos el objeto para generar el pdf
                GenerarPDF pdf = new GenerarPDF(pathFonts, siteUrl);

                bytes = pdf.makeHtmlPDF(contenidoHtml, data.headerHtml, data.footerHtml, docProp, css);

            }catch (Exception e){

                res.statusCode = HttpStatusCode.InternalServerError;
                res.msgError = e.InnerException + " " + e.Message + e.StackTrace;

                log.writeLog(requestId, (int)res.statusCode, e.InnerException +" " + e.Message , e.StackTrace, path);// Invocamos el método para escribir los datos en el log

                return Request.CreateResponse(HttpStatusCode.InternalServerError, res, Configuration.Formatters.JsonFormatter);

            }

            /****************************************************************************************/

            MemoryStream stream = new MemoryStream(bytes);// Creamos el stream con los bytes del pdf generado

            // Si se indica que se guarde el archivo en una biblioteca
            if (data.saveFile){

                if (data.librarySiteUrl != null && data.librarySiteUrl.Trim() != "" &&
                    data.fileUrlPath != null && data.fileUrlPath.Trim() != "" &&
                    data.libraryName != null && data.libraryName.Trim() != ""){

                    try{

                        GesFile oGFile = new GesFile(data.librarySiteUrl, data.libraryName);
                        oGFile.saveFile(data.fileUrlPath, fileName + ".pdf", stream, data.fieldInternalNames, data.fieldValues);

                    }catch (Exception e){

                        res.statusCode = HttpStatusCode.InternalServerError;
                        res.msgError = e.Message + e.StackTrace;

                        log.writeLog(requestId, (int)res.statusCode, e.Message, e.StackTrace, path);// Invocamos el método para escribir los datos en el log

                        stream.Close();

                        return Request.CreateResponse(HttpStatusCode.InternalServerError, res, Configuration.Formatters.JsonFormatter);
                    }

                }else{

                    res.statusCode = HttpStatusCode.BadRequest;
                    res.msgError = "librarySiteUrl, fileUrlPath and libraryName must not be empty";

                    stream.Close();

                    return Request.CreateResponse(res.statusCode, res, Configuration.Formatters.JsonFormatter);

                }

            }

            stream.Position = 0;// Ponemos la posición del stream al inicio

            response = new HttpResponseMessage(HttpStatusCode.OK);

            // Si la descarga es "attachment" y se ha pasado el nombre de la cookie que indica al cliente, la finalización de la creación del archivo.
            if (download == "attachment" && data.clientId != null && data.clientId != ""){

                var cookie = new CookieHeaderValue(data.clientId, "true");// Indicamos con la cookie que se ha terminado de generar el archivo
                cookie.Expires = DateTimeOffset.Now.AddDays(1);
                cookie.Path = "/";

                response.Headers.AddCookies(new CookieHeaderValue[] { cookie });
            }

            response.Content = new StreamContent(stream);

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(download);
            response.Content.Headers.ContentDisposition.FileName = fileName + ".pdf";

            return response;
            /****************************************************************************************/

        }

        /// <summary>
        /// Método que incluye los tipos de letra por defecto, para el pdf.
        /// Carga los archivos con los tipos de letra de la carpeta "wwwroot/Fonts".
        /// </summary>
        /// 
        /// <returns>Devuelve un array con strings que contienen las rutas a los tipos de letra</returns>
        private List<string> getDefaultPathFonst(){

            List<string> pathFonts = new List<string>();// Lista con las rutas a los tipos de letra

            string path = HostingEnvironment.MapPath(@"\wwwroot\Fonts\");

            if (Directory.Exists(path)){

                DirectoryInfo directoryInfo = new DirectoryInfo(path);// Obtenemos la información del dircetorio que contiene los tipos de letra

                FileInfo[] fontFiles = directoryInfo.GetFiles();// Obtenemos la información del los archivos del directorio

                // Para cada archivo
                foreach (var fontFile in fontFiles){

                    pathFonts.Add(fontFile.FullName);// Añadimos su ruta                
                }
            }

            return pathFonts;
        }

        /// <summary>
        /// Método que comprueba los datos obtenidos por Json en la petición.
        /// </summary>
        /// 
        /// <param name="data">Objeto con los datos a comprobar</param>
        /// 
        /// <returns>Devuelve un objeto con el mensaje de respuesta</returns>
        private ErrorModel checkInData(JsonData data){

            ErrorModel response = new ErrorModel();

            // Valores de la respuesta si no se han producido errores
            response.statusCode = HttpStatusCode.OK;
            response.msgError = "";

            // Si no se han obtenido datos correctos o no se han podido deserializar
            if (data == null){

                response.statusCode = HttpStatusCode.BadRequest;
                response.msgError = "< Incorrect format data or you are missing some data in the request. > ";

            }else{

                if (data.siteUrl == null || data.siteUrl.Trim() == ""){

                    response.statusCode = HttpStatusCode.BadRequest;
                    response.msgError = "< siteUrl is empty > ";
                }

                if ((data.contentHtml == null || data.contentHtml.Trim() == "") && (data.htmlFromUrl == null || data.htmlFromUrl.Trim() == "")){

                    response.statusCode = HttpStatusCode.BadRequest;
                    response.msgError += "< contentHtml is empty > ";
                }

                if (data.download == null || data.download.Trim() == "" || (data.download != "inline" && data.download != "attachment")){

                    response.statusCode = HttpStatusCode.BadRequest;
                    response.msgError += "< download must be: inline or attachment > ";

                }

            }

            return response;

        }

    }
}
