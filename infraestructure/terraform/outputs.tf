output "api_endpoint" {
  description = "API endpoint URL (Load Balancer DNS)"
  value       = "http://${aws_lb.main.dns_name}"
}

output "alb_dns_name" {
  description = "Application Load Balancer DNS name"
  value       = aws_lb.main.dns_name
}

output "ecs_cluster_name" {
  description = "ECS Cluster name"
  value       = aws_ecs_cluster.main.name
}

output "ecs_service_name" {
  description = "ECS Service name"
  value       = aws_ecs_service.api.name
}

output "ecr_repository_url" {
  description = "ECR Repository URL"
  value       = aws_ecr_repository.api.repository_url
}

output "dynamodb_tables" {
  description = "DynamoDB table names"
  value = {
    customers      = aws_dynamodb_table.customers.name
    funds          = aws_dynamodb_table.funds.name
    transactions   = aws_dynamodb_table.transactions.name
    subscriptions  = aws_dynamodb_table.subscriptions.name
  }
}

output "vpc_id" {
  description = "VPC ID"
  value       = aws_vpc.main.id
}