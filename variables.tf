variable "region" {
  type = string
}

variable "vpc_id" {
  type = string
}

variable "cluster_arn" {
  type = string
}

variable "task_definitions_arn" {
  type = string
}

variable "subnets" {
  type = list(string)
}
