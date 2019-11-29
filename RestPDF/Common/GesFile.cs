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
using System.Collections.Generic;
using Microsoft.SharePoint.Client;
using System.IO;

namespace RestPDF.Common {

    /// <summary>
    /// Clase que encapsula las operaciones de gestión de archivos en una Biblioteca de Sharepoint.
    /// </summary>
    public class GesFile {

        private ClientContext clientContext;/**< Contexto cliente para acceder a las listas de sharepoint */

        private string siteUrl;/**< Url del servidor donde se encuentran las listas de SharePoint */

        private string libraryName;/**< Nombre de la lista de la que obtener o modificar datos **/

        /// <summary>
        /// Constructor
        /// </summary>
        /// 
        /// <param name="siteUrl">Url del sitio con el que crear el contexto de cliente</param>
        /// <param name="libraryName">Nombre de la lista</param>
        public GesFile(string siteUrl, string libraryName) {

            this.siteUrl = siteUrl;
            this.libraryName = libraryName;

            clientContext = new ClientContext(siteUrl);

        }

        /// <summary>
        /// Método que guarda un stream con los bytes de un archivo y actualiza los campos "internalFieldNames" con los valores "fieldValues" en una biblioteca de Sharepoint
        /// </summary>
        /// 
        /// <param name="destinationRelativeUrl">Ruta donde guardar el archivo</param>
        /// <param name="fileName">Nombre del archivo a guardar</param>
        /// <param name="fileStream">Stream con los datos a guardar</param>
        /// <param name="internalFieldNames">Nombres internos de los campos a actualizar</param>
        /// <param name="fieldValues">Valores a actualizar en los campos</param>
        public void saveFile(string destinationRelativeUrl, string fileName, MemoryStream fileStream, string internalFieldNames, string fieldValues) {

            string nLocation = destinationRelativeUrl + fileName;

            Microsoft.SharePoint.Client.File.SaveBinaryDirect(clientContext, nLocation, fileStream, false);

            List lista = clientContext.Web.Lists.GetByTitle(libraryName);

            // Obtenemos el archivo Destino
            Microsoft.SharePoint.Client.File file = clientContext.Web.GetFileByServerRelativeUrl(nLocation);

            // Una vez subido el archivo se obtiene su ID
            int fileID = getFileID(fileName, lista);

            ListItem listItem = lista.GetItemById(fileID);

            clientContext.Load(listItem);
            clientContext.Load(file);
            clientContext.ExecuteQuery();

            if (internalFieldNames != null && internalFieldNames.Trim() != "" && fieldValues != null && fieldValues.Trim() != ""){

                Dictionary<string, string> valores = getKeyAndValues(internalFieldNames, fieldValues);

                var fieldsData = getFieldsData(lista);// Obtenemos los campos de la lista donde se ha subido el archivo

                // Recorremos cada campo para ver si se actualiza su valor en caso de existir en "valores"
                foreach (var field in fieldsData) {

                    if (valores.ContainsKey(field.InternalName)) {// Si el internalName del campo está entre las keys de los valores a modificar

                        // Si el campo en el cual se va a actualizar el valor es un LookUp
                        if (field.FieldTypeKind == FieldType.Lookup) {

                            var ids = valores[field.InternalName].ToString().Split(';');

                            List<FieldLookupValue> oLookupValues = new List<FieldLookupValue>();

                            foreach (var id in ids) {// Para cada id del Lookup

                                var v = new FieldLookupValue();

                                v.LookupId = Int32.Parse(id.Trim());

                                oLookupValues.Add(v);// Añadimos el id

                            }

                            listItem[field.InternalName] = oLookupValues;

                        } else if (field.FieldTypeKind == FieldType.User) {// Si el campo en el cual se va a actualizar el valor es un Person o Group

                            var ids = valores[field.InternalName].ToString().Split(';');

                            List<FieldUserValue> oFieldUserValues = new List<FieldUserValue>();

                            foreach (var id in ids) {// Para cada id de usuario

                                FieldUserValue uv = new FieldUserValue();
                                uv.LookupId = Int32.Parse(id.Trim());

                                oFieldUserValues.Add(uv);// Añadimos el id
                            }

                            listItem[field.InternalName] = oFieldUserValues;

                        } else if (field.FieldTypeKind == FieldType.MultiChoice) {// Si el campo en el cual se va a actualizar el valor es MultiChoice

                            List<string> vals = new List<string>();

                            var aux = valores[field.InternalName].Split(';');

                            foreach (var value in aux) { vals.Add(value); }

                            listItem[field.InternalName] = vals;


                            // Para otro tipo de campo
                        } else {

                            listItem[field.InternalName] = valores[field.InternalName];
                        }

                    }

                }

                listItem.Update();

                //**************************************
            }

            // Si el archivo destino está CheckOut, lo ponemos en CheckIn
            if (file.CheckOutType != CheckOutType.None) { file.CheckIn(String.Empty, CheckinType.MajorCheckIn); }

            clientContext.ExecuteQuery();
            
        }

        /// <summary>
        /// Método que devuelve el ID que tiene un archivo en una biblioteca
        /// </summary>
        /// 
        /// <param name="fileName">string con el nombre del archivo del cual obtener el ID</param>
        /// <param name="lista">Objeto Microsoft.SharePoint.Client.List con la lista de la que obtener el ID</param>
        /// 
        /// <returns>Devuelve un entero con el ID del archivo</returns>
        private int getFileID(string fileName, List lista) {

            int ID = 0;

            CamlQuery consulta = new CamlQuery();

            // En la consulta filtramos obteniendo las filas cuya fecha de modificación sea
            // igual o mayor que la última fecha de ejecución de la tarea de replicación
            consulta.ViewXml =
                String.Format(@"<View Scope='RecursiveAll'>
                                    <Query>
                                        <Where>
                                            <Eq>
                                                <FieldRef Name='FileLeafRef'/>
                                                <Value Type='Text'>{0}</Value>
                                            </Eq>
                                        </Where>    
                                    </Query>
                                </View>", fileName);


            var ListItems = lista.GetItems(consulta);
            clientContext.Load(ListItems);

            clientContext.ExecuteQuery();

            if (ListItems.Count > 0) {

                var it = ListItems[0];
                ID = (int)it["ID"];
            }

            return ID;

        }

        /// <summary>
        /// Método que obtiene los datos de los campos de la biblioteca donde se guarda el archivo
        /// </summary>
        /// 
        /// <param name="lista">Objeto Microsoft.SharePoint.Client.List con la lista de la que obtener el ID</param>
        /// 
        /// <returns>Devuelve un Objeto ListData con los nombres de los campos(oListaData.fieldsInternalName)</returns>
        private List<Field> getFieldsData(List lista) {

            List<Field> listData = new List<Field>();/**< Lista con los nombres internos de los campos(columnas) de la lista y/o datos para el destino */

            //****************************************************************
            // Zona para obtener los nombre internos de los campos de la lista
            FieldCollection oListFieldCollection = lista.Fields;

            clientContext.Load(oListFieldCollection);
            clientContext.ExecuteQuery();

            // Obtenemos los nombres internos de los campos que no sean de solo lectura, ni "Attachments", ni ContentType y si sean Calculated
            foreach (Field field in oListFieldCollection) {

                // Si los campos no son de solo lectura, ni "Attachments", ni ContentType, y si sean Calculated
                if (FieldsType(field)) {

                    //listData.fieldsInternalName.Add(field.InternalName);// Añadimos el nombre interno del campo(columna)
                    listData.Add(field);// Añadimos el campo a la lista

                }
            }

            return listData;

        }

        /// <summary>
        /// Método que comprueba los tipos de campos que no son permitidos y si lo son
        /// Campos que no sean de solo lectura, ni "Attachments", ni "ContentType", ni "PreviewOnForm", ni "ThumbnailOnForm",
        /// ni "FileType", ni "ImageSize", y si sean "Calculated" o "SouceID".
        /// </summary>
        /// 
        /// <param name="field">Objeto Field con el campo a comprobar</param>
        /// 
        /// <returns>Devuelve true en caso de cumplirse las condiciones</returns>
        private Boolean FieldsType(Field field) {
            
            return (!field.ReadOnlyField &&
                       field.InternalName != "Attachments" &&
                       field.InternalName != "ContentType" &&
                       field.InternalName != "PreviewOnForm" &&
                       field.InternalName != "ThumbnailOnForm" &&
                       field.InternalName != "FileType" &&
                       field.InternalName != "ImageSize" &&
                       !field.Hidden) ||
                       field.TypeAsString.ToLower() == "calculated" ||
                       field.InternalName.ToLower() == "sourceid";
        }

        /// <summary>
        /// Método que convierte dos cadenas de caracteres en un Array asociativo
        /// </summary>
        /// 
        /// <param name="keys">Cadena con los valores para las key del Array</param>
        /// <param name="values">Cadena con los valores para los Values del Array</param>
        /// 
        /// <returns>Devuelve un Array asociativo cuyas key son los valores de "internalFieldNames" y sus valores los de "fieldValues"</returns>
        private Dictionary<string, string> getKeyAndValues(string keys, string values) {

            Dictionary<string, string> valores = new Dictionary<string, string>();

            int cNames = 0;
            int cValues = 0;

            var iNames = keys.Split('|');// Separamos los nombres internos
            var fvalues = values.Split('|');// Separamos los valores

            // Contabilizamos los elementos que no estén vacíos
            foreach (var e in iNames) {
                
                if(e.Trim() != ""){ cNames++; }
            }

            // Contabilizamos los elementos que no estén vacíos
            foreach (var e in fvalues) {

                if (e.Trim() != "") { cValues++; }
            }

            // Si los nombre internos y valores no tienen el mismo número de elementos
            if (cNames != cValues) {

                throw new Exception("internalFieldNames and fieldValues must have the same number of elements");

            } else {

                for (var i = 0; i < iNames.Length; i++) {

                    valores.Add(iNames[i].Trim(), fvalues[i].Trim());

                }

            }

            return valores;

        }

    }
}