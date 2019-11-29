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

namespace RestPDF.Models {

    /// <summary>
    /// Clase que encapsula el modelo de datos que se obtiene por Json
    /// </summary>
    public class JsonData {

        /// <summary>Nombre para el archivo pdf</summary>
        public string fileName { get; set; }

        /// <summary>Url raíz donde se encuentran las imágenes</summary>
        public string siteUrl { get; set; }

        /// <summary>string con el html para el contenido de las páginas</summary>
        public string contentHtml { get; set; }

        /// <summary>string con el html para la cabecera</summary>
        public string headerHtml { get; set; }

        /// <summary>string con el html para el pie</summary>
        public string footerHtml { get; set; }

        /// <summary>Url donde leer el archivo con el css</summary>
        public string urlCSS { get; set; }

        /// <summary>string que indica la forma de enviar el pdf generado: inline o attachment</summary>
        public string download { get; set; }

        /// <summary>string con la url para obtener el html para el contenido</summary>
        public string htmlFromUrl { get; set; }

        /// <summary>Nombre de la cookie que indica si se ha terminado de generar el archivo</summary>
        public string clientId { get; set; }

        /****************************************************/
        /* <summary>Propiedades para el documento a generar */
        public string topMargin { get; set; }/**< Indica el margen superior que tendrá la página en pt. */

        public string leftMargin { get; set; }/**< Indica el margen que tendrá la página a izquierda en pt. */

        public string rightMargin { get; set; }/**< Indica el margen que tendrá la página a derecha en pt. */

        public string bottomMargin { get; set; }/**< Indica el margen inferior para el contenido de la página en pt. El cual dejará espacio para el pie de página. */

        public float headerHeight { get; set; }/**< Indica la altura que tenrá la cabecera de página en pt. */

        public float footerHeight { get; set; }/**< Indica la altura que tenrá el pie de página en pt. */

        public string pageType { get; set; }/**< Indica el formato de la página: A2, A3, A4, etc. */

        public bool numberPage { get; set; }/**< Indica si se muestra el número y total de página */

        public int numberPageSize { get; set; }/**< Indica el tamaño de la fuente para en número de página */

        public string numberPageOf { get; set; }/**< Indica el o los caracteres que se ponen entre el número de página y el total de páginas. ej: 1 of 6. */

        public int numberPageVPos { get; set; }/**< Inica la posición vertical donde se coloca el número de página. Por defecto su valor es 30 */
        /****************************************************/

        /// <summary>Rutas de tipos de letra que se quieran añadair a los que se incorporan por defecto</summary>
        public string pathFonts { get; set; }

        /*********************************************************/
        /* Parámetros para guardar el PDF generado en un archivo */
        public bool saveFile { get; set; } /**< Indica si el pdf generado se guarda en una biblioteca */

        public string librarySiteUrl { get; set; }/**< Indica la url del sito donde se encuentra la biblioteca */

        public string fileUrlPath { get; set; }/**< Url relativa al lugar donde guardar el archivo, incluida en la ruta el nombre de la biblioteca y carpeta si existe */

        public string libraryName { get; set; }/**< Nombre de la biblioteca */

        public string fieldInternalNames { get; set; }/**< Nombres internos de los campos a actualizar */

        public string fieldValues { get; set; }/**< Valores a actualizar en los campos */
        /*********************************************************/
    }
}