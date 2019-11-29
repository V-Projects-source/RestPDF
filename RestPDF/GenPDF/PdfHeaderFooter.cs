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
    /// Clase que implementa IEventHandler para la generaci�n de la cabecera y el pie de p�gina.
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
        /// <param name="pdf">Objeto con el Documento pdf al que a�adir el pie de p�gina</param>
        /// <param name="cabeceraHtml">String con el Html para la cabecera de p�gina</param>
        /// <param name="pieHtml">String con el Html para el pie de p�gina</param>
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
        /// M�todo que se ejecuta cada vez que sucede el evento asignado(END_PAGE) a�adiendo la cabecera, el pie o ambos a la p�gina.
        /// </summary>
        /// <param name="ev">Datos sobre el evento</param>
        public void HandleEvent(Event ev) {

            PdfDocumentEvent docEvent = (PdfDocumentEvent) ev;
            PdfDocument pdf = docEvent.GetDocument();// Obtenemos el documento 

            PdfPage page = docEvent.GetPage();// Obtenemos la p�gina 

            Rectangle pageSize = page.GetPageSize();// Obtenemos las dimensiones de la p�gina
            PdfCanvas pdfCanvas = new PdfCanvas(page.GetLastContentStream(), page.GetResources(), pdf);

            float posY = pageSize.GetHeight() - (docProp.headerHeight + docProp.topMargin);
            float anchura = pageSize.GetWidth() - (docProp.leftMargin + docProp.rightMargin);// Obtenemos la anchura que tendr� la zona del pie y cabecera

            if (headerElements != null) { addHeader(pdfCanvas, posY, anchura); }

            if (footerElements != null) { addFooter(pdfCanvas, anchura); }

            pdfCanvas.Release();
        }

        /// <summary>
        /// M�todo que a�ade la cabecera a la p�gina
        /// </summary>
        ///
        /// <param name="pdfCanvas">Objeto pdfCancvas donde a�adir el contenido</param>
        /// <param name="posY">Posici�n vertical donde colocar el contenido</param>
        /// <param name="anchura">Anchura de la zona donde a�adir el contenido</param>
        private void addHeader(PdfCanvas pdfCanvas, float posY, float anchura){

            Canvas canvas = new Canvas(pdfCanvas, pdf, new Rectangle(docProp.leftMargin, posY, anchura, docProp.headerHeight));// Generamos el canvas para la zona del pie

            foreach (IElement headerElement in headerElements){

                // Nos aseguramos que el tipo del elemento sea "IBlockElement"
                if (typeof(IBlockElement).IsInstanceOfType(headerElement)){

                    canvas.Add((IBlockElement)headerElement);// Colocamos los elementos de la cabecera en la zona asignada de la p�gina
                }

            }

        }

        /// <summary>
        /// M�todo que a�ade el pie a la p�gina
        /// </summary>
        ///
        /// <param name="pdfCanvas">Objeto pdfCancvas donde a�adir el contenido</param>
        /// <param name="anchura">Anchura de la zona donde a�adir el contenido</param>
        private void addFooter(PdfCanvas pdfCanvas, float anchura){

            Canvas canvas = new Canvas(pdfCanvas, pdf, new Rectangle(docProp.leftMargin, 0, anchura, docProp.footerHeight));// Generamos el canvas para la zona del pie

            foreach (IElement footerElement in footerElements){

                // Nos aseguramos que el tipo del elemento sea "IBlockElement"
                if (typeof(IBlockElement).IsInstanceOfType(footerElement)){

                    canvas.Add((IBlockElement)footerElement);// Colocamos los elementos del pie en la zona asignada de la p�gina
                }

            }

        }


    }
}