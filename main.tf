terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.16"
    }
  }

  required_version = ">= 1.2.0"
}

provider "aws" {
  region = var.region
}

resource "aws_ecs_service" "scrum-poker-api-ecs-service" {
  name            = "scrum-poker-api-ecs-service"
  cluster         = var.cluster_arn
  task_definition = var.task_definitions_arn
  desired_count   = 1

  network_configuration {
    security_groups  = ["${aws_security_group.ecs_service_sg.id}"]
    subnets          = var.subnets
    assign_public_ip = false
  }

  load_balancer {
    target_group_arn = aws_alb_target_group.ecs_alb_target_group.arn
    container_name   = "scrum-poker-api"
    container_port   = 5001
  }
}

resource "aws_security_group" "ecs_service_sg" {
  name   = "scrum-poker-api-ecs-service-sg"
  ingress {
    from_port        = 5001
    to_port          = 5001
    protocol         = "tcp"
    cidr_blocks      = ["0.0.0.0/0"]
    ipv6_cidr_blocks = ["::/0"]
    security_groups = [aws_security_group.alb_sg.id]
  }
  egress {
    from_port        = 0
    to_port          = 0
    protocol         = "-1"
    cidr_blocks      = ["0.0.0.0/0"]
    ipv6_cidr_blocks = ["::/0"]
  }
}

resource "aws_lb" "ecs_alb" {
  name               = "scrum-poker-api-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb_sg.id]
  subnets            = var.subnets
  enable_deletion_protection = false
}

resource "aws_security_group" "alb_sg" {
  name   = "scrum-poker-api-alb-sg"
  ingress {
    from_port        = 80
    to_port          = 80
    protocol         = "tcp"
    cidr_blocks      = ["0.0.0.0/0"]
    ipv6_cidr_blocks = ["::/0"]
  }
  egress {
    from_port        = 0
    to_port          = 0
    protocol         = "-1"
    cidr_blocks      = ["0.0.0.0/0"]
    ipv6_cidr_blocks = ["::/0"]
  }
}

resource "aws_alb_target_group" "ecs_alb_target_group" {
  name        = "scrum-poker-api-tg"
  vpc_id      = var.vpc_id
  port        = 5001
  protocol    = "HTTP"
  target_type = "ip"
  health_check {
    healthy_threshold   = "3"
    interval            = "30"
    protocol            = "HTTP"
    matcher             = "200"
    timeout             = "3"
    unhealthy_threshold = "2"
  }
}

resource "aws_alb_listener" "http" {
  load_balancer_arn = aws_lb.ecs_alb.id
  port              = 80
  protocol          = "HTTP"
  default_action {
    target_group_arn = aws_alb_target_group.ecs_alb_target_group.arn
    type             = "forward"
  }
}
