# Build
Run Update-Database
Run Drop-Database if there are any errors with Update-Database

# Pages and Roles
| **Page / Route**                                           | **Controller / View**                      | **Purpose**                                                                | **Who Can Access**                                      |
| ---------------------------------------------------------- | ------------------------------------------ | -------------------------------------------------------------------------- | ------------------------------------------------------- |
| `/` → `/Home/Index`                                        | `HomeController.Index`                     | Landing page with basic welcome text and role-aware buttons.               | Everyone                                                |
| `/Home/Privacy`                                            | `HomeController.Privacy`                   | Static privacy message for the project.                                    | Everyone                                                |
| `/Users/Register`                                          | `UsersController.Register` (GET/POST)      | Custom visitor registration page. Creates both Identity + AppUser entries. | Guests (not signed in)                                  |
| `/Identity/Account/Login`                                  | Identity scaffold page                     | Login page using ASP.NET Identity.                                         | Guests                                                  |
| `/Identity/Account/Logout`                                 | Identity scaffold page                     | Ends session and redirects to Home.                                        | Any signed-in user                                      |
| `/Order/Create`                                            | `OrderController.Create`                   | Allows customers to create new orders.                                     | **Customer**                                            |
| `/Order/List?filter=mine`                                  | `OrderController.List("mine")`             | Displays orders belonging to the signed-in customer.                       | **Customer**                                            |
| `/Order/List`                                              | `OrderController.List()`                   | Global order list used by employees and admins.                            | **Employee, Admin**                                     |
| `/Order/Details/{id}`                                      | `OrderController.Details(int id)`          | Shows full details for a specific order.                                   | **Customer (their own order)**, **Employee**, **Admin** |
| `/Users/List`                                              | `UsersController.List`                     | Displays visitors awaiting approval (“Pending Confirmation”).              | **Employee, Admin**                                     |
| `/AdminUsers/Index`                                        | `AdminUsersController.Index`               | Lists all users with current roles and a dropdown to assign new roles.     | **Admin**                                               |
| `/AdminUsers/SetRole` (POST)                               | `AdminUsersController.SetRole`             | Saves a role assignment and updates Identity + AppUser tables.             | **Admin**                                               |
| `/Notifications/NewRegistrations`                          | `NotificationsController.NewRegistrations` | Returns JSON list of newly registered “Visitor” users for the popup modal. | **Employee, Admin**                                     |
| `/Identity/Account/Manage/*`                               | (Optional scaffold)                        | Identity’s self-management pages if enabled.                               | Signed-in users                                         |
| `/Identity/Account/ForgotPassword`, `/ResetPassword`, etc. | (Optional scaffold)                        | Standard Identity account features if scaffolded.                          | Guests / users as appropriate                           |

| **Role**                  | **Description / Abilities**                                                                  |
| ------------------------- | -------------------------------------------------------------------------------------------- |
| **Visitor**               | Default for new registrations. Must be approved before promotion to Customer/Employee/Admin. |
| **Customer**              | Can create and view their own orders.                                                        |
| **Employee**              | Can view all orders, see pending visitors, and approve/notify new users.                     |
| **Admin (Grandma Smith)** | Full control — can view all data, assign roles, and manage the system.                       |

---

10-5-25 Linda
Minor fix to List.cshtml. Added @ to ViewData line and removed } at end of code.
Updated Create.cshtml for dropdown ItemName selection list. Made changes to Create.cshtml, OrderFromViewModel.cs,
and OrderController.cs GET & POST Create().

10-1-25 Linda
Fixed registration form by adding the address field.
(The address field is set to required but should've been nullable.)
Registration successful message worked!
Created DbInitializer to seed test data. Add code to run it in Program.cs.
