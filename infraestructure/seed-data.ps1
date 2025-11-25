# Script para poblar datos iniciales en DynamoDB
# Ejecutar después de terraform apply

param(
    [string]$Environment = "dev",
    [string]$Region = "us-east-1",
    [string]$ProjectName = "investment-funds"
)

$ErrorActionPreference = "Stop"

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Poblar Datos Iniciales - Investment Funds" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Region: $Region" -ForegroundColor Yellow
Write-Host ""

# Nombres de tablas según Terraform
$FundsTable = "$ProjectName-funds-$Environment"

Write-Host "[1/2] Verificando tabla de fondos..." -ForegroundColor Green
Write-Host "  1. FPV_BTG_PACTUAL_RECAUDADORA..." -ForegroundColor White
aws dynamodb put-item --table-name "$FundsTable" --item '{\"FundId\":{\"S\":\"1\"},\"Name\":{\"S\":\"FPV_BTG_PACTUAL_RECAUDADORA\"},\"MinAmount\":{\"N\":\"75000\"},\"Category\":{\"S\":\"FPV\"},\"IsActive\":{\"BOOL\":true},\"CreatedAt\":{\"S\":\"2025-11-21T00:00:00Z\"}}' --region $Region --no-cli-pager

Write-Host "  2. FPV_BTG_PACTUAL_ECOPETROL..." -ForegroundColor White
aws dynamodb put-item --table-name "$FundsTable" --item '{\"FundId\":{\"S\":\"2\"},\"Name\":{\"S\":\"FPV_BTG_PACTUAL_ECOPETROL\"},\"MinAmount\":{\"N\":\"125000\"},\"Category\":{\"S\":\"FPV\"},\"IsActive\":{\"BOOL\":true},\"CreatedAt\":{\"S\":\"2025-11-21T00:00:00Z\"}}' --region $Region --no-cli-pager

Write-Host "  3. DEUDAPRIVADA..." -ForegroundColor White
aws dynamodb put-item --table-name "$FundsTable" --item '{\"FundId\":{\"S\":\"3\"},\"Name\":{\"S\":\"DEUDAPRIVADA\"},\"MinAmount\":{\"N\":\"50000\"},\"Category\":{\"S\":\"FIC\"},\"IsActive\":{\"BOOL\":true},\"CreatedAt\":{\"S\":\"2025-11-21T00:00:00Z\"}}' --region $Region --no-cli-pager

Write-Host "  4. FDO-ACCIONES..." -ForegroundColor White
aws dynamodb put-item --table-name "$FundsTable" --item '{\"FundId\":{\"S\":\"4\"},\"Name\":{\"S\":\"FDO-ACCIONES\"},\"MinAmount\":{\"N\":\"250000\"},\"Category\":{\"S\":\"FIC\"},\"IsActive\":{\"BOOL\":true},\"CreatedAt\":{\"S\":\"2025-11-21T00:00:00Z\"}}' --region $Region --no-cli-pager

Write-Host "  5. FPV_BTG_PACTUAL_DINAMICA..." -ForegroundColor White
aws dynamodb put-item --table-name "$FundsTable" --item '{\"FundId\":{\"S\":\"5\"},\"Name\":{\"S\":\"FPV_BTG_PACTUAL_DINAMICA\"},\"MinAmount\":{\"N\":\"100000\"},\"Category\":{\"S\":\"FPV\"},\"IsActive\":{\"BOOL\":true},\"CreatedAt\":{\"S\":\"2025-11-21T00:00:00Z\"}}' --region $Region --no-cli-pager

Write-Host "`n=================================================" -ForegroundColor Cyan
Write-Host "✅ Datos iniciales cargados exitosamente!" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Puedes verificar los fondos con:" -ForegroundColor Yellow
Write-Host "aws dynamodb scan --table-name $FundsTable --region $Region" -ForegroundColor White
