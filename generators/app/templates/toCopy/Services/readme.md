#Services

This layer is responsible for passing data back and forth to/from the database using Linq to SQL. 
There should be little to no business logic in this layer, aside from audit fields being set where 
necessary. Each service should only care about it's own model, nothing else.

**Do:** Simple database read and writes. Reference Services.Mapping classes to convert to DTO.

**Do not:** Inject other services. Use the Workflow layer when multiple services are needed to 
complete an action.