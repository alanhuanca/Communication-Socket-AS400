# Proyecto Cliente AS/400 (Arquitectura Limpia)

Este proyecto ha sido refactorizado para seguir los principios de la Arquitectura Limpia, con una clara separación de responsabilidades.

## Estructura del Proyecto

El proyecto está dividido en tres capas principales:

1.  **AS400.Core (Núcleo de la Aplicación)**
    * **Propósito:** Contiene las interfaces y la lógica de negocio central. Es la capa más interna y no tiene dependencias de otras capas.
    * **Contenido:**
        * `IAs400Service.cs`: Define el contrato para el servicio de AS/400.

2.  **AS400.Infrastructure (Infraestructura)**
    * **Propósito:** Contiene las implementaciones concretas de las interfaces de la capa `Core`. Aquí se encuentran los detalles de la tecnología (sockets, lógica de reintentos, etc.).
    * **Contenido:**
        * `ClienteAS400Seguro.cs`: La implementación real del servicio de AS/400.
        * `ClientSettings.cs`: Objeto POCO para manejar las opciones de configuración.

3.  **AS400.App (Aplicación)**
    * **Propósito:** Es el punto de entrada de la aplicación. Su única responsabilidad es la orquestación, la configuración de dependencias y el inicio del flujo de trabajo.
    * **Contenido:**
        * `ProgramaPrincipal.cs`: Configura los servicios, lee la configuración y ejecuta la lógica de la aplicación.

## Ventajas de este Diseño

* **Testabilidad:** La lógica de negocio en `AS400.Core` es completamente independiente y puede ser probada con facilidad.
* **Mantenimiento:** Los cambios en la implementación (por ejemplo, cambiar la biblioteca de sockets o el método de encriptación) solo afectarán a la capa de `Infrastructure`.
* **Escalabilidad:** Puedes añadir nuevas implementaciones de `IAs400Service` sin modificar el código que las utiliza.
