# EventosVivos - Sistema de Gestión de Reservas

EventosVivos es una plataforma para la creación de eventos culturales, conferencias y talleres, así como la reserva y gestión de entradas. El sistema está diseñado para solucionar problemas críticos como el sobrecupo (overbooking), la superposición de horarios en recintos, y la automatización del procesamiento de pagos y cancelaciones.

---

## 🌐 Demo & Diseño

- **Demo en vivo**: Puedes probar la aplicación en funcionamiento en [https://eventos-vivos.azurewebsites.net](https://eventos-vivos.azurewebsites.net).
- **Diseño de Interfaz**: El diseño de la aplicación fue realizado mediante Inteligencia Artificial utilizando la herramienta **Stitch de Google**. Puedes ver los diseños y prototipos interactivos en el siguiente enlace: [Proyectos de Stitch - EventosVivos](https://stitch.withgoogle.com/projects/4130516836670989072).

**Nota:** El demo puede demorar en cargar dado que la DB se pausa por inactividad

---

## 🏗️ Arquitectura General

El proyecto utiliza una arquitectura desacoplada tipo **Single Page Application (SPA)** con los siguientes pilares:

```
[ Frontend: Angular (v21) ] <--- HTTP REST ---> [ Backend: ASP.NET Core Web API (.NET 8) ]
                                                            |
                                               [ Entity Framework Core (ORM) ]
                                                            |
                                                   [ Base de Datos SQL ]
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

## 🧭 Justificación de la Arquitectura

Las decisiones estructurales de EventosVivos responden a criterios de **mantenibilidad**, **testabilidad** y **separación de responsabilidades** adecuados al dominio del problema. A continuación se detalla el razonamiento detrás de cómo se organizó cada capa del sistema.

---

### 🔷 Separación Frontend / Backend (Arquitectura Desacoplada)

El sistema está dividido en dos proyectos completamente independientes que se comunican exclusivamente mediante una **API REST HTTP**. Esta separación no es técnica sino conceptual: el backend es el **guardián de las reglas de negocio** y el frontend es el **canal de interacción con el usuario**. Ninguno conoce los detalles internos del otro.

Esto implica que:
- Si una regla de negocio cambia (ej: el límite de entradas por precio), el cambio ocurre **únicamente en el backend**, sin necesidad de modificar el frontend.
- Si el diseño de la interfaz cambia, el **backend no se ve afectado**.
- Ambos proyectos pueden ser probados de forma independiente con mocks del otro extremo.

---

### 🔷 Backend: Capas con Responsabilidad Única

El backend se organiza en tres capas bien definidas que siguen el principio de **responsabilidad única (SRP)**:

```
HTTP Request
     ↓
[ Controller ]   →  Recibe la petición, valida la forma del dato (model binding), delega al servicio
     ↓
[  Service   ]   →  Aplica TODAS las reglas de negocio (RN-01 a RN-07), lanza excepciones si se violan
     ↓
[ Repository ]   →  Solo persiste o consulta datos. No conoce ninguna regla de negocio
```

**¿Por qué esta separación?**

- Los **Controllers** no contienen `if` de negocio. Si un controller crece en lógica, es una señal de que algo está mal. Esta restricción hace que las pruebas de los servicios sean el centro de la cobertura de negocio.
- Los **Services** son el núcleo del sistema. Concentrar aquí toda la lógica permite probar exhaustivamente las 7 reglas de negocio sin necesidad de levantar HTTP ni base de datos.
- Los **Repositories** abstraen el origen de datos. Hoy es SQL Server vía Entity Framework; si en el futuro cambia a otro motor, ninguna capa de negocio se ve afectada.

**DTOs como contrato público**

Las entidades de base de datos nunca se exponen directamente en la API. Se utilizan **DTOs (Data Transfer Objects)** para:
- Evitar la sobre-exposición de campos sensibles o internos.
- Permitir que el modelo interno de la base de datos evolucione sin romper el contrato con el frontend.
- Calcular y agregar campos derivados en la respuesta (ej: `EntradasDisponibles` se calcula como `Capacidad - SumaDeReservas` en el momento de la consulta, no se persiste).

---

### 🔷 Frontend: Componentes Aislados con Estado Local

El frontend está organizado siguiendo el principio de **componentes autocontenidos**: cada componente es responsable de su propio estado, su propia lógica de presentación y su propia comunicación con la API a través de servicios inyectados.

```
[ Página / Componente Contenedor ]
    ↓  inyecta
[ Servicio HTTP ]   →  única fuente de verdad para llamadas a la API
    ↓  emite datos al componente
[ Estado local (Signals) ]   →  actualiza la vista de forma reactiva
```

**¿Por qué estado local y no un store global?**

Los datos de cada página (`reservas`, `eventos`, `errorMessage`) no necesitan ser compartidos entre páginas distintas. Un store global centralizado (como NgRx/Redux) aportaría complejidad sin beneficio real para este dominio. El estado se coloca **donde se usa**, lo que reduce el acoplamiento entre páginas y facilita entender el flujo de datos con solo leer un archivo.

**¿Por qué una carpeta `core/` separada de `pages/`?**

```
src/app/
├── core/           → Infraestructura compartida (servicios, guards, interceptors, componentes globales)
└── pages/          → Funcionalidades de negocio (cada página con sus propios componentes internos)
```

- `core/` contiene elementos que **no pertenecen a ninguna página específica**: el interceptor de autenticación, los guards de rutas, los servicios HTTP y los componentes de layout (header, footer). Son transversales al sistema.
- `pages/` contiene cada caso de uso del usuario como una unidad autónoma. La página de reservas no conoce la de creación de eventos, y viceversa.

Esta estructura permite que un desarrollador nuevo encuentre rápidamente dónde está la lógica de una funcionalidad sin necesidad de navegar por toda la aplicación.

---

### 🔷 Seguridad: Verificación Proactiva en el Cliente

La autenticación sigue un flujo **stateless en el servidor y proactivo en el cliente**:

1. El backend emite un **JWT** al autenticarse. No almacena sesiones; cada solicitud es autónoma.
2. El **`AuthInterceptor`** intercepta todas las solicitudes HTTP salientes antes de enviarlas. Verifica si el token guardado en `localStorage` ha expirado; si es así, realiza el logout localmente y redirige al inicio **sin esperar a que el servidor rechace la petición**. Esto evita errores 401 inesperados para el usuario.
3. El **`AuthGuard`** bloquea la navegación a rutas del panel administrativo si el usuario no está autenticado, actuando como primera línea de defensa en el enrutamiento.

Esta arquitectura de seguridad en capas garantiza que el acceso no autorizado se detecte y gestione lo antes posible en el ciclo de vida de cada interacción.

---

### 🔷 Pruebas: Aislamiento Total de Dependencias

Tanto en el backend como en el frontend, las pruebas están diseñadas para **no depender de infraestructura externa** (base de datos real, servidor HTTP, etc.):

- En el **backend**, los servicios reciben sus dependencias por inyección. En las pruebas, se reemplazan por mocks o implementaciones en memoria, permitiendo validar cada regla de negocio de forma aislada y reproducible.
- En el **frontend**, los servicios HTTP se reemplazan por mocks en el `TestBed` de Angular. Los componentes se prueban con datos controlados, verificando tanto la lógica TypeScript como el resultado en el DOM renderizado.

Este enfoque garantiza que las pruebas sean rápidas, deterministas y no dependan del estado externo del sistema.

---

## 🛠️ Tecnologías Utilizadas

- **Backend**:
  - .NET 8 / C# 12
  - ASP.NET Core Web API
  - Entity Framework Core
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
