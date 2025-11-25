# Script PowerShell para construir e implementar la aplicación en AWS ECS
# Requisitos: AWS CLI, Docker

param(
    [string]$Environment = "dev",
    [string]$AwsRegion = "us-east-1",
    [string]$ImageTag = "latest"
)

$ErrorActionPreference = "Stop"

# Variables
$ProjectName = "investment-funds"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Deployment Script - Investment Funds API" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Region: $AwsRegion" -ForegroundColor Yellow
Write-Host "Image Tag: $ImageTag" -ForegroundColor Yellow
Write-Host "==================================================" -ForegroundColor Cyan

try {
    # Paso 1: Obtener información del repositorio ECR
    Write-Host "[1/6] Obteniendo información de ECR..." -ForegroundColor Green
    $RepositoryName = "$ProjectName-api-$Environment"
    
    $EcrRegistry = aws ecr describe-repositories `
        --repository-names $RepositoryName `
        --region $AwsRegion `
        --query 'repositories[0].repositoryUri' `
        --output text 2>$null

    if ([string]::IsNullOrEmpty($EcrRegistry)) {
        Write-Host "Error: Repositorio ECR no encontrado. Por favor ejecuta 'terraform apply' primero." -ForegroundColor Red
        exit 1
    }

    $EcrRepository = $EcrRegistry -replace ':.*$'
    Write-Host "ECR Repository: $EcrRepository" -ForegroundColor Yellow

    # Paso 2: Autenticarse en ECR
    Write-Host "[2/6] Autenticando en ECR..." -ForegroundColor Green
    $Password = aws ecr get-login-password --region us-east-1
    docker login --username AWS --password $Password 539042874282.dkr.ecr.us-east-1.amazonaws.com

    #$LoginPassword = aws ecr get-login-password --region $AwsRegion
    #$LoginPassword | docker login --username AWS --password-stdin $EcrRepository

    # Paso 3: Construir imagen Docker
    Write-Host "[3/6] Construyendo imagen Docker..." -ForegroundColor Green
    $DockerfilePath = Join-Path $PSScriptRoot "..\dockerfile"
    $BuildContext = Join-Path $PSScriptRoot ".."
    
    docker build -t "$ProjectName-api:$ImageTag" -f $DockerfilePath $BuildContext

    # Paso 4: Etiquetar imagen
    Write-Host "[4/6] Etiquetando imagen..." -ForegroundColor Green
    docker tag "$ProjectName-api:$ImageTag" "${EcrRepository}:${ImageTag}"
    docker tag "$ProjectName-api:$ImageTag" "${EcrRepository}:latest"

    # Paso 5: Subir imagen a ECR
    Write-Host "[5/6] Subiendo imagen a ECR..." -ForegroundColor Green
    docker push "${EcrRepository}:${ImageTag}"
    docker push "${EcrRepository}:latest"

    # Paso 6: Actualizar servicio ECS
    Write-Host "[6/6] Actualizando servicio ECS..." -ForegroundColor Green
    $ClusterName = "$ProjectName-cluster-$Environment"
    $ServiceName = "$ProjectName-service-$Environment"

    aws ecs update-service `
        --cluster $ClusterName `
        --service $ServiceName `
        --force-new-deployment `
        --region $AwsRegion `
        --query 'service.serviceName' `
        --output text

    Write-Host "==================================================" -ForegroundColor Cyan
    Write-Host "Despliegue iniciado exitosamente!" -ForegroundColor Green
    Write-Host "==================================================" -ForegroundColor Cyan
    Write-Host "Puedes monitorear el progreso con:" -ForegroundColor Yellow
    Write-Host "aws ecs describe-services --cluster $ClusterName --services $ServiceName --region $AwsRegion" -ForegroundColor White
    Write-Host "" 
    Write-Host "O ver los logs con:" -ForegroundColor Yellow
    Write-Host "aws logs tail /ecs/$ProjectName-$Environment --follow --region $AwsRegion" -ForegroundColor White
    Write-Host "==================================================" -ForegroundColor Cyan

}
catch {
    Write-Host "Error durante el despliegue: $_" -ForegroundColor Red
    exit 1
}
