#Api Controllers

This layer is responsible for all communication from the web front-end to the api back-end. A controller should only 
inject a single service and a single workflow (focused responsibility). Controllers should contain no logic. Methods should be a 
one line call to a serice or workflow method.

When returning relational data; let the parameter(s) you are passing be a guide. Example: If you want a list 
of related models based on Thing.Id, the method for that should be on ThingController as 
"public IActionResult GetWidgets(long id)", with a route of "api/thing/{id}/widgets".

**Do:** Keep it simple. Use a parent model's controller to return a list of child elements specific to the parent's Id.

**Do not:** Inject more then one service and one workflow.