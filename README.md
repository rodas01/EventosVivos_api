# EventosVivos - Sistema de Gestión de Reservas

EventosVivos es una plataforma para la creación de eventos culturales, conferencias y talleres, así como la reserva y gestión de entradas. El sistema está diseñado para solucionar problemas críticos como el sobrecupo (overbooking), la superposición de horarios en recintos, y la automatización del procesamiento de pagos y cancelaciones.

---

## 🏗️ Arquitectura General

El proyecto utiliza una arquitectura desacoplada tipo **Single Page Application (SPA)** con los siguientes pilares:

```
[ Frontend: Angular (v17+) ] <--- HTTP REST ---> [ Backend: ASP.NET Core Web API (.NET 8) ]
                                                            |
                                                 [ In-Memory Storage / Mock Repositories ]
```

### 1. Backend (`Back-End`)
El backend está estructurado bajo principios de **Separación de Concernimientos** y **Clean Code**:
- **Controllers**: Exponen los endpoints RESTful para la comunicación con el cliente Angular.
- **Services (Capa de Negocio)**: Implementan toda la lógica y reglas de negocio descritas en la especificación (validación de capacidades, solapamiento, límites por precio/tiempo, penalizaciones).
- **Models / DTOs**: Estructuras limpias para la transferencia y almacenamiento de datos, protegiendo las entidades internas.
- **Repository Pattern**: Abstracción del almacenamiento utilizando bases de datos en memoria para facilitar las pruebas rápidas sin dependencias externas.

### 2. Frontend (`EventosVivos_Frontend`)
Construido utilizando **Angular** moderno:
- **Standalone Components**: Modularidad e independencia sin necesidad de pesados módulos declarativos.
- **Signal-Based State Management**: Reactividad de grano fino para una interfaz veloz y sincronizada.
- **Reactive Forms**: Formulario robusto con validadores dinámicos y cruzados (ej: capacidad máxima basada en el venue, hora nocturna en fines de semana).
- **Guards e Interceptors**: Control de rutas seguras para paneles de administración y adjuntado automático de tokens/cabeceras de sesión.
- **Premium Design System**: Estilizado con CSS vanilla y Tailwind, con transiciones fluidas, badges de estado e indicaciones visuales elegantes.

---

## 🛠️ Tecnologías Utilizadas

- **Backend**:
  - .NET 8 / C# 12
  - ASP.NET Core Web API
  - MSTest (Pruebas unitarias)
- **Frontend**:
  - Angular (v21.2)
  - TypeScript
  - Vitest (Pruebas unitarias ultrarrápidas)
  - Tailwind CSS / Vanilla CSS

---

## 📋 Reglas de Negocio Implementadas

El sistema valida de manera estricta todas las reglas de negocio descritas en la prueba:
- **RN-01 (Capacidad del Venue)**: La capacidad de un evento no puede exceder la del recinto asignado.
- **RN-02 (Superposición de Horarios)**: No se permite registrar eventos activos que compartan el mismo venue y solapen horarios.
- **RN-03 (Horario Nocturno)**: Eventos programados para sábados o domingos no pueden iniciar después de las 22:00 horas.
- **RN-04 (Reserva Tardía)**: No es posible reservar entradas si el evento inicia en menos de 1 hora.
- **RN-05 (Límite por Precio)**: Eventos con precio mayor a $100 limitan las compras a un máximo de 10 entradas por transacción.
- **RF-03 (Prioridad de Tiempo Urgente)**: Si el evento inicia en menos de 24 horas, el límite de compra se reduce a un máximo de 5 entradas (esta regla tiene prioridad sobre la RN-05).
- **RN-06 (Estado Automático)**: El estado del evento se actualiza a "completado" automáticamente cuando pasa su hora de finalización.
- **RN-07 (Penalización)**: Cancelaciones con menos de 48 horas de anticipación registran la reserva como "perdida". Las entradas **no se liberan** para la venta (penalización) y se marcan así únicamente en los reportes de ocupación.

---

## 🚀 Instrucciones de Ejecución Local

Para levantar el proyecto completo de manera local, siga los siguientes pasos:

### 1. Levantar el Backend (API)
1. Navegue al directorio del backend:
   ```bash
   cd Back-End/EventosVivos_Api
   ```
2. Restaure y compile las dependencias:
   ```bash
   dotnet restore
   ```
3. Ejecute la aplicación:
   ```bash
   dotnet run
   ```
   *La API estará disponible por defecto en `http://localhost:5038` (o el puerto configurado en `launchSettings.json`).*

### 2. Levantar el Frontend (UI)
1. Navegue al directorio del frontend:
   ```bash
   cd EventosVivos_Frontend
   ```
2. Instale los paquetes NPM necesarios:
   ```bash
   npm install
   ```
3. Inicie el servidor de desarrollo:
   ```bash
   npm run start
   ```
4. Abra su navegador e ingrese a `http://localhost:4200`.

---

## 🧪 Pruebas Automatizadas y Reporte de Cobertura

El proyecto incluye suites completas de pruebas unitarias para garantizar el correcto funcionamiento del software:

### Backend
- Pruebas que validan todas las reglas de negocio, límites de compra, penalizaciones y cruces de horarios.
- **Comando para ejecutar**:
  ```bash
  cd Back-End
  dotnet test --settings test.runsettings
  ```
- **Generar Reporte de Cobertura (HTML)**:
  Restaure las herramientas locales, recopile la cobertura de las pruebas y convierta el XML con `reportgenerator`:
  ```bash
  cd Back-End
  dotnet tool restore
  dotnet test --collect:"XPlat Code Coverage" --settings test.runsettings
  dotnet reportgenerator -reports:"EventosVivos_Api.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
  ```
  El reporte estará disponible en `Back-End/coveragereport/index.html`.

### Frontend
- Pruebas que verifican el ciclo de vida de los componentes, lógica de validación de formularios en tiempo real, modales de confirmación, ordenamiento y redirecciones de rutas.
- **Comando para ejecutar**:
  ```bash
  cd EventosVivos_Frontend
  npm run test
  ```
- **Generar Reporte de Cobertura (HTML)**:
  Asegúrese de situarse en el directorio del frontend, instale las dependencias y ejecute el comando de cobertura:
  ```bash
  cd EventosVivos_Frontend
  npm install
  npm run test:coverage
  ```
  Esto generará un reporte de cobertura en formato HTML interactivo dentro del directorio `coverage/EventosVivos_Frontend/`. Puede abrir el archivo `index.html` en cualquier navegador web para auditar el porcentaje de cobertura de cada archivo fuente.

---

## 🗄️ Base de Datos, Migraciones y Datos Semilla (Seeding)

El backend utiliza **Entity Framework Core** para el mapeo objeto-relacional (ORM):

- **Cadena de Conexión**: Se configura en el archivo `Back-End/EventosVivos_Api/appsettings.json` bajo la clave `ConnectionStrings:DefaultConnection`.
- **Ejecución Automática**: Al iniciar la aplicación API, el sistema ejecuta automáticamente `context.Database.MigrateAsync()` y la clase `DatabaseSeeder`, garantizando que la estructura de la base de datos esté actualizada.
- **Datos Iniciales**: Las migraciones y el seeder registran de manera automática:
  - **Venues/Locales por defecto**: Auditorio Central, Sala Norte y Arena Sur con sus respectivas capacidades.
  - **Tablas Maestras**: Estados por defecto de eventos y reservas, además de los tipos de evento válidos (`conferencia`, `taller`, `concierto`).
  - **Usuario Administrador**: Se registra automáticamente un usuario de prueba para gestionar eventos y reservas con las siguientes credenciales:
    - **Usuario**: `admin`
    - **Contraseña**: `Admin1234!`

