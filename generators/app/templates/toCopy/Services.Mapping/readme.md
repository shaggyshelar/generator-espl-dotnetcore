#Services.Mapping

This layer is responsible for the conversion from domain models to DTO models. The Services layer should use this 
layer for the conversion. The services layer will pass a pre-filtered query that will be further modified and 
directly mapped to the DTO classes. See the ThingMap as an example.

**Do:** Simple database read and writes. Conversion to DTO's.

**Do not:** Inject other services. Use the Workflow layer for stuff like that.