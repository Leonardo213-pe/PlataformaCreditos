# 💳 Plataforma de Créditos

## Descripción

Portal web para gestión de solicitudes de crédito.
Pila: **ASP.NET Core MVC (.NET 8) + Identity + EF Core SQLite + Redis (opcional)**

---

## Pasos para correr localmente

### Requisitos

* SDK de .NET 8
* Git

---

### Instalación

```bash
git clone https://github.com/Leonardo213-pe/PlataformaCreditos.git
cd PlataformaCreditos/PlataformaCreditosWeb
dotnet restore
```

---

### Migraciones

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

### Correr el proyecto

```bash
dotnet run
```

Abrir en el navegador:
http://localhost:5247

---

## Variables de entorno

| Variable                             | Descripción          | Ejemplo                     |
| ------------------------------------ | -------------------- | --------------------------- |
| ASPNETCORE_ENVIRONMENT               | Entorno de ejecución | Production                  |
| ASPNETCORE_URLS                      | URL de escucha       | http://0.0.0.0:10000        |
| ConnectionStrings__DefaultConnection | Cadena SQLite        | Data Source=app.db          |
| Redis__ConnectionString              | URL Redis            | redis://user:pass@host:port |

---

## Credenciales iniciales

| Rol      | Email                                                     | Contraseña |
| -------- | --------------------------------------------------------- | ---------- |
| Cliente  | [cliente1@test.com](mailto:cliente1@test.com)             | 123456     |
| Analista | [analista@financiera.com](mailto:analista@financiera.com) | 123456     |

---

## Reglas de negocio

* El monto debe ser mayor a 0
* Máximo 10x ingresos del cliente
* Máximo 5x ingresos para aprobación
* Solo una solicitud pendiente por cliente
* Motivo obligatorio para rechazo

---

## Ramas del proyecto

| Rama                      | Descripción                      |
| ------------------------- | -------------------------------- |
| feature/bootstrap-dominio | P1: Modelos y configuración base |
| feature/solicitudes       | P2: Registro de solicitudes      |
| feature/cache-sesion      | P3: Caché y sesiones             |
| feature/panel-analista    | P4: Panel de analista            |
| deploy/render             | P6: Despliegue en Render         |

---

## URL en Render

https://plataformacreditos-1.onrender.com/

---
