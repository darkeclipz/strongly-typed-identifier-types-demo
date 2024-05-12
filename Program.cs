using Microsoft.EntityFrameworkCore;

Trade trade = Trade.CreateNew();
trade.AddOrder(1m, 55000m);

using var db = new TradeDbContext();
// db.Trades.Add(trade);
// db.SaveChanges();

// Console.WriteLine($"Trade created {trade.Id.Value}.");

var tradeId = new TradeId(Guid.Parse("a849289f-c5c5-434c-84eb-ab8aae2e1f86"));
var existingTrade = db.Trades.Include(t => t.Orders).Single(t => t.Id == tradeId);

Console.WriteLine($"Trade retrieved: {existingTrade.Id}");
Console.WriteLine($"Trade has {existingTrade.Orders.Count} orders.");


public readonly record struct OrderId(Guid Value)
{
    public static readonly OrderId Empty = new(Guid.Empty);
    public static OrderId CreateNew() => new(Guid.NewGuid());
}

class Order
{
    public OrderId Id { get; init; } = OrderId.Empty;
    // public TradeId TradeId { get; init; } = TradeId.Empty; // Why should we specify this???
    public PositiveDecimal Quantity { get; init; }
    public PositiveDecimal Price { get; init; }
    public DateTime DateCreated { get; init; }

    public virtual Trade? Trade { get; set; }

    public static Order CreateNew(TradeId tradeId, PositiveDecimal quantity, PositiveDecimal price) => new()
    {
        Id = OrderId.CreateNew(),
        // TradeId = tradeId,
        Quantity = quantity,
        Price = price,
    };

    public override string ToString()
    {
        return $"Order(Id = {Id.Value}, Quantity = {Quantity.Value:N2}, Price = {Price.Value:N2})";
    }
}

readonly record struct Money(decimal Value, string Currency)
{
    public decimal Value { get; init; } = Value >= 0m ? Value
        : throw new ArgumentException("Value must be positive.");

    public string Currency { get; init; } = !string.IsNullOrEmpty(Currency) ? Currency
        : throw new ArgumentException("Currency required.");

    public static explicit operator decimal(Money value) => value.Value;
    public static Money operator +(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        if (left.Currency != right.Currency)
            throw new ArgumentException("Unable to add money of different currencies.");
        return new Money(left.Value + right.Value, left.Currency);
    }
}

readonly record struct PositiveDecimal(decimal Value)
{
    public decimal Value { get; init; } =
        Value >= 0m ? Value
        : throw new ArgumentException("Value must be positive.");

    public static PositiveDecimal CreateNew(decimal value) => new(value);
    public static implicit operator decimal(PositiveDecimal value) => value.Value;
    public static implicit operator PositiveDecimal(decimal value) => new(value);
}

public readonly record struct TradeId(Guid Value)
{
    public static readonly TradeId Empty = new(Guid.Empty);
    public static TradeId CreateNew() => new(Guid.NewGuid());
}

class Trade
{
    public TradeId Id { get; init; } = TradeId.Empty;

    // Otherwise, if the navigation is exposed as an IEnumerable<T>, an ICollection<T>, or an ISet<T>, then 
    // an instance of HashSet<T> using ReferenceEqualityComparer is created.
    private ICollection<Order>? _orders;
    public ICollection<Order> Orders => _orders ??= [];

    public static Trade CreateNew() => new()
    {
        Id = TradeId.CreateNew()
    };

    public void AddOrder(PositiveDecimal quantity, PositiveDecimal price)
    {
        var order = Order.CreateNew(Id, quantity, price);
        Orders.Add(order);
    }
}

class TradeDbContext : DbContext
{
    public DbSet<Trade> Trades { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Initial Catalog=trade-db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trade>()
            .Property(trade => trade.Id)
            .HasConversion(id => id.Value, value => new(value))
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Order>()
            .Property(order => order.Id)
            .HasConversion(id => id.Value, value => new(value))
            .ValueGeneratedOnAdd();

        // modelBuilder.Entity<Order>()
        //     .Property(order => order.TradeId)
        //     .HasConversion(id => id.Value, value => new(value));

        modelBuilder.Entity<Order>()
            .Property(order => order.Quantity)
            .HasConversion(quantity => quantity.Value, value => new(value));

        modelBuilder.Entity<Order>()
            .Property(order => order.Price)
            .HasConversion(price => price.Value, value => new(value));

        modelBuilder.Entity<Order>()
            .HasOne(t => t.Trade)
            .WithMany(o => o.Orders);
    }
}
