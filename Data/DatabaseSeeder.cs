using CarRentalSystem.Models;
using CarRentalSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<CarRentalDbContext>();
                var authService = services.GetRequiredService<IAuthService>();

                // Apply migrations if they're not applied yet
                await context.Database.MigrateAsync();

                // Seed data if the database is empty
                await SeedCategories(context);
                await SeedUsers(context, authService);
                await SeedVehicles(context);
                await SeedBookings(context);
                await SeedReviews(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private static async Task SeedCategories(CarRentalDbContext context)
        {
            if (await context.VehicleCategories.AnyAsync())
                return;

            var categories = new List<VehicleCategory>
            {
                new VehicleCategory { Name = "Economy", Description = "Fuel-efficient, compact cars ideal for urban driving and budget-conscious travelers." },
                new VehicleCategory { Name = "Sedan", Description = "Comfortable midsize cars with good trunk space and balanced features." },
                new VehicleCategory { Name = "SUV", Description = "Spacious vehicles with higher ground clearance, perfect for families and outdoor adventures." },
                new VehicleCategory { Name = "Luxury", Description = "Premium vehicles with top-of-the-line features and exceptional comfort." },
                new VehicleCategory { Name = "Sports", Description = "High-performance vehicles designed for speed and handling." }
            };

            await context.VehicleCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        private static async Task SeedUsers(CarRentalDbContext context, IAuthService authService)
        {
            if (await context.Users.AnyAsync())
                return;

            var adminUser = new User
            {
                Email = "admin@example.com",
                PasswordHash = authService.HashPassword("Admin123!"),
                FirstName = "Admin",
                LastName = "User",
                PhoneNumber = "555-123-4567",
                Address = "123 Admin St, Admin City, AC 12345",
                Role = UserRole.Administrator,
                RegistrationDate = DateTime.UtcNow,
                IsEmailVerified = true
            };

            var managerUser = new User
            {
                Email = "manager@example.com",
                PasswordHash = authService.HashPassword("Manager123!"),
                FirstName = "Manager",
                LastName = "User",
                PhoneNumber = "555-234-5678",
                Address = "456 Manager St, Manager City, MC 23456",
                Role = UserRole.Manager,
                RegistrationDate = DateTime.UtcNow,
                IsEmailVerified = true
            };

            var customerUser = new User
            {
                Email = "customer@example.com",
                PasswordHash = authService.HashPassword("Customer123!"),
                FirstName = "Customer",
                LastName = "User",
                PhoneNumber = "555-345-6789",
                Address = "789 Customer St, Customer City, CC 34567",
                Role = UserRole.Customer,
                RegistrationDate = DateTime.UtcNow,
                IsEmailVerified = true
            };

            await context.Users.AddRangeAsync(adminUser, managerUser, customerUser);
            await context.SaveChangesAsync();

            var users = await context.Users.ToListAsync();
            foreach (var user in users)
            {
                var loyaltyProgram = new LoyaltyProgram
                {
                    UserId = user.Id,
                    Points = 0,
                    Tier = "Bronze",
                    LastUpdated = DateTime.UtcNow
                };

                await context.LoyaltyPrograms.AddAsync(loyaltyProgram);
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedVehicles(CarRentalDbContext context)
        {
            if (await context.Vehicles.AnyAsync())
                return;

            var categories = await context.VehicleCategories.ToListAsync();

            // Economy Vehicles
            var economyCategory = categories.First(c => c.Name == "Economy");
            var economyVehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    Make = "Toyota",
                    Model = "Yaris",
                    Year = 2022,
                    LicensePlate = "ECO-101",
                    VIN = "1HGCM82633A123456",
                    Color = "Blue",
                    Mileage = 5000,
                    DailyRate = 45.00m,
                    ImageUrl = "https://hips.hearstapps.com/hmg-prod/images/2020-toyota-yaris-104-1587498075.jpg",
                    Description = "Compact and fuel-efficient. Perfect for city driving.",
                    Seats = 5,
                    Transmission = "Automatic",
                    FuelType = "Gasoline",
                    Status = VehicleStatus.Available,
                    CategoryId = economyCategory.Id
                },
                new Vehicle
                {
                    Make = "Honda",
                    Model = "Fit",
                    Year = 2021,
                    LicensePlate = "ECO-102",
                    VIN = "2HGFG1B33D1234567",
                    Color = "Red",
                    Mileage = 8000,
                    DailyRate = 48.00m,
                    ImageUrl = "https://www.cnet.com/a/img/resize/f970bb01b111c14e98d648df6a5e3ca17a233d3e/hub/2018/05/23/7173ead9-6e73-43e9-81b6-7e2cf165bc18/2018-honda-fit-promo.jpg?auto=webp&fit=crop&height=675&width=1200",
                    Description = "Spacious interior despite its compact size.",
                    Seats = 5,
                    Transmission = "Automatic",
                    FuelType = "Gasoline",
                    Status = VehicleStatus.Available,
                    CategoryId = economyCategory.Id
                }
            };

            // Sedan Vehicles
            var sedanCategory = categories.First(c => c.Name == "Sedan");
            var sedanVehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    Make = "Toyota",
                    Model = "Camry",
                    Year = 2022,
                    LicensePlate = "SED-101",
                    VIN = "4T1BF1FK3CU123456",
                    Color = "Silver",
                    Mileage = 10000,
                    DailyRate = 65.00m,
                    ImageUrl = "https://hips.hearstapps.com/hmg-prod/images/2025-toyota-camry-xse-awd-123-66993cc94cc40.jpg",
                    Description = "Reliable midsize sedan with excellent comfort.",
                    Seats = 5,
                    Transmission = "Automatic",
                    FuelType = "Gasoline",
                    Status = VehicleStatus.Available,
                    CategoryId = sedanCategory.Id
                },
                new Vehicle
                {
                    Make = "Honda",
                    Model = "Accord",
                    Year = 2023,
                    LicensePlate = "SED-102",
                    VIN = "1HGCV1F13MA123456",
                    Color = "Black",
                    Mileage = 7500,
                    DailyRate = 68.00m,
                    ImageUrl = "https://static0.carbuzzimages.com/wordpress/wp-content/uploads/2025/02/honda-accord-facelift-china-03.jpg",
                    Description = "Sophisticated sedan with advanced safety features.",
                    Seats = 5,
                    Transmission = "Automatic",
                    FuelType = "Gasoline",
                    Status = VehicleStatus.Available,
                    CategoryId = sedanCategory.Id
                }
            };

            // SUV Vehicles
            var suvCategory = categories.First(c => c.Name == "SUV");
            var suvVehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    Make = "Toyota",
                    Model = "RAV4",
                    Year = 2022,
                    LicensePlate = "SUV-101",
                    VIN = "JTMWFREV3MD123456",
                    Color = "Green",
                    Mileage = 15000,
                    DailyRate = 75.00m,
                    ImageUrl = "https://7cars.com.ua/wp-content/uploads/2019/03/DSC_0499-1.jpg",
                    Description = "Compact SUV with ample cargo space and great fuel economy.",
                    Seats = 5,
                    Transmission = "Automatic",
                    FuelType = "Gasoline",
                    Status = VehicleStatus.Available,
                    CategoryId = suvCategory.Id
                },
                new Vehicle
                {
                    Make = "Honda",
                    Model = "CR-V",
                    Year = 2023,
                    LicensePlate = "SUV-102",
                    VIN = "7FARS5H53PE123456",
                    Color = "Blue",
                    Mileage = 12000,
                    DailyRate = 78.00m,
                    ImageUrl = "https://hips.hearstapps.com/hmg-prod/images/2025-honda-cr-v-hybrid-awd-sport-touring-102-679407cb80051.jpg",
                    Description = "Versatile SUV with a comfortable ride and spacious interior.",
                    Seats = 5,
                    Transmission = "Automatic",
                    FuelType = "Gasoline",
                    Status = VehicleStatus.Available,
                    CategoryId = suvCategory.Id
                }
            };

            // Luxury Vehicles
            var luxuryCategory = categories.First(c => c.Name == "Luxury");
            var luxuryVehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    Make = "BMW",
                    Model = "5 Series",
                    Year = 2023,
                    LicensePlate = "LUX-101",
                    VIN = "WBAJA5C56DC123456",
                    Color = "Black",
                    Mileage = 8000,
                    DailyRate = 120.00m,
                    ImageUrl = "https://www.domkrat.by/upload/img_catalog/5-series-sedan/BMW_5_Sedan_G60_12.jpg",
                    Description = "Luxurious sedan with cutting-edge technology and premium comfort.",
                    Seats = 5,
                    Transmission = "Automatic",
                    FuelType = "Gasoline",
                    Status = VehicleStatus.Available,
                    CategoryId = luxuryCategory.Id
                },
                new Vehicle
                {
                    Make = "Mercedes-Benz",
                    Model = "E-Class",
                    Year = 2022,
                    LicensePlate = "LUX-102",
                    VIN = "WDDZF4JB5HA123456",
                    Color = "Silver",
                    Mileage = 10000,
                    DailyRate = 125.00m,
                    ImageUrl = "https://autoimage.capitalone.com/cms/Auto/assets/images/3147-hero-2024-mercedes-eclass-review.jpg",
                    Description = "Elegant luxury vehicle with superior craftsmanship.",
                    Seats = 5,
                    Transmission = "Automatic",
                    FuelType = "Gasoline",
                    Status = VehicleStatus.Available,
                    CategoryId = luxuryCategory.Id
                }
            };

            // Sports Vehicles
            var sportsCategory = categories.First(c => c.Name == "Sports");
            var sportsVehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    Make = "Ford",
                    Model = "Mustang",
                    Year = 2022,
                    LicensePlate = "SPT-101",
                    VIN = "1FA6P8TH5M5123456",
                    Color = "Red",
                    Mileage = 5000,
                    DailyRate = 110.00m,
                    ImageUrl = "https://lh3.googleusercontent.com/proxy/cH6Pm9V3OfuQk305l-ogW_dWOBAHkUuRwym9XZtkxc9j0xRh3qrh9Hrq7ke44GjtFmMuRqpg_gBDTEFPQWUtLKvx_SQ9En73kNoLJJFBCq9U-RxyZ28peuSgLPZEpqVYXxcIXmLycnkHMCsmzic0MA",
                    Description = "Iconic American muscle car with powerful performance.",
                    Seats = 4,
                    Transmission = "Automatic",
                    FuelType = "Gasoline",
                    Status = VehicleStatus.Available,
                    CategoryId = sportsCategory.Id
                },
                new Vehicle
                {
                    Make = "Chevrolet",
                    Model = "Camaro",
                    Year = 2023,
                    LicensePlate = "SPT-102",
                    VIN = "1G1FB1RS1K0123456",
                    Color = "Yellow",
                    Mileage = 3000,
                    DailyRate = 115.00m,
                    ImageUrl = "https://www.automoli.com/common/vehicles/_assets/img/gallery/f62/chevrolet-camaro-vi-facelift-2018.jpg",
                    Description = "Bold sports car with aggressive styling and thrilling performance.",
                    Seats = 4,
                    Transmission = "Automatic",
                    FuelType = "Gasoline",
                    Status = VehicleStatus.Available,
                    CategoryId = sportsCategory.Id
                }
            };

            var allVehicles = new List<Vehicle>();
            allVehicles.AddRange(economyVehicles);
            allVehicles.AddRange(sedanVehicles);
            allVehicles.AddRange(suvVehicles);
            allVehicles.AddRange(luxuryVehicles);
            allVehicles.AddRange(sportsVehicles);

            await context.Vehicles.AddRangeAsync(allVehicles);
            await context.SaveChangesAsync();
        }

        private static async Task SeedBookings(CarRentalDbContext context)
        {
            if (await context.Bookings.AnyAsync())
                return;

            var customers = await context.Users.Where(u => u.Role == UserRole.Customer).ToListAsync();
            var vehicles = await context.Vehicles.ToListAsync();

            if (!customers.Any() || !vehicles.Any())
                return;

            var customerUser = customers.First();

            // Create some sample bookings
            var bookings = new List<Booking>
            {
                new Booking
                {
                    UserId = customerUser.Id,
                    VehicleId = vehicles[0].Id,
                    BookingDate = DateTime.UtcNow.AddDays(-30),
                    PickupDate = DateTime.UtcNow.AddDays(-20),
                    ReturnDate = DateTime.UtcNow.AddDays(-15),
                    TotalPrice = 5 * vehicles[0].DailyRate,
                    Status = BookingStatus.Completed,
                    PickupLocation = "Airport Terminal 1",
                    ReturnLocation = "Airport Terminal 1",
                    PaymentMethod = PaymentMethod.PayNow,
                    PaymentStatus = PaymentStatus.Completed,
                    Notes = "First booking test"
                },
                new Booking
                {
                    UserId = customerUser.Id,
                    VehicleId = vehicles[2].Id,
                    BookingDate = DateTime.UtcNow.AddDays(-10),
                    PickupDate = DateTime.UtcNow.AddDays(5),
                    ReturnDate = DateTime.UtcNow.AddDays(10),
                    TotalPrice = 5 * vehicles[2].DailyRate,
                    Status = BookingStatus.Approved,
                    PickupLocation = "Downtown Office",
                    ReturnLocation = "Downtown Office",
                    PaymentMethod = PaymentMethod.PayAtPickup,
                    PaymentStatus = PaymentStatus.Pending,
                    Notes = "Please have a child seat available"
                },
                new Booking
                {
                    UserId = customerUser.Id,
                    VehicleId = vehicles[4].Id,
                    BookingDate = DateTime.UtcNow.AddDays(-2),
                    PickupDate = DateTime.UtcNow.AddDays(15),
                    ReturnDate = DateTime.UtcNow.AddDays(20),
                    TotalPrice = 5 * vehicles[4].DailyRate,
                    Status = BookingStatus.Requested,
                    PickupLocation = "Airport Terminal 2",
                    ReturnLocation = "Downtown Office",
                    PaymentMethod = PaymentMethod.PayNow,
                    PaymentStatus = PaymentStatus.Pending,
                    Notes = "Looking forward to trying this luxury car!"
                }
            };

            await context.Bookings.AddRangeAsync(bookings);
            await context.SaveChangesAsync();

            var completedBooking = await context.Bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .FirstOrDefaultAsync();

            if (completedBooking != null)
            {
                var loyaltyProgram = await context.LoyaltyPrograms
                    .FirstOrDefaultAsync(lp => lp.UserId == completedBooking.UserId);

                if (loyaltyProgram != null)
                {
                    int days = (int)(completedBooking.ReturnDate - completedBooking.PickupDate).TotalDays;
                    int points = days * 10; // 10 points per day

                    loyaltyProgram.Points += points;
                    loyaltyProgram.LastUpdated = DateTime.UtcNow;

                    var transaction = new LoyaltyTransaction
                    {
                        LoyaltyProgramId = loyaltyProgram.Id,
                        Points = points,
                        TransactionType = "Earned",
                        Description = $"Booking #{completedBooking.Id} - {days} days rental",
                        TransactionDate = DateTime.UtcNow
                    };

                    await context.LoyaltyTransactions.AddAsync(transaction);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedReviews(CarRentalDbContext context)
        {
            if (await context.Reviews.AnyAsync())
                return;

            var completedBookings = await context.Bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .ToListAsync();

            if (!completedBookings.Any())
                return;

            var reviews = new List<Review>();

            foreach (var booking in completedBookings)
            {
                reviews.Add(new Review
                {
                    UserId = booking.UserId,
                    VehicleId = booking.VehicleId,
                    Rating = 5, // 5-star rating
                    Comment = "Fantastic experience! The car was clean, well-maintained, and performed perfectly. The pickup and return process was very smooth. Will definitely rent again!",
                    ReviewDate = DateTime.UtcNow.AddDays(-12)
                });
            }

            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();
        }
    }
}