#WebApi

**Startup.cs:** This is where all the magic happens.

**appsettings.json & appsettings.ENV.json:** It's the new web.config. ;)

##User Secrets
You'll need to use the dotnet CLI to add a few user secrets to your local development environment. [Reference docs.](https://docs.asp.net/en/latest/security/app-secrets.html)

Variables to add to user-secrets (using "dotnet user-secrets set VARIABLE_NAME VARIABLE_VALUE"):
* UserSecrets:DbConnection
* UserSecrets:Jwt:Issuer
* UserSecrets:Jwt:Audience
* UserSecrets:Emailer:Server
* UserSecrets:Emailer:Username
* UserSecrets:Emailer:Password

Example commands and values:
* dotnet user-secrets set UserSecrets:DbConnection "Server=(localdb)\mssqllocaldb;Database=MyDatabase;Trusted_Connection=True;MultipleActiveResultSets=true"
* dotnet user-secrets set UserSecrets:Jwt:Audience "http://localhost:4200"
* dotnet user-secrets set UserSecrets:Jwt:Issuer "http://localhost:5000"
* dotnet user-secrets set UserSecrets:Emailer:Server "http://localhost"
* dotnet user-secrets set UserSecrets:Emailer:Username "someUser"
* dotnet user-secrets set UserSecrets:Emailer:Password "somePassword"

See Startup.cs comments for more information on how they are used and examples.

##Sections

###Controllers
Should all be model-based, and very simple. At most, a controller action should only call a log and a service method.

###Middleware
**Logging:** An implementation of Entity Framework logging middleware.

**Tokens:** An implementation of JWT token generator/provider middleware. The "api/token" endpoint is in here too.

###Models
These are input models specific to authentication / account related actions.