# .NET 6 MVC User Authentication System

This project showcases a robust user authentication system using the .NET 6 MVC framework. The main aim of this project is to provide a seamless experience for users to register, log in, update their passwords and account details, or even delete their account if they wish.

## Features:

1. **User Registration**: Users can sign up and receive a confirmation email.
2. **Login**: Existing users can log in with their credentials.
3. **Account Management**: 
    - Password Update
    - Update personal details
    - Delete account option
4. **Email Notifications**:
    - Confirmation email on registration
    - Daily reminder emails to users

## Technologies Used:

- **.NET 6 MVC**: For building the web application and handling server-side logic.
- **Somee Free Remote Database**: As the primary datastore.
- **Redis**: For storing account details with quick retrieval.
- **RabbitMQ**: For managing the email notifications in an asynchronous manner.
- **Hangfire**: Scheduled tasks to send daily reminder emails.

## Getting Started:

### Prerequisites:

- [.NET SDK 6.0](https://dotnet.microsoft.com/download/dotnet/6.0)
- Redis server
- RabbitMQ server
- A configured SMTP server for email notifications
- Somee database credentials

### Installation:

1. Clone this repository:
   \```bash
   git clone [https://github.com/bilalcinal/AuthenticationMVC]
   \```
2. Navigate to the project directory:
   \```bash
   cd [AuthenticationMVC]
   \```
3. Update the `appsettings.json` file with your Redis, RabbitMQ, SMTP server, and Somee database credentials.
4. Run the application:
   \```bash
   dotnet run
   \```

Visit `http://localhost:5000/` in your browser to access the application.

## Contributing:

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License:

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
