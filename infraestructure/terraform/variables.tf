variable "aws_region" {
  description = "AWS Region"
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "Environment (dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "project_name" {
  description = "Project name"
  type        = string
  default     = "investment-funds"
}

variable "jwt_secret_key" {
  description = "JWT Secret Key"
  type        = string
  sensitive   = true
}

variable "ses_from_email" {
  description = "SES From Email"
  type        = string
  default     = "noreply@example.com"
}

# ECS Configuration
variable "ecs_task_cpu" {
  description = "ECS Task CPU units (256, 512, 1024, 2048, 4096)"
  type        = string
  default     = "512"
}

variable "ecs_task_memory" {
  description = "ECS Task Memory in MB (512, 1024, 2048, 4096, 8192)"
  type        = string
  default     = "1024"
}

variable "ecs_desired_count" {
  description = "Desired number of ECS tasks"
  type        = number
  default     = 2
}

variable "ecs_min_capacity" {
  description = "Minimum number of ECS tasks for auto-scaling"
  type        = number
  default     = 1
}

variable "ecs_max_capacity" {
  description = "Maximum number of ECS tasks for auto-scaling"
  type        = number
  default     = 4
}

variable "image_tag" {
  description = "Docker image tag to deploy"
  type        = string
  default     = "latest"
}
