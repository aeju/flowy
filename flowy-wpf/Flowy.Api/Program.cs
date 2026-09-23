using Flowy.Core.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EventAnalyzer 등록 (WPF와 같은 flowy.db를 읽기 전용으로 공유)
// AppContext.BaseDirectory 기준으로 DB 경로를 잡아 어느 환경에서도 실행 폴더의 flowy.db를 찾음
// (EventRepository와 동일 방식, 상대 경로 "flowy.db"는 CurrentDirectory 기준이라 실행 시 위치가 달라져 실패)
var dbPath = Path.Combine(AppContext.BaseDirectory, "flowy.db");
var connectionString = $"Data Source={dbPath};Mode=ReadOnly";
builder.Services.AddSingleton(new EventAnalyzer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
