








10-5-25 Linda
Minor fix to List.cshtml. Added @ to ViewData line and removed } at end of code.
Updated Create.cshtml for dropdown ItemName selection list. Made changes to Create.cshtml, OrderFromViewModel.cs,
and OrderController.cs GET & POST Create().

10-1-25 Linda
Fixed registration form by adding the address field.
(The address field is set to required but should've been nullable.)
Registration successful message worked!
Created DbInitializer to seed test data. Add code to run it in Program.cs.