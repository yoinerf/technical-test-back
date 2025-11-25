# Investment Funds Platform - .NET 9 API

El proyecto implementa una API REST desarrollada en .NET 9 (Clean Architecture) con base de datos DynamoDB. Realizada para gestionar clientes, fondos, suscripciones y transacciones.

Se implementó siguiendo **Clean Architecture** y principios **SOLID**.

## API Disponible en el enlace:
https://d3j5dokywqwu74.cloudfront.net/health

## Capas de la Aplicación

- **API**: Capa de presentación que expone los endpoints REST.
- **Application**: Capa de aplicación que contiene los servicios y la lógica de negocio.
- **Domain**: Capa de dominio que contiene las entidades y las interfaces de repositorio.
- **Infrastructure**: Capa de infraestructura que contiene la implementación de los repositorios y los servicios externos.

### Funcionalidades

1. Suscribirse a un nuevo fondo (apertura).
2. Cancelar la suscripcio n a un fondo actual.
3. Ver historial de transacciones (aperturas y cancelaciones).
4. Enviar notificacio n por email o SMS segu n preferencia del usuario al suscribirse a un fondo.

## 🚀 Tecnologías

- .NET 9 SDK
- Docker Desktop (para desarrollo local)
- AWS CLI (para despliegue en AWS)
- Terraform ≥ 1.0 (para despliegue en AWS)

### Enpoints principales 

- GET /health
- GET /api/auth/checkStatus
- POST /api/auth/register
- POST /api/auth/login
- GET /api/funds
- GET /api/subscriptions
- POST /api/subscriptions
- DELETE /api/subscriptions/{id}
- GET /api/transactions

---

## 🔐 Autenticación

Todos los endpoints (excepto `/api/auth/*` y `/health`) requieren autenticación JWT.

### Headers Requeridos

```http
Authorization: Bearer <access_token>
Content-Type: application/json
```

### Registrar Usuario

Registra un nuevo cliente en la plataforma.

```http
POST /api/auth/register
```

**Headers:**
```
Content-Type: application/json
```

**Body:**
```json
{
  "email": "juan.perez@example.com",
  "password": "SecurePassword123!",
  "phone": "+573001234567",
  "notificationPreference": 0
}
```

**NotificationPreference:**
- `0` = Email
- `1` = SMS
- `2` = Ambos (Email + SMS)

**Respuesta Exitosa (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "customerId": "CUST-123456",
  "email": "juan.perez@example.com",
  "expiresAt": "2024-11-25T23:15:00Z"
}
```

**Errores Posibles:**
- `400 Bad Request` - Email ya existe o datos inválidos
- `404 Not Found` - Error en el registro

---

### Iniciar Sesión
Autentica un usuario existente.

```http
POST /api/auth/login
```

**Body:**
```json
{
  "email": "juan.perez@example.com",
  "password": "SecurePassword123!"
}
```

**Respuesta Exitosa (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "customerId": "CUST-123456",
  "email": "juan.perez@example.com",
  "expiresAt": "2024-11-25T23:15:00Z"
}
```

**Errores Posibles:**
- `401 Unauthorized` - Credenciales incorrectas

---

## Desplegar en AWS
### 1. Desplegar Infraestructura

```powershell
cd "Infraestructure\terraform"

# Inicializar
terraform init

# Aplicar
terraform apply
```

### 2. Construir y Desplegar la Aplicación

```powershell
cd ..
.\deploy.ps1 -Environment dev -ImageTag v1.0.4
```

### 3. Poblar Datos Iniciales de Fondos

```powershell
.\seed-data.ps1 -Environment dev
```
---

## 📊 Datos de Fondos que se Cargarán

| ID | Nombre | Monto Mínimo | Categoría |
|----|--------|--------------|-----------|
| 1 | FPV_BTG_PACTUAL_RECAUDADORA | $75,000 | FPV |
| 2 | FPV_BTG_PACTUAL_ECOPETROL | $125,000 | FPV |
| 3 | DEUDAPRIVADA | $50,000 | FIC |
| 4 | FDO-ACCIONES | $250,000 | FIC |
| 5 | FPV_BTG_PACTUAL_DINAMICA | $100,000 | FPV |

---