using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace HotelSearchApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Hotel Search!");

            // Connection string for MySQL database
            string connectionString = "server=localhost;user=root;password=localhost123;database=hotel_schema";

            // Establish connection to the database
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    bool exit = false;
                    while (!exit)
                    {
                        Console.WriteLine("\nOptions:");
                        Console.WriteLine("1. List all hotels");
                        Console.WriteLine("2. Search hotels based on options");
                        Console.WriteLine("3. Add hotel");
                        Console.WriteLine("4. Remove hotel");
                        Console.WriteLine("5. Exit");

                        Console.Write("Enter your choice: ");
                        int choice;
                        while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 5)
                        {
                            Console.WriteLine("Invalid choice. Please enter a number between 1 and 5.");
                            Console.Write("Enter your choice: ");
                        }

                        switch (choice)
                        {
                            case 1:
                                ListAllHotels(connection);
                                break;
                            case 2:
                                SearchHotels(connection);
                                break;
                            case 3:
                                AddHotel(connection);
                                break;
                            case 4:
                                RemoveHotel(connection);
                                break;
                            case 5:
                                exit = true;
                                Console.WriteLine("Exiting the program...");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        static void ListAllHotels(MySqlConnection connection)
        {
            Console.WriteLine("\nList of all hotels:");
            string query = "SELECT hotel_name FROM hotel_desc";
            MySqlCommand command = new MySqlCommand(query, connection);
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine($"- {reader.GetString("hotel_name")}");
                }
            }
        }

        static void SearchHotels(MySqlConnection connection)
        {
            Console.WriteLine("Search hotels based on options:");

            // Reusing the code for searching hotels from the previous version
            HotelSearchCriteria criteria = GetUserPreferences();

            string query = "SELECT hotel_name FROM hotel_desc WHERE";

            List<string> filters = new List<string>();
            if (criteria.VegetarianPreference)
            {
                filters.Add("hotel_cuisine = 'Veg'");
            }
            if (criteria.ServiceType != ServiceType.Any)
            {
                filters.Add($"srvc_type = '{criteria.ServiceType}'");
            }
            if (criteria.Seating > 0)
            {
                filters.Add($"seating >= {criteria.Seating}");
            }
            if (criteria.HotelRating > 0)
            {
                filters.Add($"hotel_rating >= {criteria.HotelRating}");
            }
            if (criteria.Distance > 0)
            {
                filters.Add($"distance <= {criteria.Distance}");
            }
            if (criteria.AveragePrice > 0)
            {
                filters.Add($"average_price <= {criteria.AveragePrice}");
            }

            // Combine filters into the SQL query
            if (filters.Count > 0)
            {
                query += " " + string.Join(" AND ", filters);
            }
            else
            {
                query += " 1"; // True condition to select all rows
            }

            MySqlCommand command = new MySqlCommand(query, connection);
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                Console.WriteLine("\nSearch results:");

                while (reader.Read())
                {
                    Console.WriteLine($"- {reader.GetString("hotel_name")}");
                }
            }
        }

        static void AddHotel(MySqlConnection connection)
        {
            Console.WriteLine("Add a new hotel:");

            Console.Write("Enter hotel name: ");
            string hotelName = Console.ReadLine();

            Console.Write("Enter hotel cuisine (Veg/NonVeg): ");
            string cuisine = Console.ReadLine();

            Console.Write("Enter service type (Self Service/Service): ");
            string serviceType = Console.ReadLine();

            Console.Write("Enter distance from hotel: ");
            double distance;
            while (!double.TryParse(Console.ReadLine(), out distance) || distance < 0)
            {
                Console.WriteLine("Invalid input. Please enter a non-negative number.");
                Console.Write("Enter distance from hotel: ");
            }

            Console.Write("Enter average price per person: ");
            double averagePrice;
            while (!double.TryParse(Console.ReadLine(), out averagePrice) || averagePrice < 0)
            {
                Console.WriteLine("Invalid input. Please enter a non-negative number.");
                Console.Write("Enter average price per person: ");
            }

            Console.Write("Enter hotel rating: ");
            double hotelRating;
            while (!double.TryParse(Console.ReadLine(), out hotelRating) || hotelRating < 0 || hotelRating > 5)
            {
                Console.WriteLine("Invalid input. Please enter a number between 0 and 5.");
                Console.Write("Enter hotel rating: ");
            }

            Console.Write("Enter service time (Fast/Average/Slow): ");
            string serviceTime = Console.ReadLine();

            Console.Write("Does the hotel provide seating? (Yes/No): ");
            bool hasSeating = Console.ReadLine().Equals("Yes", StringComparison.OrdinalIgnoreCase);

            // Insert the new hotel into the database
            string query = $"INSERT INTO hotel_desc (hotel_name, hotel_cuisine, srvc_type, distance, average_price, hotel_rating, srvc_time, seating) " +
                           $"VALUES ('{hotelName}', '{cuisine}', '{serviceType}', {distance}, {averagePrice}, {hotelRating}, '{serviceTime}', {(hasSeating ? 1 : 0)})";
            MySqlCommand command = new MySqlCommand(query, connection);
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                Console.WriteLine("Hotel added successfully.");
            }
            else
            {
                Console.WriteLine("Failed to add hotel.");
            }
        }

        static void RemoveHotel(MySqlConnection connection)
        {
            Console.WriteLine("Remove a hotel:");

            Console.Write("Enter the name of the hotel to remove: ");
            string hotelName = Console.ReadLine();

            // Delete the hotel from the database
            string query = $"DELETE FROM hotel_desc WHERE hotel_name = '{hotelName}'";
            MySqlCommand command = new MySqlCommand(query, connection);
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                Console.WriteLine("Hotel removed successfully.");
            }
            else
            {
                Console.WriteLine("No hotel found with the given name.");
            }
        }

        static HotelSearchCriteria GetUserPreferences()
        {
            Console.WriteLine("\nPlease provide your preferences:");

            // Get user preferences
            Console.Write("Vegetarian preference (Yes/No): ");
            bool vegetarianPreference = Console.ReadLine().Equals("Yes", StringComparison.OrdinalIgnoreCase);

            Console.Write("Service type (Self Service/Service/Any): ");
            string serviceTypeStr = Console.ReadLine();
            ServiceType serviceType = ServiceType.Any;
            if (serviceTypeStr.Equals("Self Service", StringComparison.OrdinalIgnoreCase))
            {
                serviceType = ServiceType.SelfService;
            }
            else if (serviceTypeStr.Equals("Service", StringComparison.OrdinalIgnoreCase))
            {
                serviceType = ServiceType.Service;
            }

            Console.Write("Do you want seating or no seating? (1. Yes / 2. No): ");
            int seatingChoice;
            while (!int.TryParse(Console.ReadLine(), out seatingChoice) || (seatingChoice != 1 && seatingChoice != 2))
            {
                Console.WriteLine("Invalid choice. Please enter 1 for Yes or 2 for No.");
                Console.Write("Do you want seating or no seating? (1. Yes / 2. No): ");
            }
            double seating = seatingChoice == 1 ? 1 : 0;

            Console.Write("Minimum hotel rating (Enter 0 if no preference): ");
            double hotelRating;
            while (!double.TryParse(Console.ReadLine(), out hotelRating) || hotelRating < 0 || hotelRating > 5)
            {
                Console.WriteLine("Invalid input. Please enter a number between 0 and 5.");
                Console.Write("Minimum hotel rating (Enter 0 if no preference): ");
            }

            Console.Write("Maximum distance from hotel (Enter 0 if no preference): ");
            double distance;
            while (!double.TryParse(Console.ReadLine(), out distance) || distance < 0)
            {
                Console.WriteLine("Invalid input. Please enter a non-negative number.");
                Console.Write("Maximum distance from hotel (Enter 0 if no preference): ");
            }

            Console.Write("Maximum average price per person (Enter 0 if no preference): ");
            double averagePrice;
            while (!double.TryParse(Console.ReadLine(), out averagePrice) || averagePrice < 0)
            {
                Console.WriteLine("Invalid input. Please enter a non-negative number.");
                Console.Write("Maximum average price per person (Enter 0 if no preference): ");
            }

            return new HotelSearchCriteria
            {
                VegetarianPreference = vegetarianPreference,
                ServiceType = serviceType,
                AveragePrice = averagePrice,
                Distance = distance,
                HotelRating = hotelRating,
                Seating = seating
            };
        }
    }

    // Data structure to hold hotel search criteria
    class HotelSearchCriteria
    {
        public bool VegetarianPreference { get; set; }
        public ServiceType ServiceType { get; set; }
        public double AveragePrice { get; set; }
        public double Distance { get; set; }
        public double HotelRating { get; set; }
        public double Seating { get; set; }
        // Add more criteria properties if needed
    }

    // Enum to represent service type
    enum ServiceType
    {
        SelfService,
        Service,
        Any
    }
}