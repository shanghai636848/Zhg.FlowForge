using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Services;

public interface IDeploymentService
{
    Task<DeploymentResult> DeployAsync(DeploymentRequest request, IProgress<string>? progress = null);
    Task<List<DeploymentHistory>> GetDeploymentHistoryAsync(string projectId);
    Task<DeploymentStatus> GetDeploymentStatusAsync(string deploymentId);
    Task RollbackAsync(string deploymentId);
}

public class DeploymentConfiguration
{
    public string Target { get; set; } = "";
}

public class EnvironmentConfiguration
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "production";
    public int CpuLimit { get; set; } = 1000;
    public int MemoryLimit { get; set; } = 512;
    public Dictionary<string, string> Variables { get; set; } = new();
    public bool EnableHealthCheck { get; set; } = true;
    public string HealthCheckPath { get; set; } = "/health";
    public int HealthCheckInterval { get; set; } = 30;
    public int HealthCheckTimeout { get; set; } = 5;
    public string LogLevel { get; set; } = "Information";
    public bool EnableStructuredLogging { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
}

public class ContainerConfiguration
{
    public string ImageName { get; set; } = "";
    public string ImageTag { get; set; } = "latest";
    public string Registry { get; set; } = "docker-hub";
    public bool AlwaysPullImage { get; set; } = true;
    public string ContainerName { get; set; } = "";
    public int Replicas { get; set; } = 1;
    public string RestartPolicy { get; set; } = "always";
    public bool Privileged { get; set; } = false;
    public List<PortMapping> PortMappings { get; set; } = new();
    public List<VolumeMount> Volumes { get; set; } = new();
    public double CpuLimit { get; set; } = 1.0;
    public int MemoryLimit { get; set; } = 512;
}

public class PortMapping
{
    public int ContainerPort { get; set; }
    public int HostPort { get; set; }
    public string Protocol { get; set; } = "TCP";
}

public class VolumeMount
{
    public string HostPath { get; set; } = "";
    public string ContainerPath { get; set; } = "";
    public bool ReadOnly { get; set; }
}

public class NetworkConfiguration
{
    public string Domain { get; set; } = "";
    public bool EnableHttps { get; set; } = true;
    public string SslSource { get; set; } = "letsencrypt";
    public string SslCertificate { get; set; } = "";
    public string SslPrivateKey { get; set; } = "";
    public bool ForceHttps { get; set; } = true;
    public bool EnableLoadBalancer { get; set; } = true;
    public string LoadBalancingAlgorithm { get; set; } = "round-robin";
    public bool EnableSessionAffinity { get; set; } = false;
    public int HealthCheckInterval { get; set; } = 30;
    public int UnhealthyThreshold { get; set; } = 3;
    public bool EnableCors { get; set; } = false;
    public string CorsOrigins { get; set; } = "";
    public string CorsMethods { get; set; } = "GET, POST, PUT, DELETE";
    public bool CorsAllowCredentials { get; set; } = false;
    public bool EnableRateLimit { get; set; } = false;
    public int RateLimit { get; set; } = 100;
    public bool EnableWaf { get; set; } = false;
    public bool EnableDdosProtection { get; set; } = false;
}

public class DeploymentRequest
{
    public string ProjectId { get; set; } = "";
    public DeploymentConfiguration Configuration { get; set; } = new();
    public EnvironmentConfiguration Environment { get; set; } = new();
    public ContainerConfiguration Container { get; set; } = new();
    public NetworkConfiguration Network { get; set; } = new();
}

public class DeploymentResult
{
    public bool Success { get; set; }
    public string Error { get; set; } = "";
    public string DeploymentId { get; set; } = "";
    public string ApplicationUrl { get; set; } = "";
    public DateTime DeployedAt { get; set; } = DateTime.Now;
}

public class DeploymentHistory
{
    public string Id { get; set; } = "";
    public DateTime DeployedAt { get; set; }
    public string Environment { get; set; } = "";
    public bool Success { get; set; }
    public string Version { get; set; } = "";
}

public class DeploymentStatus
{
    public string Status { get; set; } = "";
    public int HealthyInstances { get; set; }
    public int TotalInstances { get; set; }
}