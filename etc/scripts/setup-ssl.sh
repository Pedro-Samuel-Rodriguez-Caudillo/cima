#!/bin/bash
# Script para configurar SSL con Let's Encrypt en produccion
# Uso: sudo ./etc/scripts/setup-ssl.sh cima.com admin@cima.com

set -e

DOMAIN=${1:-cima.com}
EMAIL=${2:-admin@cima.com}
STAGING=${3:-0}  # 1 para usar staging de Let's Encrypt (testing)

echo "========================================="
echo "Configuracion SSL para CIMA"
echo "========================================="
echo "Dominio: $DOMAIN"
echo "Email: $EMAIL"
echo "Staging: $STAGING"
echo ""

# Verificar que se ejecuta como root
if [ "$EUID" -ne 0 ]; then 
    echo "ERROR: Este script debe ejecutarse como root (sudo)"
    exit 1
fi

# Verificar argumentos
if [ -z "$DOMAIN" ] || [ -z "$EMAIL" ]; then
    echo "Uso: sudo $0 <dominio> <email> [staging]"
    echo "Ejemplo: sudo $0 cima.com admin@cima.com 0"
    exit 1
fi

# Instalar certbot si no esta instalado
echo "[1/6] Verificando certbot..."
if ! command -v certbot &> /dev/null; then
    echo "Instalando certbot..."
    apt-get update
    apt-get install -y certbot
else
    echo "Certbot ya instalado: $(certbot --version)"
fi

# Crear directorios necesarios
echo "[2/6] Creando directorios..."
mkdir -p /etc/nginx/ssl
mkdir -p /var/www/certbot
chown -R www-data:www-data /var/www/certbot

# Detener Nginx si esta corriendo
echo "[3/6] Deteniendo Nginx..."
if systemctl is-active --quiet nginx; then
    systemctl stop nginx
fi

# Obtener certificado
echo "[4/6] Obteniendo certificado SSL..."
if [ "$STAGING" -eq 1 ]; then
    echo "Usando servidor de staging de Let's Encrypt (para testing)"
    certbot certonly --standalone \
        --non-interactive \
        --agree-tos \
        --staging \
        --email "$EMAIL" \
        -d "$DOMAIN" \
        -d "www.$DOMAIN"
else
    echo "Usando servidor de produccion de Let's Encrypt"
    certbot certonly --standalone \
        --non-interactive \
        --agree-tos \
        --email "$EMAIL" \
        -d "$DOMAIN" \
        -d "www.$DOMAIN"
fi

# Copiar certificados a directorio de Nginx
echo "[5/6] Copiando certificados..."
cp "/etc/letsencrypt/live/$DOMAIN/fullchain.pem" /etc/nginx/ssl/
cp "/etc/letsencrypt/live/$DOMAIN/privkey.pem" /etc/nginx/ssl/
cp "/etc/letsencrypt/live/$DOMAIN/chain.pem" /etc/nginx/ssl/
chmod 644 /etc/nginx/ssl/*.pem
chown root:root /etc/nginx/ssl/*.pem

# Configurar renovacion automatica
echo "[6/6] Configurando renovacion automatica..."

# Crear script de renovacion
cat > /etc/cron.daily/certbot-renew << 'EOF'
#!/bin/bash
# Renovar certificados SSL y recargar Nginx

certbot renew --quiet --post-hook "docker exec cima-nginx-prod nginx -s reload"

# Copiar certificados actualizados
if [ -d "/etc/letsencrypt/live" ]; then
    DOMAIN=$(ls /etc/letsencrypt/live | grep -v README | head -n 1)
    if [ -n "$DOMAIN" ]; then
        cp "/etc/letsencrypt/live/$DOMAIN/fullchain.pem" /etc/nginx/ssl/
        cp "/etc/letsencrypt/live/$DOMAIN/privkey.pem" /etc/nginx/ssl/
        cp "/etc/letsencrypt/live/$DOMAIN/chain.pem" /etc/nginx/ssl/
    fi
fi
EOF

chmod +x /etc/cron.daily/certbot-renew

echo ""
echo "========================================="
echo "SSL configurado exitosamente!"
echo "========================================="
echo "Certificados ubicados en: /etc/nginx/ssl/"
echo "Renovacion automatica: /etc/cron.daily/certbot-renew"
echo ""
echo "Proximos pasos:"
echo "1. Iniciar Nginx: systemctl start nginx"
echo "2. Verificar: https://$DOMAIN"
echo "3. Verificar renovacion: certbot renew --dry-run"
echo ""