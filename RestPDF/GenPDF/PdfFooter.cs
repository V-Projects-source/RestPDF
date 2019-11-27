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

using System.Collections.Generic;
using iText.Html2pdf;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Colors;
using iText.Layout.Properties;
using RestPDF.Models;

namespace GenPDF{

    /// <summary>
    /// Clase que encapsula la generación del pie de página para el documento.
    /// Implementa la interface IEventHandler
    /// </summary>
    public class PdfFooter : IEventHandler{

        protected PdfDocument pdf;/**< Objeto que contiene el documento en el que generar el footer */

        private IList<IElement> footerElements;/**< Elementos extraidos del html para poner en el footer */

        protected DocProperties docProp;/**< Objeto con las propiedades para el documento */

        /// <summary>
        /// Constructor 
        /// </summary>
        /// 
        /// <param name="pdf">Objeto con el Documento pdf al que añadir el pie de página</param>
        /// <param name="pieHtml">String con el Html para el pie de página</param>
        /// <param name="cprop">Objeto con los tipos de letra</param>
        /// <param name="docProp">Objeto con las propiedades para el documento</param>
        public PdfFooter(PdfDocument pdf, string pieHtml, ConverterProperties cprop, DocProperties docProp) {

            this.pdf = pdf;

            this.docProp = docProp;

            footerElements = HtmlConverter.ConvertToElements(pieHtml, cprop);

        }

        /// <summary>
        /// Método que se ejecuta cada vez que sucede el evento asignado(END_PAGE) añadiendo el pie de página.
        /// </summary>
        /// <param name="ev">Datos sobre el evento</param>
        public void HandleEvent(Event ev) {

            PdfDocumentEvent docEvent = (PdfDocumentEvent) ev;
            PdfDocument pdf = docEvent.GetDocument();// Obtenemos el documento 
            
            PdfPage page = docEvent.GetPage();// Obtenemos la página 

            Rectangle pageSize = page.GetPageSize();// Obtenemos las dimensiones de la página
            PdfCanvas pdfCanvas = new PdfCanvas(page.GetLastContentStream(), page.GetResources(), pdf);// Creamos la zona donde se colocará el pie

            float anchura = pageSize.GetWidth() - (docProp.leftMargin + docProp.rightMargin);// Obtenemos la anchura que tendrá la zona del pie

            Canvas canvas = new Canvas(pdfCanvas, pdf, new Rectangle(docProp.leftMargin, 0, anchura, docProp.footerHeight));// Generamos el canvas para la zona del pie

            foreach (IElement footerElement in footerElements) {

                // Nos aseguramos que el tipo del elemento sea "IBlockElement"
                if(typeof(IBlockElement).IsInstanceOfType(footerElement)){

                    canvas.Add( (IBlockElement)footerElement );// Colocamos los elementos del pie en la zona asignada en la página
                }

            }

            pdfCanvas.Release();
        }

    }
}