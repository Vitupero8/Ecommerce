#!/bin/bash
# =========================================================
# Runs ONCE, as MySQL root, the first time the mysql
# container starts with an empty data volume.
# Creates one least-privilege user per microservice.
# Passwords come from environment variables (.env), never from this file.
# =========================================================
set -eu

# Stop with a clear error if any password is missing
: "${CUSTOMER_DB_PASSWORD:?CUSTOMER_DB_PASSWORD is not set}"
: "${PRODUCT_DB_PASSWORD:?PRODUCT_DB_PASSWORD is not set}"
: "${CART_DB_PASSWORD:?CART_DB_PASSWORD is not set}"
: "${PAYMENT_DB_PASSWORD:?PAYMENT_DB_PASSWORD is not set}"
: "${SHIPMENT_DB_PASSWORD:?SHIPMENT_DB_PASSWORD is not set}"

mysql --protocol=socket -uroot -p"${MYSQL_ROOT_PASSWORD}" <<SQL
-- '%' = any host. Needed because services connect from OTHER containers,
-- not from 'localhost'. MySQL is not published outside the Docker network.
CREATE USER IF NOT EXISTS 'customer_user'@'%' IDENTIFIED BY '${CUSTOMER_DB_PASSWORD}';
CREATE USER IF NOT EXISTS 'product_user'@'%'  IDENTIFIED BY '${PRODUCT_DB_PASSWORD}';
CREATE USER IF NOT EXISTS 'cart_user'@'%'     IDENTIFIED BY '${CART_DB_PASSWORD}';
CREATE USER IF NOT EXISTS 'payment_user'@'%'  IDENTIFIED BY '${PAYMENT_DB_PASSWORD}';
CREATE USER IF NOT EXISTS 'shipment_user'@'%' IDENTIFIED BY '${SHIPMENT_DB_PASSWORD}';

-- CustomerService
GRANT SELECT, INSERT, UPDATE, DELETE ON Ecommerce.Customers TO 'customer_user'@'%';

-- ProductService
GRANT SELECT, INSERT, UPDATE, DELETE ON Ecommerce.Product TO 'product_user'@'%';

-- CartService
GRANT SELECT, INSERT, UPDATE, DELETE ON CartService.Carts     TO 'cart_user'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON CartService.CartItems TO 'cart_user'@'%';

-- PaymentService
GRANT SELECT, INSERT, UPDATE         ON PaymentService.Orders         TO 'payment_user'@'%';
-- SELECT is required: EF Core reads back the auto-generated OrderItemID after INSERT
GRANT SELECT, INSERT                 ON PaymentService.OrderItem      TO 'payment_user'@'%';
GRANT SELECT, INSERT, UPDATE         ON PaymentService.Payments       TO 'payment_user'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON PaymentService.PaymentMethods TO 'payment_user'@'%';

-- ShipmentService
GRANT SELECT, INSERT, UPDATE ON ShipmentService.Shipments TO 'shipment_user'@'%';
SQL

echo "02-users.sh: service users created with least-privilege grants."
