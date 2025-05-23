output "manager_public_ip" {
  description = "The public IP address of the Swarm manager instance"
  value       = aws_instance.manager.public_ip
}

output "worker_public_ips" {
  description = "List of public IP addresses of the worker instances"
  value       = aws_instance.worker[*].public_ip
}

output "manager_private_ip" {
  description = "The private IP address of the Swarm manager instance"
  value       = aws_instance.manager.private_ip
}

output "instance_ids" {
  description = "List of all EC2 instance IDs"
  value       = concat([aws_instance.manager.id], aws_instance.worker[*].id)
}

output "worker_private_ips" {
  description = "List of private IP addresses of the worker instances"
  value       = aws_instance.worker[*].private_ip
}
