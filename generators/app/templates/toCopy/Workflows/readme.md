#Workflows

This layer should house all of the data related business logic. Anything outside of 
simple CRUD methods should be happening in the workflow layer. See the "ThingWorkflow" for a simple example.

**Do:** Inject multiple services if needed.

**Do not:** Inject other workflows.