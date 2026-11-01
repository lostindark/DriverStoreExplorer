<table><tr>
<td><img src="./Rapr/icon.ico" alt="logo" width="96"></td>
<td>
<h1>Driver Store Explorer (RAPR)</h1>
🌏: <a href="/README.md">English</a> | <a href="/README_ES.md"><b>Español</b></a> | <a href="/README_TR.md">Türkçe</a> | <a href="/README_KO.md">한국어</a> | <a href="/README_ZH-CN.md">简体中文</a> | <a href="/README_ZH-TW.md">繁體中文</a>
</td>
</tr></table>

---

[![Build Status](https://ci.appveyor.com/api/projects/status/kqtvhfq23am2gq26/branch/master?svg=true)](https://ci.appveyor.com/project/lostindark/driverstoreexplorer/branch/master)
[![Build Status](https://github.com/lostindark/DriverStoreExplorer/actions/workflows/ci.yml/badge.svg)](https://github.com/lostindark/DriverStoreExplorer/actions/workflows/ci.yml)

## ⚠️ Advertencia
**Driver Store Explorer modifica el almacén de controladores de Windows. Un uso incorrecto puede hacer que el sistema falle, impedir que Windows arranque o dejar dispositivos sin funcionar. Conozca los riesgos antes de continuar. Haga siempre una copia de seguridad de los controladores antes de borrar nada.**

---

## ¿Qué es Driver Store Explorer?

Driver Store Explorer (RAPR) es una herramienta para ver, administrar y limpiar el [almacén de controladores](https://learn.microsoft.com/es-es/windows-hardware/drivers/install/driver-store) (DriverStore) de Windows. Está pensada para usuarios avanzados y administradores. Si no conoce bien el funcionamiento interno de Windows, no se recomienda usarla.

---


## Características

### Operaciones principales
* **Ver y listar**: muestra todos los paquetes de controladores de terceros con sus datos (tamaño, versión, fecha, etc.)
* **Agregar e instalar**: agrega paquetes de controladores nuevos y, si se desea, los instala automáticamente en los dispositivos
* **Quitar y borrar**: borra uno o varios controladores, con borrado forzado para los que están en uso  
	_Nota: lo que implica quitar o borrar un controlador depende del estado y del uso del sistema. Tenga cuidado, porque puede afectar al funcionamiento de un dispositivo u obligar a reinstalar los controladores de algún hardware._
* **Exportar**: guarda una copia de seguridad de los controladores seleccionados, o de todos, en carpetas ordenadas
* **Limpieza inteligente**: identifica y selecciona automáticamente las versiones antiguas o sin usar

### Funciones avanzadas
* **Varias API**: API nativa de Windows, DISM o PnPUtil, con detección automática
* **En línea y sin conexión**: funciona con el equipo local o con el almacén de controladores de una imagen de Windows sin conexión
* **Dispositivos asociados**: muestra los dispositivos conectados y si están presentes
* **Operaciones por lotes**: selección múltiple para trabajar con muchos controladores a la vez, con indicador de progreso
* **Búsqueda en tiempo real**: filtra la lista mientras escribe y exporta a CSV para analizar los datos

### Interfaz
* **Varios idiomas**: más de 20 idiomas, con soporte para escritura de derecha a izquierda
* **Personalizable**: columnas ordenables, agrupación y diseño flexible
* **Información visual**: colores, resaltado de la selección y registro detallado

---

## Estados de los controladores y opciones para quitarlos

- **Controladores antiguos:** un controlador se considera "antiguo" cuando hay versiones más nuevas en el sistema. Quitarlos puede liberar espacio y ordenar la lista, pero puede afectar a la compatibilidad con algunos dispositivos o configuraciones. Conviene hacer una copia de seguridad antes de quitarlos. "Seleccionar controlador(es) antiguo(s)" los identifica automáticamente, aunque el resultado puede variar.
- **Controladores sin usar:** "Seleccionar controlador(es) sin usar" selecciona controladores que no tienen un dispositivo asociado, no controladores que se sepa que se pueden borrar sin riesgo. También puede seleccionar controladores esenciales de antivirus, servicios de red, impresoras y otros programas. Tanto el botón como el menú contextual muestran un aviso antes de cambiar la selección, y la opción predeterminada es Cancelar. Este aviso está activado de forma predeterminada y se puede desactivar en **Opciones > Advertir antes de seleccionar controladores sin usar**. Mientras haya alguno de estos controladores marcado, la barra de estado muestra un recordatorio en amarillo; los errores se muestran en rojo sin ocultarlo. Si exporta correctamente los paquetes de controladores, el recordatorio desaparece para los que ha respaldado; exportar a CSV o una copia fallida no lo quitan. Revise los controladores seleccionados y haga una copia de seguridad antes de borrarlos.
- **Controladores críticos de arranque:** Windows puede necesitarlos para iniciarse. "Seleccionar controlador(es) antiguo(s)" y "Seleccionar controlador(es) sin usar" los omiten, salvo que active **Opciones > Incluir críticos de arranque en controladores antiguos y sin usar**.
- **Seleccionar mientras busca:** cuando el cuadro de búsqueda filtra la lista, "Seleccionar todo", "Invertir selección" y "Seleccionar" solo cambian los controladores que se ven. "Seleccionar controlador(es) antiguo(s)" y "Seleccionar controlador(es) sin usar" revisan todo el almacén, así que primero borran la búsqueda y todo lo que seleccionan queda a la vista.
- **Nombres de dispositivo en gris:** los controladores con el nombre del dispositivo en gris están asociados a dispositivos que ahora no están conectados (por ejemplo, cámaras, teléfonos o discos externos). Si los quita, tendrá que volver a instalarlos cuando conecte de nuevo el dispositivo.
- **Limpieza sin conexión:** en los almacenes sin conexión no se pueden seleccionar automáticamente los controladores antiguos o sin usar, porque no es posible saber con certeza qué dispositivos los usan. Puede seleccionarlos a mano para quitarlos.
- **Forzar borrado:** use esta opción si necesita borrar un controlador que está en uso. Nota: puede no funcionar con controladores de impresora.
---

## Captura de pantalla
![Captura de pantalla de Driver Store Explorer](https://github.com/user-attachments/assets/2d7df896-494d-4bcd-b064-5f05696cd0d3)

---

## Instalación

### Requisitos
- Windows 7 o posterior
- .NET Framework 4.7.2 o posterior
- Permisos de administrador

### Opción 1: descargar la versión compilada (recomendado)
1. Vaya a la [página de la última versión](https://github.com/lostindark/DriverStoreExplorer/releases/latest)
2. Descargue el archivo ZIP más reciente
3. Extraiga los archivos en la carpeta que prefiera
4. Ejecute `Rapr.exe`

### Opción 2: instalar con Winget (recomendado)
```powershell
winget install lostindark.DriverStoreExplorer
```
Después de instalarla, abra la herramienta con:
```powershell
rapr
```

### Opción 3: instalar con Scoop
```powershell
scoop bucket add extras
scoop install driverstoreexplorer
```
Después de instalarla, abra la herramienta con:
```powershell
rapr
```

### Opción 4: compilar desde el código fuente
1. Clone o descargue este repositorio
2. Abra `Rapr.sln` en Visual Studio 2022
3. Compile la solución (Compilar → Compilar solución, o Ctrl+Mayús+B)
4. Ejecute el programa desde la carpeta de salida

---

## Historia del proyecto
Originalmente alojado en [https://driverstoreexplorer.codeplex.com/](https://web.archive.org/web/20190417132137/https://archive.codeplex.com/?p=driverstoreexplorer).

## Créditos
- [ObjectListView](https://github.com/lostindark/ObjectListView)
- [Managed DismApi Wrapper](https://github.com/jeffkl/ManagedDism)
- [FlexibleMessageBox](https://www.codeproject.com/Articles/601900/FlexibleMessageBox-A-Flexible-Replacement-for-the)
- [Resource Embedder](https://github.com/0xced/resource-embedder)
- [PortableSettingsProvider](https://github.com/bluegrams/SettingsProviders)
- [Strong Namer](https://github.com/dsplaisted/strongnamer)

## Patrocinadores
Firma de código gratuita para Windows proporcionada por [SignPath.io], con certificado de [SignPath Foundation].

[SignPath.io]: https://signpath.io
[SignPath Foundation]: https://signpath.org
