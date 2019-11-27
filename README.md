# RestPDF

**Rest service to generate PDF document from Html**

This services convert HTML and CSS into PDF documents. The HTML and CSS are received by **JSON** format.

Use the path **"http:// {domain_name}/vrs/pdf"** using the **Post** method to make the request.

You can also save the generated file in a Sharepoint list, if you send **"saveFile"** with the value **"true"** in the request. See below for more information in the **"Request information"** section.

Request body example:
```
{
    "fileName":""
    "siteUrl": "http://www.domain_name.com",
    "contentHtml": "{Html code}",
    "footerHtml": "{Html code}",
    "urlCSS": "http://www.domain_name.co/file.css",
    "download": "inline",
    "topMargin":"30",
    "bottomMargin":"120",
    "leftMargin":"37",
    "rightMargin":"37",
    "footerHeight":"110",
    "pageType":"A4",
    "saveFile":"true",
    "librarySiteUrl":"http://domain_name.com/list/",
    "fileUrlPath":"/list/uploadPDF/",
    "libraryName":"upload PDF",
    "fieldInternalNames":"",
    "fieldValues":"",
    "numberPage":"true",
    "numberPageSize":"8",
    "numberPageOf":"of",
    "numberPageVpos":"30"
}
```
## Request Information:
Headers

| Name  |  Description |
|-------|--------------|
|  Content-Type |  Indicates the type of data to send in the request. Must be **application/json** |


Body		

| Name  |  Description |
|-------|--------------|
|fileName (optional)|String with the file's name without extension.|
|siteUrl|Url root where the images are located. It is added to the "src" of the images to get the absolute path.|
|htmlFromUrl (optional)|Url where is located the html for the content.If you use this field, the contentHtml field does not use in the request.|
|contentHtml|HTML with the content to generate the file.|
|footerHtml (optional)|HTML with the content to generate the footer.|
|urlCSS (optional)|Url where is located the styles file.|
|download|Indicates how the file is received.|
|docProperties (optional)|<p>(optional) Indicates the properties for the document to be generated: "topMargin", "leftMargin", "rightMargin", "bottomMargin", "footerHeight", "pageType", "numberPage","numberPageSize","numberPageOf", "numberPageVpos"<p>values for **"pageType"**: A0, A1, A2, A3, A4, A5, A6, A7, A9, B0, B1, B2, B3, B5, B6, B7, B8, EXECUTIVE, LEDGER, LEGAL, LETTER, TABLOID</p><p>With **"numberPage"** = true the number of each page is added.</p><p>**"numberPageSize"** is the font size.</p><p>**"numberPageOf"** indicates the character (s) that are placed between the page number and the total pages. ex: 1 of 6. If not indicated, only the page number is added</p><p>**"numberPageVpos"** Indicates the vertical position where the page number is placed.</p>|
|pathFonts (optional)|Paths of the fonts that you want to add to the document.<p>Use &#124; to separate the paths.</p>|
|saveFile (optional)|Indicates if the generated pdf is saved in a library.The file name will be saved as:{fileName}_yyyy-MM-dd_hh-mm-ss.fff.pdf|
|librarySiteUrl (optional)|**This values is required if saveFile is true.** Indicates the url of the site where the library is located.|
|fileUrlPath (optional)|**This values is required if saveFile is true.** Url relative to the place where to save the file, including in the path the name of the library and folder if it exists.|
|libraryName (optional)|**This values is required if saveFile is true.** Library name.|
|fieldInternalNames (optional)|Internal names of the fields to update. Use &#124; to separate the names.|
|fieldValues (optional)|Values to update in the fields. Use &#124; to separate the values and ';' if a field has several values. For "Person or Group" and "Lookup" fields the values must be integer with the ID.|


## Response Information:

>Status code 200 (Ok)

>Status code 400 (Bad Request)

>Status code 500 (Internal server error)

## Response Formats:

> With Status code 200 **(application/pdf)**
```
    File in pdf format
```

> With Status code 400 or 500 **(application/json)**
```
    {
        "requesId":"13d11674-e037-45c6-8fa2-a2c50ba694cd",
        "statusCode": 400,
        "msgError": "< siteUrl is empty > "
    }
```


## Requirements
* [iText 7 Community (>=7.1.8)](https://www.nuget.org/packages/itext7/)
* [itext7.pdfHTML (>=2.1.5 )](https://www.nuget.org/packages/itext7.pdfhtml/)
* Microsoft.Sharepoint.Client
* Microsoft.Sharepoint.Client.Runtime

## License
This project is licensed under the GNU AFFERO GENERAL PUBLIC LICENSE Version 3 - see the [LICENSE](LICENSE) file for details