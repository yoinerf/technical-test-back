# Secret para JWT
resource "aws_secretsmanager_secret" "jwt_secret" {
  name        = "${var.project_name}-jwt-secret2-${var.environment}"
  description = "JWT Secret Key for authentication"
}

resource "aws_secretsmanager_secret_version" "jwt_secret" {
  secret_id     = aws_secretsmanager_secret.jwt_secret.id
  secret_string = var.jwt_secret_key
}
