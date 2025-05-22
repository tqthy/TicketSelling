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

SWARM_IP=$(curl http://169.254.169.254/latest/meta-data/local-ipv4)
docker swarm init --advertise-addr $SWARM_IP || true

# Output join command to file for fetch by remote-exec
docker swarm join-token -q worker > /tmp/swarm_worker_token.txt
