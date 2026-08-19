variable "workload_name" {
  description = "Short lowercase workload name used in resource names."
  type        = string
  default     = "orders"

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{2,18}$", var.workload_name))
    error_message = "workload_name must be 3-19 lowercase letters, digits, or hyphens."
  }
}

variable "environment" {
  description = "Deployment environment."
  type        = string
  default     = "dev"

  validation {
    condition     = contains(["dev", "test", "prod"], var.environment)
    error_message = "environment must be dev, test, or prod."
  }
}

variable "location" {
  description = "Azure region for all resources."
  type        = string
  default     = "eastus"
}

variable "tags" {
  description = "Additional governance tags."
  type        = map(string)
  default     = {}
}
