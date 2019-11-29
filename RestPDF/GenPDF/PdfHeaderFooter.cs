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
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;

namespace GenPDF{

    /// <summary>
    /// Clase que implementa IEventHandler para la generación de la cabecera y el pie de página.
    /// </summary>
    public class PdfHeaderFooter : IEventHandler{

        protected PdfDocument pdf;/**< Objeto que contiene el documento en el que generar el footer */

        private IList<IElement> headerElements;/**< Elementos extraidos del html para poner en el header */

        private IList<IElement> footerElements;/**< Elementos extraidos del html para poner en el footer */

        protected DocProperties docProp;/**< Objeto con las propiedades para el documento */

        /// <summary>
        /// Constructor 
        /// </summary>
        /// 
        /// <param name="pdf">Objeto con el Documento pdf al que añadir el pie de página</param>
        /// <param name="cabeceraHtml">String con el Html para la cabecera de página</param>
        /// <param name="pieHtml">String con el Html para el pie de página</param>
        /// <param name="cprop">Objeto con los tipos de letra</param>
        /// <param name="docProp">Objeto con las propiedades para el documento</param>
        public PdfHeaderFooter(PdfDocument pdf, string cabeceraHtml, string pieHtml, ConverterProperties cprop, DocProperties docProp) {

            this.pdf = pdf;

            this.docProp = docProp;

            if (pieHtml != null && pieHtml.Trim() != ""){

                footerElements = HtmlConverter.ConvertToElements(pieHtml, cprop);
            }

            if (cabeceraHtml != null && cabeceraHtml.Trim() != "") {

                headerElements = HtmlConverter.ConvertToElements(cabeceraHtml, cprop);
            }

        }

        /// <summary>
        /// Método que se ejecuta cada vez que sucede el evento asignado(END_PAGE) añadiendo la cabecera, el pie o ambos a la página.
        /// </summary>
        /// <param name="ev">Datos sobre el evento</param>
        public void HandleEvent(Event ev) {

            PdfDocumentEvent docEvent = (PdfDocumentEvent) ev;
            PdfDocument pdf = docEvent.GetDocument();// Obtenemos el documento 

            PdfPage page = docEvent.GetPage();// Obtenemos la página 

            Rectangle pageSize = page.GetPageSize();// Obtenemos las dimensiones de la página
            PdfCanvas pdfCanvas = new PdfCanvas(page.GetLastContentStream(), page.GetResources(), pdf);

            float posY = pageSize.GetHeight() - (docProp.headerHeight + docProp.topMargin);
            float anchura = pageSize.GetWidth() - (docProp.leftMargin + docProp.rightMargin);// Obtenemos la anchura que tendrá la zona del pie y cabecera

            if (headerElements != null) { addHeader(pdfCanvas, posY, anchura); }

            if (footerElements != null) { addFooter(pdfCanvas, anchura); }

            pdfCanvas.Release();
        }

        /// <summary>
        /// Método que añade la cabecera a la página
        /// </summary>
        ///
        /// <param name="pdfCanvas">Objeto pdfCancvas donde añadir el contenido</param>
        /// <param name="posY">Posición vertical donde colocar el contenido</param>
        /// <param name="anchura">Anchura de la zona donde añadir el contenido</param>
        private void addHeader(PdfCanvas pdfCanvas, float posY, float anchura){

            Canvas canvas = new Canvas(pdfCanvas, pdf, new Rectangle(docProp.leftMargin, posY, anchura, docProp.headerHeight));// Generamos el canvas para la zona del pie

            foreach (IElement headerElement in headerElements){

                // Nos aseguramos que el tipo del elemento sea "IBlockElement"
                if (typeof(IBlockElement).IsInstanceOfType(headerElement)){

                    canvas.Add((IBlockElement)headerElement);// Colocamos los elementos de la cabecera en la zona asignada de la página
                }

            }

        }

        /// <summary>
        /// Método que añade el pie a la página
        /// </summary>
        ///
        /// <param name="pdfCanvas">Objeto pdfCancvas donde añadir el contenido</param>
        /// <param name="anchura">Anchura de la zona donde añadir el contenido</param>
        private void addFooter(PdfCanvas pdfCanvas, float anchura){

            Canvas canvas = new Canvas(pdfCanvas, pdf, new Rectangle(docProp.leftMargin, 0, anchura, docProp.footerHeight));// Generamos el canvas para la zona del pie

            foreach (IElement footerElement in footerElements){

                // Nos aseguramos que el tipo del elemento sea "IBlockElement"
                if (typeof(IBlockElement).IsInstanceOfType(footerElement)){

                    canvas.Add((IBlockElement)footerElement);// Colocamos los elementos del pie en la zona asignada de la página
                }

            }

        }


    }
}