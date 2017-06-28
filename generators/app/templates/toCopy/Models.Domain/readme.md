#Models.Domain

This layer contains POCO's that can be mapped directly to the database. Non-conventional EF mapping should 
be handled in the Data layer in the DbContext class (not attributes).

**Do:** Make sure you create a Data.Migration after adding a new model. The ApplicationDbContext is set up
to update the database when the app is run.

**Do not:** Reference any solution projects in this project! It should be very clean and simple.