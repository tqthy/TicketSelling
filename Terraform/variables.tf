variable "project_name" {
  description = "The name of the project (used for resource naming and tags)"
  type        = string
  default     = "ticket-selling"
}

variable "aws_region" {
  description = "AWS region to deploy resources"
  type        = string
  default     = "us-east-1"
}

variable "ec2_key_name" {
  description = "Name of the EC2 key pair"
  type        = string
  default     = "ticket-selling-key"
}

variable "private_key_path" {
  description = "Path to the private key file for SSH access"
  type        = string
  default     = "./ticket-selling-key.pem"
}

variable "worker_count" {
  description = "Number of worker nodes to deploy"
  type        = number
  default     = 2
}

variable "db_username" {
  description = "Database administrator username"
  type        = string
  sensitive   = true
  default     = "admin"
}

variable "db_password" {
  description = "Database administrator password"
  type        = string
  sensitive   = true
  default     = "" # Should be set via environment variable or .tfvars
}

variable "db_name" {
  description = "Name of the database to create"
  type        = string
  default     = "ticketdb"
}

variable "vpc_cidr" {
  description = "The CIDR block for the VPC"
  type        = string
  default     = "10.10.0.0/16"
}

variable "public_subnet_cidrs" {
  description = "The CIDR blocks for the public subnets"
  type        = list(string)
  default     = ["10.10.1.0/24", "10.10.2.0/24"]
}

variable "private_subnet_cidrs" {
  description = "The CIDR blocks for the private subnets"
  type        = list(string)
  default     = ["10.10.3.0/24", "10.10.4.0/24"]
}

variable "availability_zones" {
  description = "The availability zones to deploy resources into"
  type        = list(string)
  default     = ["us-east-1a", "us-east-1b"]
}

variable "tags" {
  description = "A map of tags to add to all resources"
  type        = map(string)
  default = {
    Environment = "production"
    Terraform   = "true"
    Project     = "ticket-selling"
  }
}