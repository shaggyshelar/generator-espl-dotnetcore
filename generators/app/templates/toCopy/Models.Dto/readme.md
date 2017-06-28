#Models.Dto

Like domain models, this layer consists of POCO's only. These are the transfer objects used when 
returning a domain model is either too simple, or too detailed. Class names should always be upper 
camel case and end with "Dto". Not all domain models will need DTO's.

Conversion from a domain model to a DTO happens in the Services.Mapping layer. See Services.Mapping.ThingMap 
for an example.

**Do:** Determine cases were we need DTO's - this is not an all encompassing layer.

**Do not:** Reference any solution projects other then Models.Domain in this project. Add mapping to the 
constructors, leave that all up to the Services.Mapping layer.