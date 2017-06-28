#.Net Core Application

##Getting Started
This solution is architected using a practical flavor of your typical N Tier/onion architectures. 
It's simple where it can be, and technical where it makes sense. I'm sure there is plenty of room for improvement, and that's what pull requests are for. ;)

In each section, you'll find readme.md files that attempt to explain the layer, it's purpose, and how to use/implement it. Also room for improvement.

Review these sites if you haven't already.
- [.NET Core SDK](https://www.microsoft.com/net/core) <-- Install the things
- [ASP.NET Core documentation](https://docs.asp.net/en/latest/intro.html) <-- Read about the things you just installed
- [Another good resource](https://docs.microsoft.com/en-us/dotnet/) --> Maybe. I didn't read any of it. Looks legit though. ;)

##This here app
In order to run the solution you just generated with that fancy ...[generator](https://github.com/Kizmar/generator-funion)... you'll need to manually set a few more things up.

###Do the .NET Core command-line (CLI) things
_Note: If you are using Visual Studio, you can pretty much skip this part. It **should** do it for you. Should._
- [Documentation](https://github.com/dotnet/cli) <-- Read about more things
- Assuming you have the right stuff installed now, you'll want to run "dotnet restore" from the root of your solution folder. In this case; run it from the very folder this file sits in.
- After that command finishes successfully (fingers crossed), run "dotnet build".
- Ok, now you have a .NET Core solution that builds and runs, right? ...well not quite... You can't run it without some super secret stuff...

###Set user secrets (technically more dotnet cli stuff)
Why bother with these silly things? User secrets allow developers to do things like: set up a local database connection without having to ignore the web.config on commit. 
Sound familiar? If not... just... do it anyway.

- [Documentation](https://docs.asp.net/en/latest/security/app-secrets.html) <-- Almost done reading about things, I promise
- WHOH! Before you go copy & pasting the commands below, make sure you "cd WebApi". The user secrets are pulled during Startup.cs there.

- This is basically what you need to run... fill in the blanks:
  - _WebApi\project.json_: [generate a GUID](https://www.guidgenerator.com/) for the "userSecretsId". This tells the app which settings belong to this app.
  - dotnet user-secrets set UserSecrets:DbConnection "Server=(localdb)\mssqllocaldb;Database=**DATABASE**;Trusted_Connection=True;MultipleActiveResultSets=true"
  - dotnet user-secrets set UserSecrets:Jwt:Audience "http://localhost:4200"
  - dotnet user-secrets set UserSecrets:Jwt:Issuer "http://localhost:5000"
  - dotnet user-secrets set UserSecrets:Emailer:Server "http://localhost"
  - dotnet user-secrets set UserSecrets:Emailer:Username "**USER**"
  - dotnet user-secrets set UserSecrets:Emailer:Password "**PASSWORD**"

Lets take a short break and talk about SMTP servers. If you just want to test stuff locally, the easiest thing I've found is [Papercut](https://papercut.codeplex.com/). 
If you want a decent SMTP server that will let you send a few emails for free every month, try [Mailjet](https://www.mailjet.com/feature/smtp-relay/). We now return you to
your regularly scheduled program...

###Create your first migration
This solution is set up to automatically run generated migrations when you start the app (check out _Data\ApplicationDbContext.cs_). 
Migrations are stored in _Data\Migrations_. Your first migration will also create a _ApplicationDbContextModelSnapshot.cs_ file. This is automatically updated when you generate migrations. 
Don't touch this file. It will get angry and you'll regret it. Any time you add a new domain model, create a migration for it.

_Completing this step is way easier with Visual Studio. If you choose to do it with the cli, good luck._ ;)

- Using Visual Studio:
  - [Read about it](https://msdn.microsoft.com/en-us/data/jj591621.aspx#generating) <-- Mostly irrelevant, but in case yer bored
  - Pull up the "Package Manager Console" from View > Other Windows
  - Change the "Default project" to "Infrastructure\Data".
  - Command: "add-migration init_database"
- Using .NET Core CLI: 
  - [Read about it](https://docs.efproject.net/en/latest/miscellaneous/cli/dotnet.html#dotnet-ef-migrations-add) <-- Probably good to review
  - Do some things ¯\\\_(ツ)\_/¯ ...figure it out. I'm not your mom. :)

###Last but not least: environment variables
There are a few environment and application settings that need attention. The new _web.config_ is _appsettings.json_. You don't really touch _web.config_ anymore. 
- Each configuration has an _appsettings.ENVIRONMENT.json_ file
- These files are all included in _project.json_. I should probably look into including only the file needed for each build, some day.
- You'll want to make sure you set the empty properties on each environment file.

This solution is set up so that the "development" configuration is used both locally, and in a CI (continuous integration) environment - assuming you have development, 
staging, and production. If _Startup.cs_ finds user secrets, it knows it's running locally, and uses the user secret values instead of those set in _appsettings.development.json_.

