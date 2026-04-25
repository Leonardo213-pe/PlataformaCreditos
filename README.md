Plataforma de Créditos Web

Aplicación web desarrollada en ASP.NET Core (.NET 8) para la gestión de solicitudes de crédito, con autenticación, roles, validaciones de negocio, caché distribuido y sesiones.

Demo en producción

https://plataformacreditos-1.onrender.com/
Repositorio

https://github.com/Leonardo213-pe/PlataformaCreditos
Características principales
✔️ Registro e inicio de sesión (Identity)
✔️ Gestión de clientes
✔️ Registro de solicitudes de crédito
✔️ Validaciones de negocio:
Monto > 0
Máximo 10x ingresos (cliente)
Máximo 5x ingresos para aprobación (analista)
Solo una solicitud pendiente por cliente
✔️ Panel de analista:
Aprobar solicitudes
Rechazar con motivo obligatorio
✔️ Caché distribuido (Redis o memoria)
✔️ Uso de sesiones
✔️ Migraciones automáticas + seed de datos
👤 Roles del sistema
Cliente
Crear solicitudes de crédito
Ver historial
Filtrar solicitudes
Analista
Ver solicitudes pendientes
Aprobar o rechazar solicitudes
Usuarios de prueba
Rol	Email	Password
Cliente	cliente1@test.com	123456
Analista	analista@financiera.com	123456
Tecnologías usadas
.NET 8
ASP.NET Core MVC
Entity Framework Core
SQLite (dev / Render simple)
Redis (opcional)
Bootstrap
Docker (para despliegue)
Configuración local
1. Clonar repositorio
git clone https://github.com/Leonardo213-pe/PlataformaCreditos.git
cd PlataformaCreditos
2. Ejecutar proyecto
cd PlataformaCreditosWeb
dotnet run
Variables de entorno (Render)
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Data Source=app.db
Redis__ConnectionString=
Docker

El proyecto incluye un Dockerfile para despliegue en Render.

Build y ejecución local:

docker build -t plataforma-creditos .
docker run -p 8080:8080 plataforma-creditos
📌 Funcionalidades clave
📍 Solicitudes
Registro con validaciones de negocio
Filtrado por estado, monto y fechas
Caché por usuario
📍 Panel Analista
Listado de solicitudes pendientes
Validación automática de monto máximo
Aprobación/Rechazo con persistencia
📍 Caché
Implementado con IDistributedCache
Invalida al crear o procesar solicitudes
📍 Sesión
Guarda última solicitud visitada
Estructura del proyecto
PlataformaCreditos/
│
├── PlataformaCreditosWeb/
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   ├── Views/
│   └── Program.cs
│
├── Dockerfile
└── README.md
 Estado del proyecto

✔️ Funcional
✔️ Desplegado en Render
✔️ Cumple validaciones de negocio
✔️ Manejo de roles completo

 Licencia

Uso académico.