# EventosVivos API (Backend)

La API de EventosVivos está desarrollada bajo la plataforma **.NET 8** utilizando **ASP.NET Core Web API**. Gestiona la persistencia de datos (en memoria) y procesa todas las validaciones de negocio relativas a la creación de eventos, recintos, reservación de entradas, confirmación de pagos y reportes de ocupación.

---

## 📂 Estructura del Proyecto

El backend se divide en dos proyectos principales:
1. **EventosVivos_Api**: Proyecto web principal que contiene:
   - **Controllers**: Rutas RESTful expuestas para el cliente.
   - **Services**: Capa lógica encargada de aplicar las validaciones y reglas de negocio.
   - **Repositories**: Almacenamiento y persistencia en memoria del estado del sistema.
   - **Models / DTOs**: Modelos de transporte de datos de entrada y salida.
2. **EventosVivos_Api.Tests**: Proyecto de pruebas unitarias implementado con **MSTest** que valida de forma automatizada los flujos críticos.

---

## ⚡ Endpoints RESTful Expuestos

### Eventos
- `GET /api/eventos` - Lista eventos culturales con filtros opcionales (tipo, rango de fechas, recinto, estado, búsqueda parcial por título).
- `POST /api/eventos` - Crea un nuevo evento. Valida capacidad del recinto, superposición horaria (RN-02) y restricción de fin de semana (RN-03).
- `GET /api/eventos/{id}` - Obtiene los detalles de un evento específico.
- `GET /api/eventos/reporte` - Genera un reporte detallado de ocupación, ingresos y totales de reservas (RN-06, RN-07).

### Venues / Recintos
- `GET /api/venues` - Retorna los lugares preexistentes en el sistema (Auditorio Central, Sala Norte, Arena Sur).

### Tipos de Evento
- `GET /api/tipoevento` - Retorna los tipos de eventos válidos (conferencia, taller, concierto).

### Reservas
- `GET /api/reservas` - Lista todas las reservas del sistema (usado en el panel administrativo).
- `GET /api/reservas/cliente/{email}` - Retorna las reservas asociadas a un correo de cliente (historial).
- `POST /api/reservas` - Crea una reserva con estado inicial `pendiente_pago`. Aplica límites transaccionales por precio (RN-05) y por tiempo (RF-03).
- `POST /api/reservas/{id}/confirmar-pago` - Confirma el pago de una reserva pendiente, asignando un código de reserva único `EV-xxxxxx` (RN-04).
- `POST /api/reservas/{id}/cancelar` - Cancela una reserva confirmada. Libera la capacidad del evento a menos que aplique penalización por tiempo menor a 48h (RN-07).

---

## 🚀 Requisitos y Configuración Local

### Prerrequisitos
- Instalar [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

### Compilación y Ejecución
1. Sitúese en el directorio del proyecto de API:
   ```bash
   cd Back-End/EventosVivos_Api
   ```
2. Restaure los paquetes NuGet:
   ```bash
   dotnet restore
   ```
3. Corra el servidor:
   ```bash
   dotnet run
   ```
   *El servidor estará escuchando por defecto en `http://localhost:5038` (consulte `Properties/launchSettings.json`).*

---

## 🧪 Pruebas Unitarias y Cobertura de Código (HTML)

La suite de pruebas automatizadas está construida sobre MSTest. Valida casos borde como colisión de horarios, límites de transacciones y penalizaciones por cancelación tardía.

### Ejecución de las Pruebas Unitarias
1. Sitúese en el directorio raíz de `Back-End`:
   ```bash
   cd Back-End
   ```
2. Ejecute las pruebas unitarias:
   ```bash
   dotnet test --settings test.runsettings
   ```

### Generación del Reporte de Cobertura (HTML)
Para auditar la cobertura de código del backend de forma gráfica e interactiva:
1. Sitúese en el directorio raíz de `Back-End`:
   ```bash
   cd Back-End
   ```
2. Restaure las herramientas locales configuradas (como `reportgenerator` y `dotnet-ef`):
   ```bash
   dotnet tool restore
   ```
3. Ejecute las pruebas unitarias recolectando los datos de cobertura:
   ```bash
   dotnet test --collect:"XPlat Code Coverage" --settings test.runsettings
   ```
   *Esto generará los archivos de cobertura de código en una carpeta con un identificador único bajo `EventosVivos_Api.Tests/TestResults/`.*
4. Procese el reporte XML con `reportgenerator` para convertirlo a HTML:
   ```bash
   dotnet reportgenerator -reports:"EventosVivos_Api.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
   ```
5. Abra el archivo `index.html` generado en la carpeta `Back-End/coveragereport/` en cualquier navegador web para visualizar el detalle.

---

## 🗄️ Base de Datos, Migraciones y Seeding

El sistema utiliza **Entity Framework Core** para interactuar con la base de datos SQL Server:

### 1. Cadena de Conexión
La cadena de conexión a la base de datos se encuentra especificada en `EventosVivos_Api/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=EventosVivos;Integrated Security=True;TrustServerCertificate=True"
}
```

### 2. Generación y Aplicación de Migraciones
- Para crear una nueva migración cuando modifique los modelos:
  ```bash
  dotnet ef migrations add NombreDeLaMigracion --project EventosVivos_Api
  ```
- Para aplicar manualmente las migraciones y actualizar la base de datos:
  ```bash
  dotnet ef database update --project EventosVivos_Api
  ```

### 3. Datos Semilla por Defecto
El sistema cuenta con un mecanismo de inicialización automática en `Program.cs` (`DatabaseSeeder`). Al levantar el proyecto, se aplican las migraciones pendientes y se insertan los siguientes datos iniciales si no existen:
- **Venues / Recintos**:
  - Auditorio Central (Capacidad: 200, Ciudad: Bogotá)
  - Sala Norte (Capacidad: 50, Ciudad: Bogotá)
  - Arena Sur (Capacidad: 500, Ciudad: Medellín)
- **Tablas Maestras**:
  - Tipos de Evento: `conferencia`, `taller`, `concierto`.
  - Estados de Eventos y Estados de Reservas.
- **Usuario Administrador**:
  Se registra automáticamente el siguiente perfil de administración para ingresar a la plataforma:
  - **Usuario**: `admin`
  - **Contraseña**: `Admin1234!`
