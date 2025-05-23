output "swarm_sg_id" {
  description = "The ID of the Swarm security group"
  value       = aws_security_group.swarm.id
}

output "rds_sg_id" {
  description = "The ID of the RDS security group"
  value       = aws_security_group.rds.id
}

output "all_security_group_ids" {
  description = "A map of all security group IDs"
  value = {
    swarm = aws_security_group.swarm.id
    rds   = aws_security_group.rds.id
  }
}
