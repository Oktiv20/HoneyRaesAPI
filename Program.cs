using HoneyRaesAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// CUSTOMER LIST
List<Customer> customers = new List<Customer>
{
    new Customer()
    {
        Id = 1,
        Name = "Mr. Toad",
        Address = "123 Nowhere Road"
    },
    new Customer()
    {
        Id = 2,
        Name = "John Blanket",
        Address = "3563 Container Drive"
    },
    new Customer()
    {
        Id = 3,
        Name = "David Guitar",
        Address = "210 Music Lane"
    }
};


// EMPLOYEE LIST
List<Employee> employees = new List<Employee>
{
    new Employee()
    {
        Id = 1,
        Name = "Sarah Johnson",
        Specialty = "Software Development",
    },
    new Employee()
    {
        Id = 2,
        Name = "Michael Smith",
        Specialty = "Graphic Design",
    },
    new Employee()
    {
        Id = 3,
        Name = "Emily Williams",
        Specialty = "Marketing and Sales"
    }
};


// SERVICE TICKET LIST
List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket()
    {
        Id = 1,
        CustomerId = 1,
        EmployeeId = null,
        Description = "Issue with network connectivity",
        Emergency = false,
        DateCompleted = DateTime.Now.AddDays(-3)
    },
    new ServiceTicket()
    {
        Id = 2,
        CustomerId = 3,
        EmployeeId = 2,
        Description = "Server maintenance",
        Emergency = false,
        DateCompleted = DateTime.Now.AddDays(-2)
    },
    new ServiceTicket()
    {
        Id = 3,
        CustomerId = 2,
        EmployeeId = 1,
        Description = "Hardware replacement",
        Emergency = true,
        DateCompleted = DateTime.Now.AddDays(-1)
    },
    new ServiceTicket()
    {
        Id = 4,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "Software installation",
        Emergency = false,
        DateCompleted = null
    },
    new ServiceTicket()
    {
        Id = 5,
        CustomerId = 2,
        EmployeeId = 3,
        Description = "Printer setup",
        Emergency = false,
        DateCompleted = DateTime.Now
    },

};

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapGet("/customers", () =>
{
    return customers;
});


app.MapGet("/employees", () =>
{
    return employees;
});


app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});


app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});


app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(customer);
});


app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServicesTickets = serviceTickets.Where(servticket => servticket.EmployeeId == id).ToList();
    return Results.Ok(employee);
});


app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(servticket => servticket.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    return Results.Ok(serviceTicket);
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

