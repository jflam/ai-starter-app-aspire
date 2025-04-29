var builder = DistributedApplication.CreateBuilder(args);

var petbnbdb = builder.AddPostgres("postgresql").AddDatabase("petbnbdb");

 var server = builder.AddProject<Projects.Server>("server")
                    .WaitFor(petbnbdb)
                    .WithReference(petbnbdb);

 builder.AddProject<Projects.DbMigrations>("dbmigrations")
       .WaitFor(petbnbdb)
       .WithReference(petbnbdb);

 builder.AddProject<Projects.Client>("client")
       .WaitFor(server)
       .WithReference(server);

 builder.Build().Run();
