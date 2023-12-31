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
        EmployeeId = 2,
        Description = "Issue with network connectivity",
        Emergency = true,
        DateCompleted = DateTime.Now.AddYears(-1)
    },
    new ServiceTicket()
    {
        Id = 2,
        CustomerId = 3,
        EmployeeId = null,
        Description = "Server maintenance",
        Emergency = true
    },
    new ServiceTicket()
    {
        Id = 3,
        CustomerId = 2,
        EmployeeId = 1,
        Description = "Hardware replacement",
        Emergency = true,
        DateCompleted = DateTime.Today
    },
    new ServiceTicket()
    {
        Id = 4,
        CustomerId = 2,
        EmployeeId = null,
        Description = "Software installation",
        Emergency = false,
        DateCompleted = DateTime.Today.AddMonths(-5)
    },
    new ServiceTicket()
    {
        Id = 5,
        CustomerId = 2,
        EmployeeId = 3,
        Description = "Printer setup",
        Emergency = true
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


app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});


app.MapDelete("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(servticket => servticket.Id == id);
    serviceTickets.Remove(serviceTicket);
    return serviceTicket;
});


app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(servticket => servticket.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});


app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(servticket => servticket.Id == id);
    ticketToComplete.DateCompleted = DateTime.Now;
});


app.MapGet("/servicetickets/incompleteEmergencies", () =>
{
    List<ServiceTicket> incompleteEmergencies = serviceTickets.Where(servticket => !servticket.DateCompleted.HasValue && servticket.Emergency).ToList();
    return Results.Ok(incompleteEmergencies);
});


app.MapGet("/servicetickets/unAssigned", () =>
{
    List<ServiceTicket> unassignedTicket = serviceTickets.Where(servticket => servticket.EmployeeId == null).ToList();
    return Results.Ok(unassignedTicket);
});


app.MapGet("/servicetickets/inactiveCustomers", () =>
{
    DateTime oneYearAgo = DateTime.Now.AddYears(-1);

    List<Customer> oldCustomers = customers.Where(customer =>
    {
        bool hasActiveTicket = serviceTickets.Any(servticket => servticket.CustomerId == customer.Id && servticket.DateCompleted.HasValue && servticket.DateCompleted >= oneYearAgo);
        return !hasActiveTicket;
    }).ToList();

    return Results.Ok(oldCustomers);

});


app.MapGet("/employees/availableEmployees", () =>
{
    List<Employee> availableEmployees = employees.Where(employee =>
        !serviceTickets.Any(servticket =>
            servticket.EmployeeId == employee.Id && servticket.DateCompleted == null)).ToList();

    return Results.Ok(availableEmployees);
});


app.MapGet("/employee/{id}/customers", (int id) =>
{
    var employee = employees.FirstOrDefault(employee => employee.Id == id);

    if (employee == null)
    {
        return Results.NotFound();
    }

    var employeesCustomers = customers.Where(customer => serviceTickets.Any(servticket => servticket.CustomerId == customer.Id && servticket.EmployeeId == id));

    return Results.Ok(employeesCustomers);
});


app.MapGet("/employeeOfTheMonth", () =>
{
    var lastMonth = DateTime.Now.AddMonths(-1);

    var employeeOfTheMonth = employees.OrderByDescending(employee => serviceTickets.Count(servticket => servticket.EmployeeId == employee.Id && servticket.DateCompleted >= lastMonth)).FirstOrDefault();

    return Results.Ok(employeeOfTheMonth);
});


app.MapGet("/completedTickets", () =>
{
    var completedTickets = serviceTickets.Where(servticket => servticket.DateCompleted.HasValue)
        .OrderBy(servticket => servticket.DateCompleted.HasValue).ToList();

    return Results.Ok(completedTickets);
});


app.MapGet("/prioritizedTickets", () =>
{
    var prioritizedTickets = serviceTickets.Where(servticket => servticket.DateCompleted == null).OrderByDescending(servticket => servticket.Emergency).ThenBy(servticket => servticket.EmployeeId == 0).ToList();
    return Results.Ok(prioritizedTickets);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

