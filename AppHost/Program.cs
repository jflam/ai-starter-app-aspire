var builder = DistributedApplication.CreateBuilder(args);

var postgresdb = builder.AddPostgres("postgresql").AddDatabase("fortunesdb");

var server = builder.AddProject<Projects.Server>("server")
                    .WaitFor(postgresdb)
                    .WithReference(postgresdb);

builder.AddProject<Projects.DbMigrations>("dbmigrations")
       .WaitFor(postgresdb)
       .WithReference(postgresdb);

builder.AddProject<Projects.Client>("client")
       .WithExternalHttpEndpoints()
       .WaitFor(postgresdb)
       .WithReference(server);

builder.Build().Run();
