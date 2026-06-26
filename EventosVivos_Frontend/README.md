# EventosVivos Frontend

Este proyecto constituye la interfaz de usuario de **EventosVivos**, una aplicación web SPA construida con **Angular** y TypeScript. Consume los servicios RESTful del backend para permitir la visualización de eventos, la creación y compra de entradas, el historial de reservas de clientes y la administración de transacciones y reportes.

---

## 🛠️ Arquitectura y Características de Diseño

El frontend implementa prácticas modernas de desarrollo en Angular:
- **Standalone Components**: Eliminación de módulos clásicos para una estructura ligera y de fácil lectura.
- **Signal Reactivity**: Uso de signals (`signal`, `computed`) para manejar estados locales reactivos (filtros, estados de carga, listas) de grano fino.
- **Dynamic CSS & Tailwind**: Interfaces altamente estéticas, con badges coloreados por estado y layouts responsivos adaptados tanto para móviles como pantallas grandes.
- **Reactive Forms & Custom Cross-Field Validators**:
  - `futureDateValidator`: Garantiza que las fechas sean futuras.
  - `weekendTimeValidator`: Valida la regla de negocio que restringe el inicio de eventos los fines de semana a un máximo de las 22:00h.
  - `capacityValidator`: Validador de formulario cruzado que compara el límite del recinto seleccionado con el input de capacidad máxima del evento.
  - `dateComparisonValidator`: Asegura que la fecha de finalización sea estrictamente posterior a la fecha de inicio.
- **Session Protection**:
  - `authGuard`: Protege rutas internas (como el Home del panel de administración).
  - Interceptor HTTP: Agrega tokens o cabeceras necesarias y maneja redirecciones automáticas en caso de fallos.
- **Interactive Modals**: Modales dinámicos desacoplados de confirmación (ej: LoginModal, confirmaciones en confirmación de pagos y cancelaciones).

---

## 📂 Páginas e Interfaz de Usuario

- **Crear Eventos**: Formulario interactivo con validaciones robustas y dinámicas en tiempo real para publicar nuevas propuestas culturales.
- **Eventos (Listado)**: Buscador con filtros por tipo de evento, recinto, fechas, estado y búsqueda de título.
- **Reservas**: Formulario de compra de entradas con limitación de compra en función del precio del ticket y cercanía al evento (RF-03/RN-05).
- **Mis Reservas**: Historial de compras por email, permitiendo la cancelación en línea.
- **Reporte Eventos**: Vista de control que muestra porcentaje de ocupación, totales vendidos, ingresos estimados y clasificaciones de estados.
- **Confirmación de Pagos (Gestión Reservas)**: Panel de administración para procesar confirmaciones de pagos pendientes y cancelaciones manuales con modales interactivos y registros ordenados por prioridad de pago.

---

## 🚀 Requisitos e Instalación Local

### Prerrequisitos
- Instalar [Node.js](https://nodejs.org/) (versión 18 o superior).

### Instalación y Servidor de Desarrollo
1. Sitúese en el directorio del frontend:
   ```bash
   cd EventosVivos_Frontend
   ```
2. Instale los paquetes NPM de dependencias:
   ```bash
   npm install
   ```
3. Inicie el servidor local:
   ```bash
   npm run start
   ```
4. Abra su navegador e ingrese a `http://localhost:4200/`.

---

## 🧪 Pruebas Automatizadas y Cobertura de Código

Las pruebas unitarias están configuradas con **Vitest** para una compilación y ejecución ultrarrápida. Prueban el comportamiento de los componentes, la reactividad de signals, validadores de formularios y llamadas simuladas a servicios.

### Ejecución de las Pruebas Unitarias
1. Sitúese en el directorio del frontend:
   ```bash
   cd EventosVivos_Frontend
   ```
2. Ejecute las pruebas unitarias:
   ```bash
   npm run test
   ```

### Generación del Reporte de Cobertura (HTML)
Para auditar la cobertura de código de forma gráfica e interactiva:
1. Ejecute el comando de cobertura:
   ```bash
   npm run test:coverage
   ```
2. Esto generará el reporte de cobertura en formato HTML dentro de la carpeta `coverage/EventosVivos_Frontend/`.
3. Abra el archivo `index.html` en su navegador para inspeccionar los porcentajes y líneas cubiertas por componente.
