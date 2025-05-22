#!/bin/bash
set -eux

apt-get update
apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release

curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null

apt-get update
apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

usermod -aG docker ubuntu

systemctl enable docker
systemctl start docker

# Wait for Docker to start
sleep 10

# Retry mechanism for fetching the join token from the manager
MAX_RETRIES=30
RETRY_DELAY=10
COUNT=0

while [ $COUNT -lt $MAX_RETRIES ]; do
  echo "Attempt $COUNT: Fetching swarm join token from manager..."
  
  # Try to get the token from the manager using its private IP
  JOIN_TOKEN=$(ssh -o StrictHostKeyChecking=no -o ConnectTimeout=5 ubuntu@${manager_ip} 'docker swarm join-token -q worker' 2>/dev/null)
  
  # If that fails, try the public IP
  if [ -z "$JOIN_TOKEN" ]; then
    JOIN_TOKEN=$(ssh -o StrictHostKeyChecking=no -o ConnectTimeout=5 ubuntu@${manager_public_ip} 'docker swarm join-token -q worker' 2>/dev/null)
  fi
  
  # If we got a token, join the swarm
  if [ ! -z "$JOIN_TOKEN" ]; then
    echo "Successfully retrieved join token. Joining swarm..."
    docker swarm join --token $JOIN_TOKEN ${manager_ip}:2377
    exit 0
  fi
  
  echo "Failed to get join token. Retrying in $RETRY_DELAY seconds..."
  sleep $RETRY_DELAY
  COUNT=$((COUNT+1))
done

echo "Failed to join swarm after $MAX_RETRIES attempts"
