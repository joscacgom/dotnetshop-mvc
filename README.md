# dotnetshop-mvc
dotnetshop is an e-commerce application developed with ASP.NET Core, dedicated to the sale of books. It offers a comprehensive platform for users to purchase their favourite ones. The application includes features such as user authentication, a shopping cart system, order processing and so on.

# Features
- User Authentication and Authorization: The application has well-implemented user authentication and authorization feature, a variety of roles for access control and Facebook login/register.

- Catalog: On the home page, users can easily view and access detailed information about each book.

- Shopping Cart: Users can get full shopping experience with the convenient shopping cart functionality.

- Stripe Payment Integration: The app integrates the Stripe payment system, enhancing the security and efficiency of the purchase process.

## Installation

```bash
# Example installation steps
git clone https://github.com/username/repository.git
cd repository
dotnet restore
dotnet build
```
## Start

- Create appsettings.json copy-pasting appsettings.Developtment.json example.

- Create your own api keys for such services: Stripe and Facebook login.

- Create your own database and replace ConnectionString in appsettings.json

- Apply migrations and update database:
```bash
dotnet ef migrations add <name>
dotnet ef database update
```
- Default running command:
```bash
dotnet run
```
- Including hot reload:
```bash
dotnet watch run
```
