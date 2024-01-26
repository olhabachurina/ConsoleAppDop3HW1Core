using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Diagnostics;
using System;
using System.IO;
namespace ConsoleAppDop3HW1Core
{
    class Program
    {
        static void Main()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            var options = optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection")).Options;

            using (ApplicationContext db = new ApplicationContext(options))
            {
                // Добавляем студентов
                var students = new List<Student>
                {
                    new Student { FirstName = "Anna", LastName = "Gogylenko" },
                    new Student { FirstName = "Ivan", LastName = "Petrov" },
                    new Student { FirstName = "Olga", LastName = "Didenko" },
                    new Student { FirstName = "Petr", LastName = "Petrenko" },
                    new Student { FirstName = "Maria", LastName = "Ivanenko" },
                };

                db.Students.AddRange(students);
                db.SaveChanges();
            }

            using (ApplicationContext db = new ApplicationContext(options))
            {
                // Создаем группы
                var group1 = new Group { Name = "ПВ211" };
                var group2 = new Group { Name = "ПВ212" };
                db.Groups.AddRange(group1, group2);
                db.SaveChanges();

                // Добавляем студентов в группы
                var students = db.Students.ToList();
                group1.Students.AddRange(students.GetRange(0, 2));
                group2.Students.AddRange(students.GetRange(2, 3));
                db.SaveChanges();
            }

            using (ApplicationContext db = new ApplicationContext(options))
            {
                var student = db.Students.FirstOrDefault();

                if (student != null)
                {
                    var studentGroups = db.Students
                        .Where(s => s.Id == student.Id)
                        .SelectMany(s => s.Groups)
                        .ToList();

                    Console.WriteLine("Student Groups:");
                    foreach (var studentGroup in studentGroups)
                    {
                        Console.WriteLine($"Group Id: {studentGroup.Id}, Group Name: {studentGroup.Name}");
                    }
                }
                else
                {
                    Console.WriteLine("No students found.");
                }
            }
        }
    }

    public class ApplicationContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Group> Groups { get; set; }

        public ApplicationContext()
        {
        }

        public ApplicationContext(DbContextOptions options) : base(options)
        {
            
            Database.EnsureCreated();
        }
    }

    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public List<Group> Groups { get; set; } = new List<Group>();
    }

    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
    }
}