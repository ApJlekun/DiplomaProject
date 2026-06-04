using BarInventoryApp.DataContexts;
using BarInventoryApp.Pages;
using BarInventoryApp.Services;
using BarInventoryApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;

namespace BarInventoryApp
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(basePath);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<MainViewModel>();

                    services.AddTransient<AuthorizationPage>();
                    services.AddTransient<AuthorizationViewModel>();

                    services.AddTransient<IngredientsPage>();
                    services.AddTransient<IngredientsViewModel>();

                    services.AddTransient<OrdersPage>();
                    services.AddTransient<OrdersViewModel>();

                    services.AddTransient<UserPage>();
                    services.AddTransient<UsersViewModel>();

                    services.AddTransient<ManagerDashboardPage>();
                    services.AddTransient<AdminDashboardPage>();

                    services.AddTransient<CocktailsPage>();
                    services.AddTransient<CocktailsViewModel>();

                    services.AddScoped<AuthService>();
                    services.AddScoped<IngredientService>();
                    services.AddScoped<InvoiceService>();
                    services.AddScoped<UserService>();
                    services.AddScoped<ExcelExportService>();
                    services.AddScoped<ExcelImportService>();
                    services.AddScoped<ReceiptService>();
                    services.AddScoped<CocktailService>();
                    services.AddScoped<EmailService>();
                    services.AddScoped<RevisionService>();

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));
                })
                .Build();

            ServiceProvider = host.Services;

            try
            {
                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска приложения: {ex.Message}\n\n{ex.InnerException?.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
