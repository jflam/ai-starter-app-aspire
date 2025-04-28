var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Server>("server");

builder.AddProject<Projects.Client>("client");

builder.Build().Run();
