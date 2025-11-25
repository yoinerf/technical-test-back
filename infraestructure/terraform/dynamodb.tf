# Tabla de Clientes
resource "aws_dynamodb_table" "customers" {
  name         = "${var.project_name}-customers-${var.environment}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "CustomerId"

  attribute {
    name = "CustomerId"
    type = "S"
  }

  attribute {
    name = "Email"
    type = "S"
  }

  global_secondary_index {
    name            = "EmailIndex"
    hash_key        = "Email"
    projection_type = "ALL"
  }

  tags = {
    Name = "${var.project_name}-customers"
  }
}

# Tabla de Fondos
resource "aws_dynamodb_table" "funds" {
  name         = "${var.project_name}-funds-${var.environment}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "FundId"

  attribute {
    name = "FundId"
    type = "S"
  }

  tags = {
    Name = "${var.project_name}-funds"
  }
}

# Tabla de Transacciones
resource "aws_dynamodb_table" "transactions" {
  name         = "${var.project_name}-transactions-${var.environment}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "TransactionId"

  attribute {
    name = "TransactionId"
    type = "S"
  }

  attribute {
    name = "CustomerId"
    type = "S"
  }

  global_secondary_index {
    name            = "CustomerIndex"
    hash_key        = "CustomerId"
    projection_type = "ALL"
  }

  tags = {
    Name = "${var.project_name}-transactions"
  }
}

# Tabla de Suscripciones
resource "aws_dynamodb_table" "subscriptions" {
  name         = "${var.project_name}-subscriptions-${var.environment}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "CustomerId"
  range_key    = "FundId"

  attribute {
    name = "CustomerId"
    type = "S"
  }

  attribute {
    name = "FundId"
    type = "S"
  }

  tags = {
    Name = "${var.project_name}-subscriptions"
  }
}
