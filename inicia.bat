@echo off
setlocal
cd /d "%~dp0"
if exist ".docker-local-ca.crt" (
  docker buildx build --load --secret id=local_ca,src=.docker-local-ca.crt -t agistudiop-api backend/Salao.Api
  if errorlevel 1 goto fail
  docker buildx build --load --secret id=local_ca,src=.docker-local-ca.crt -t agistudiop-web frontend
  if errorlevel 1 goto fail
  docker compose up -d
) else (
  docker compose up --build -d
)
if errorlevel 1 goto fail
echo.
echo Interface: http://localhost:3000
echo API:       http://localhost:5000/swagger
goto end
:fail
echo.
echo Nao foi possivel construir ou iniciar a aplicacao.
:end
pause
