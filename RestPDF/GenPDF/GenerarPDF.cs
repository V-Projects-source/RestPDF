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

using System.IO;
using iText.Kernel.Pdf;
using iText.Html2pdf;
using iText.IO.Font;
using iText.Layout.Font;
using System;
using System.Collections.Generic;
using iText.Kernel.Geom;
using iText.Kernel.Events;
using iText.Layout;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Net.Mail;

namespace GenPDF{

    /// <summary>
    /// Clase que encapsula la generación de un documento Pdf a apartir de Html y Css.
    /// El Constructor recibe las rutas a los tipos de letra que se añadiran al FontProvider,
    /// y la url raíz que se usa para añadir a los "src" y "href" para transformarlos en absolutos.
    /// 
    /// </summary>
    public class GenerarPDF{

        private string siteUrl;/**< Url del sito donde se encuentran los recursos(imágenes, etc) */

        private List<string> pathFonts = new List<string>();/**< Rutas donde obtener los tipos de letra para el documento */

         /// <summary>
         /// Constructor
         /// </summary>
         ///
         /// <param name="pathFonts">Array con las rutas y nombres de los tipos de letra para el documento pdf</param>
        /// <param name="siteUrl">String con la Url del sito donde se encuentran los recursos(imágenes, etc)</param>
        public GenerarPDF(List<string> pathFonts, string siteUrl){

            this.pathFonts = pathFonts;
            this.siteUrl = siteUrl;
        }

        /// <summary>
        /// Método que genera un documento pdf a partir de Html y css
        /// </summary>
        /// <param name="contenidoHtml">String con el html para el contenido del documento</param>
        /// <param name="cabeceraHtml">String con el html para la cabecera de la página</param>
        /// <param name="pieHtml">String con el html para el pie de página</param>
        /// <param name="docProp">Objeto con las propiedades para el documento: márgenes, altura de pie y formato de la página(A1, A2, A4, LETTER, etc)</param>
        /// <param name="css">String con el css</param>
        ///
        /// <returns>Devuelve un array con los bytes del archivo pdf generado</returns>
        public byte[] makeHtmlPDF(string contenidoHtml, string cabeceraHtml, string pieHtml, DocProperties docProp, string css) {

            byte[] bytes = null;

            if(contenidoHtml != null && contenidoHtml.Trim() != ""){

                float topMargin = docProp.topMargin;
                float bottomMargin = docProp.bottomMargin;
                bool cabecera = false;
                bool pie = false;

                if (cabeceraHtml != null && cabeceraHtml.Trim() != "" && docProp.headerHeight != 0){//Si se ha pasado la cabecera de página

                    cabeceraHtml = "<style>\n" + css + "</style>\n" + cabeceraHtml;
                    topMargin = docProp.headerHeight;
                    cabecera = true;

                } else {

                    cabeceraHtml = "";
                }

                if (pieHtml != null && pieHtml.Trim() != "" && docProp.footerHeight != 0){//Si se ha pasado el pie de página

                    pieHtml = "<style>\n" + css + "</style>\n" + pieHtml;
                    bottomMargin = docProp.footerHeight;
                    pie = true;

                } else {

                    pieHtml = "";
                }

                contenidoHtml ="<style>\n@page{margin-top: " + topMargin + "pt;" +// Top Margin de la página
                                "margin-bottom: " + bottomMargin+ "pt;" + // Bottom Maring de la página
                                "margin-left: " + docProp.leftMargin + "pt;" + // Left Margin de la página
                                "margin-right: " + docProp.rightMargin + "pt;}" + // Right Margin de la página
                                css + "</style>\n" + // Añadimos los estilos css al html del contenido
                                contenidoHtml;

                using (var dest = new MemoryStream()){// Creamos un buffer en memoria para guardar el documento generado

                    PdfWriter writer = new PdfWriter(dest);
                    PdfDocument pdf = new PdfDocument(writer);
                    pdf.SetDefaultPageSize(getPageType(docProp.pageType));

                    FontProvider fProvider = new FontProvider();
                    FontProgram fProgram = null;

                    ConverterProperties prop = new ConverterProperties();// Creamos el objeto con las propiedades que tendrá el pdf
                    prop.SetBaseUri(this.siteUrl);//Añadimos la base para convertir las "src" y "href" en rutas absolutas

                    // Para cada ruta al tipo de letra
                    foreach (var path in pathFonts) {

                        try {

                            fProgram = FontProgramFactory.CreateFont(path);// Se crea la fuente
                            fProvider.AddFont(fProgram);// Se añade al proveedor

                        } catch (Exception e) {

                            Console.WriteLine(e.Message);
                        }
                    }

                    FontSet fSet = fProvider.GetFontSet();// Obtenemos el set de fuentes del proveedor

                    if(fSet.Size() > 0){// Si se ha añadido alguna fuente al proveedor

                        prop.SetFontProvider(fProvider);// Añadimos los tipos de letra a las propiedades
                    }

                    // Si el contenido para la cabecera o pié no es nulo ni está vacío
                    if ( cabecera || pie ){

                        PdfHeaderFooter footerHandler = new PdfHeaderFooter(pdf, cabeceraHtml, pieHtml, prop, docProp);// Se crea el manejador para añadr el pie de página
                        pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, footerHandler);// Añadimos el manejador para el evento fin de página

                    }

                    HtmlConverter.ConvertToPdf(contenidoHtml, pdf, prop);// Generamos el pdf

                    bytes = dest.ToArray();// Volcamos los bytes del documento a un array
                }

                if (docProp.numberPage) {// Si se quiere añadir el número de página

                    bytes = addPageNumber(bytes, docProp);
                }
            }

            return bytes;
        }

        /// <summary>
        /// Método que devuelve el tipo de página: A2, A3, A4, etc, en forma de objeto PageSize
        /// </summary>
        /// <param name="type">String con el tipo de página</param>
        /// 
        /// <returns>Devuelve el PageSize correspondiente</returns>
        private PageSize getPageType(string type) {

            switch (type.ToUpper()) {
                case "A0":
                    return PageSize.A0;
                case "A1":
                    return PageSize.A1;
                case "A2":
                    return PageSize.A2;
                case "A3":
                    return PageSize.A3;
                case "A5":
                    return PageSize.A5;
                case "A6":
                    return PageSize.A6;
                case "A7":
                    return PageSize.A7;
                case "A9":
                    return PageSize.A9;
                case "B0":
                    return PageSize.B0;
                case "B1":
                    return PageSize.B1;
                case "B2":
                    return PageSize.B2;
                case "B3":
                    return PageSize.B3;
                case "B4":
                    return PageSize.B4;
                case "B5":
                    return PageSize.B5;
                case "B6":
                    return PageSize.B6;
                case "B7":
                    return PageSize.B7;
                case "B8":
                    return PageSize.B8;
                case "EXECUTIVE":
                    return PageSize.EXECUTIVE;
                case "LEDGER":
                    return PageSize.LEDGER;
                case "LEGAL":
                    return PageSize.LEGAL;
                case "LETTER":
                    return PageSize.LETTER;
                case "TABLOID":
                    return PageSize.TABLOID;
                default:
                    return PageSize.A4;
            }
        
        }


        /// <summary>
        /// Método que coloca en cada página del documento el número de página, y en caso de indicarse, el total de páginas.
        /// </summary>
        ///
        /// <param name="bytes">Array de bytes con los datos del documento donde añadir los números de página</param>
        /// <param name="docProp">Objeto con las propiedades del documento donde se indican las opciones para colocar el número de página</param>
        ///
        /// <returns>Deveuelve un array de bytes con el documento modificado</returns>
        private byte[] addPageNumber(byte[] bytes, DocProperties docProp) {

            MemoryStream aux = new MemoryStream(bytes);
            MemoryStream dest = new MemoryStream();

            PdfReader reader = new PdfReader(aux);
            PdfWriter writer = new PdfWriter(dest);
            PdfDocument pdfDoc = new PdfDocument(reader, writer);

            Document document = new Document(pdfDoc);

            var total = pdfDoc.GetNumberOfPages();

            for (int i = 1; i <= total; i++) {// Para cada página del documento, añadimos su número y total

                PdfPage page = pdfDoc.GetPage(i);
                Rectangle pageSize = page.GetPageSize();
                PdfCanvas pdfCanvas = new PdfCanvas(page);
                
                string npage = npage = i.ToString();

                int vPos = docProp.numberPageVPos + docProp.numberPageSize;

                float anchura = pageSize.GetWidth() - (docProp.leftMargin + docProp.rightMargin);// Obtenemos la anchura que tendrá la zona del pie

                // Obsoleto a partir de la versión 7.1.11
                //Canvas canvas = new Canvas(pdfCanvas, pdfDoc, new Rectangle(docProp.leftMargin, vPos, anchura, docProp.numberPageSize));
                Canvas canvas = new Canvas(pdfCanvas, new Rectangle(docProp.leftMargin, vPos, anchura, docProp.numberPageSize));// Generamos el canvas para la zona del número de página

                if (docProp.numberPageOf != "") { npage = i.ToString() + " " + docProp.numberPageOf + " " + total.ToString(); }

                Paragraph p = new Paragraph().Add(npage)
                                             .SetTextAlignment(TextAlignment.CENTER)
                                             .SetFontSize(docProp.numberPageSize);
                canvas.Add(p);
            }
            
            pdfDoc.Close();

            return dest.ToArray();
        }
    }
}