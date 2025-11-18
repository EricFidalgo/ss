var builder = WebApplication.CreateBuilder(args);

// --- FIX 1: Add Controller support ---
builder.Services.AddControllers();
// -------------------------------------

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// --- FIX 2: Map the Controllers ---
app.MapControllers();
// ----------------------------------

app.Run();