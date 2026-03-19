# ============================================================================
# EvalSystem - Docker Setup Script
# ============================================================================
# Prerequisites: Docker Desktop running
#
# This script:
#   1. Starts SQL Server in Docker
#   2. Waits for it to be ready
#   3. Creates the database via EF migrations
#   4. Seeds initial data
# ============================================================================

param(
    [switch]$SkipSeed,
    [switch]$ResetDb
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
if (-not $root) { $root = Split-Path -Parent (Resolve-Path ".") }
# If run from docker/ folder, go up one level
if ((Split-Path $root -Leaf) -eq "docker") { $root = Split-Path $root -Parent }

$SA_PASSWORD = "EvalSystem_Dev2024!"
$DB_NAME = "EvalSystemDb"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  EvalSystem - Docker Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ── Step 1: Start Docker container ──────────────────────────────────────────
Write-Host "[1/4] Starting SQL Server container..." -ForegroundColor Yellow
Push-Location $root
try {
    if ($ResetDb) {
        Write-Host "  -> Resetting: removing volume..." -ForegroundColor DarkYellow
        docker compose down -v 2>$null
    }
    docker compose up -d
} finally {
    Pop-Location
}

# ── Step 2: Wait for SQL Server to be ready ─────────────────────────────────
Write-Host "[2/4] Waiting for SQL Server to be ready..." -ForegroundColor Yellow
$maxRetries = 30
$retry = 0
do {
    $retry++
    Start-Sleep -Seconds 2
    try {
        $result = sqlcmd -S "localhost" -U sa -P $SA_PASSWORD -C -Q "SELECT 1" -h -1 -W 2>&1
        if ($result -match "1") {
            Write-Host "  -> SQL Server is ready! (attempt $retry)" -ForegroundColor Green
            break
        }
    } catch {}
    if ($retry -ge $maxRetries) {
        Write-Host "  -> ERROR: SQL Server did not start after $maxRetries attempts" -ForegroundColor Red
        exit 1
    }
    Write-Host "  -> Waiting... (attempt $retry/$maxRetries)" -ForegroundColor DarkGray
} while ($true)

# ── Step 3: Run EF Migrations ──────────────────────────────────────────────
Write-Host "[3/4] Applying EF migrations..." -ForegroundColor Yellow
Push-Location $root
try {
    $env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=$DB_NAME;User Id=sa;Password=$SA_PASSWORD;TrustServerCertificate=True;"
    dotnet ef database update --project src/EvalSystem.Infrastructure --startup-project src/EvalSystem.API
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  -> ERROR: Migration failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "  -> Migrations applied successfully" -ForegroundColor Green
} finally {
    Remove-Item Env:\ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
    Pop-Location
}

# ── Step 4: Seed data ──────────────────────────────────────────────────────
if (-not $SkipSeed) {
    Write-Host "[4/4] Seeding initial data..." -ForegroundColor Yellow
    $seedFile = Join-Path $root "docker\seed-data.sql"
    if (Test-Path $seedFile) {
        # Check if data already exists
        $existing = sqlcmd -S "localhost" -U sa -P $SA_PASSWORD -C -d $DB_NAME -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM Usuarios" 2>&1
        if ($existing.Trim() -gt 0) {
            Write-Host "  -> Database already has data ($($existing.Trim()) users). Skipping seed." -ForegroundColor DarkYellow
            Write-Host "     To re-seed, run: .\docker\setup.ps1 -ResetDb" -ForegroundColor DarkGray
        } else {
            sqlcmd -S "localhost" -U sa -P $SA_PASSWORD -C -d $DB_NAME -i $seedFile
            if ($LASTEXITCODE -ne 0) {
                Write-Host "  -> WARNING: Seed script had errors (non-fatal)" -ForegroundColor DarkYellow
            } else {
                Write-Host "  -> Seed data inserted successfully" -ForegroundColor Green
            }
        }
    } else {
        Write-Host "  -> Seed file not found at $seedFile, skipping" -ForegroundColor DarkYellow
    }
} else {
    Write-Host "[4/4] Skipping seed (--SkipSeed)" -ForegroundColor DarkGray
}

# ── Done ────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Setup complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Connection string:" -ForegroundColor Cyan
Write-Host "  Server=localhost;Database=$DB_NAME;User Id=sa;Password=$SA_PASSWORD;TrustServerCertificate=True;" -ForegroundColor White
Write-Host ""
Write-Host "Test users:" -ForegroundColor Cyan
Write-Host "  Admin:     admin@eval.com / Admin123!" -ForegroundColor White
Write-Host "  Evaluador: chelin@juji.com / [original password]" -ForegroundColor White
Write-Host "  Candidato: candidato@test.com / Test123!" -ForegroundColor White
Write-Host ""
Write-Host "Start the API:" -ForegroundColor Cyan
Write-Host "  dotnet run --project src\EvalSystem.API --urls `"http://localhost:5099`"" -ForegroundColor White
Write-Host ""
