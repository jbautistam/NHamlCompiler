NHamlCompiler
=============

NhamlCompiler es un compilador de código NHaml escrito en C#

NHaml es un lenguaje similar al HTML pero con ciertas características que lo hacen más fácil de escribir y mantener.

Por ejemplo, no es necesario escribir todas las etiquetas de HTML, podemos incluir variables y ejecutar sentencias de código.

Descripción de la solución
==========================

La solución incluye cuatro proyectos. El más importante es NHamlCompiler, una librería de clases que controla el proceso de compilación de código NHaml en HTML.

Entre el resto de las librerías, LibHelper es una librería de ayuda que entre otros métodos, proporciona clases de extensión para cadenas de texto que se utilizan desde NHamlCompiler.

Los otros dos proyectos se utilizan para pruebas: TestNhamlCompiler es una aplicación Windows Forms que nos permite escribir o cargar archivos de instrucciones NHaml para interpretarlos. BauControls es una librería de controles que se utilizan en la aplicación de pruebas.


