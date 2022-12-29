using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["Database:SqlServer"]);
var app = builder.Build();

var configuration = app.Configuration;
ProductRepository.Init(configuration);


app.MapPost("/product", (ProductRequest productRequest,ApplicationDbContext context) =>{
    var category = context.Categories.Where(c=> c.Id == productRequest.CategoryId).First();
    var product = new Product { 
        Code = productRequest.Code,
        Name = productRequest.Name,
        Description = productRequest.Description,
        Category = category
    };
    if (productRequest.Tags != null)
    {
        product.Tags = new List<Tag>();
        foreach (var item in productRequest.Tags)
        {
            product.Tags.Add(new Tag{Name = item});
        }
    }
    context.Products.Add(product);
    context.SaveChanges();
    return Results.Created($"/product/{product.Id}", product.Id);
});


app.MapGet("/product/{id}", ([FromRoute]int id,ApplicationDbContext context) =>{
   var product = context.Products
        .Include(p => p.Category)
        .Include(p => p.Tags)
        .Where(p=> p.Id == id).First();
    if(product != null)
        return Results.Ok(product);
    return Results.NotFound();    
});

app.MapPut("/product/{id}", ([FromRoute]int id,ProductRequest productRequest,ApplicationDbContext context) =>{
    var product = context.Products
        .Include(p => p.Tags)
        .Where(p=> p.Id == id).First();
    
    var category = context.Categories.Where(c=> c.Id == productRequest.CategoryId).First();
    
    product.Code = productRequest.Code;
    product.Name = productRequest.Name;
    product.Description = productRequest.Description;
    product.Category = category;

    if (productRequest.Tags != null)
    {
        product.Tags = new List<Tag>();
        foreach (var item in productRequest.Tags)
        {
            product.Tags.Add(new Tag{Name = item});
        }
    }
    context.SaveChanges();
    return Results.Ok();
});

app.MapDelete("/product/{id}", ([FromRoute] int id, ApplicationDbContext context) =>{
    var product = context.Products.Where(p=> p.Id == id).First();
    context.Products.Remove(product);
    context.SaveChanges();
    return Results.Ok();
});


app.Run();
