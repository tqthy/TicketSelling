# VPC Outputs
output "vpc_id" {
  description = "The ID of the VPC"
  value       = module.vpc.vpc_id
}

output "public_subnet_ids" {
  description = "List of public subnet IDs"
  value       = module.vpc.public_subnet_ids
}

output "public_subnet_id" {
  description = "The ID of the first public subnet"
  value       = module.vpc.public_subnet_id
}

output "private_subnet_ids" {
  description = "List of private subnet IDs"
  value       = module.vpc.private_subnet_ids
}

output "private_subnet_id" {
  description = "The ID of the first private subnet"
  value       = module.vpc.private_subnet_id
}

# EC2 Outputs
output "manager_public_ip" {
  description = "Public IP address of the Swarm manager instance"
  value       = module.ec2.manager_public_ip
}

output "worker_public_ips" {
  description = "List of public IP addresses of the worker instances"
  value       = module.ec2.worker_public_ips
}

output "manager_private_ip" {
  description = "Private IP address of the Swarm manager instance"
  value       = module.ec2.manager_private_ip
}

# RDS Outputs
output "rds_endpoint" {
  description = "The connection endpoint for the RDS instance"
  value       = var.create_rds ? module.rds[0].db_endpoint : "RDS not created"
}

output "rds_identifier" {
  description = "The RDS instance identifier"
  value       = var.create_rds ? module.rds[0].db_identifier : "RDS not created"
}

output "rds_port" {
  description = "The database port"
  value       = var.create_rds ? module.rds[0].db_port : "RDS not created"
}

# Security Groups Outputs
output "swarm_sg_id" {
  description = "The ID of the Swarm security group"
  value       = module.security_groups.swarm_sg_id
}

output "rds_sg_id" {
  description = "The ID of the RDS security group"
  value       = module.security_groups.rds_sg_id
}

# Combined Outputs
output "all_security_group_ids" {
  description = "Map of all security group IDs"
  value       = module.security_groups.all_security_group_ids
}