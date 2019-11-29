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

namespace GenPDF {

    /// <summary>
    /// Clase que encapsula las propiedades para una página del documento
    /// </summary>
    public class DocProperties {

        public float topMargin = 30;/**< Indica el margen superior que tendrá la página en pt. */

        public float leftMargin = 37;/**< Indica el margen que tendrá la página a izquierda en pt. */

        public float rightMargin = 37; /**< Indica el margen que tendrá la página a derecha en pt. */

        public float bottomMargin = 30; /**< Indica el margen inferior para el contenido de la página en pt. El cual dejará espacio para el pie de página. */

        public float headerHeight = 0;/**< Indica la altura que tenrá la cabecera de página en pt. */

        public float footerHeight = 0;/**< Indica la altura que tenrá el pie de página en pt. */

        public string pageType = "A4";/**< Indica el formato de la página: A2, A3, A4, etc. */

        public bool numberPage = false;/**< Indica si se muestra el número y total de página */

        public int numberPageSize = 8;/**< Indica el tamaño de la fuente para en número de página */

        public string numberPageOf = "";/**< Indica el o los caracteres que se ponen entre el número de página y el total de páginas. ej: 1 of 6. Si no se indica solo se pone el número de página */

        public int numberPageVPos = 30;/**< Inica la posición vertical donde se coloca el número de página. Por defecto su valor es 30 */
    }
}